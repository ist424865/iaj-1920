using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class AStarPathfinding
    {
        public NavMeshPathGraph NavMeshGraph { get; protected set; }
        //how many nodes do we process on each call to the search method (assuming this method will be called every frame when there is a pathfinding process active)
        public uint NodesPerSearch { get; set; }

        //properties used for debugging purposes
        public uint TotalExploredNodes { get; protected set; }
        public int MaxOpenNodes { get; protected set; }
        public float TotalProcessingTime { get; protected set; }
        public float StartTime { get; protected set; }

        public bool InProgress { get; set; }

        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }

        public NavigationGraphNode GoalNode { get; protected set; }
        public NavigationGraphNode StartNode { get; protected set; }
        public Vector3 StartPosition { get; protected set; }
        public Vector3 GoalPosition { get; protected set; }

        //heuristic function
        public IHeuristic Heuristic { get; protected set; }

        public AStarPathfinding(NavMeshPathGraph graph, IOpenSet open, IClosedSet closed, IHeuristic heuristic)
        {
            this.NavMeshGraph = graph;
            this.Open = open;
            this.Closed = closed;
            this.InProgress = false;
            this.Heuristic = heuristic;
        }

        public virtual void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition)
        {
            this.StartTime = Time.time;
            this.StartPosition = startPosition;
            this.GoalPosition = goalPosition;
            this.StartNode = this.Quantize(this.StartPosition);
            this.GoalNode = this.Quantize(this.GoalPosition);

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            //I need to do this because in Recast NavMesh graph, the edges of polygons are considered to be nodes and not the connections.
            //Theoretically the Quantize method should then return the appropriate edge, but instead it returns a polygon
            //Therefore, we need to create one explicit connection between the polygon and each edge of the corresponding polygon for the search algorithm to work
            ((NavMeshPoly)this.StartNode).AddConnectedPoly(this.StartPosition);
            ((NavMeshPoly)this.GoalNode).AddConnectedPoly(this.GoalPosition);

            this.InProgress = true;
            this.TotalExploredNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            var initialNode = new NodeRecord
            {
                gValue = 0,
                hValue = this.Heuristic.H(this.StartNode.LocalPosition, this.GoalNode.LocalPosition),
                node = this.StartNode
            };

            initialNode.fValue = AStarPathfinding.F(initialNode);

            this.Open.Initialize(); 
            this.Open.AddToOpen(initialNode);
            this.Closed.Initialize();
        }

        protected virtual void ProcessChildNode(NodeRecord parentNode, NavigationGraphEdge connectionEdge, int edgeIndex)
        {
            // generate child node from connection edge
            NodeRecord childNode = GenerateChildNodeRecord(parentNode, connectionEdge);

            // get child node from Open and Closed sets
            NodeRecord childInOpen = this.Open.SearchInOpen(childNode);
            NodeRecord childInClosed = this.Closed.SearchInClosed(childNode);

            // if child node is not in open nor in closed
            if (childInOpen == null && childInClosed == null)
            {
                this.Open.AddToOpen(childNode);
            }
            // if child node is in open with higher F-value
            else if (childInOpen != null && childInOpen.fValue >= childNode.fValue) // solve ties by ranking better new nodes
            {
                this.Open.Replace(childInOpen, childNode);
            }
            // if child is in closed with higher F-value
            else if (childInClosed != null && childInClosed.fValue > childNode.fValue)
            {
                this.Closed.RemoveFromClosed(childInClosed);
                this.Open.AddToOpen(childNode);
                this.TotalExploredNodes--;
            }
        }

        public bool Search(out GlobalPath solution, bool returnPartialSolution = false)
        {
            NodeRecord currentNode = null;

            // iterate NodesPerFrame times
            for (int count = 0; count <= this.NodesPerSearch; count++)
            {
                // cannot reach solution node
                if (this.Open.CountOpen() == 0)
                {
                    solution = null;
                    CleanUp();
                    return true;
                }

                // update max open nodes
                int openCount = this.Open.CountOpen();
                if (this.MaxOpenNodes < openCount)
                    this.MaxOpenNodes = openCount;

                // get node with lowest F (best node)
                currentNode = this.Open.GetBestAndRemove();
                this.TotalExploredNodes++;

                // reach solution node
                if (currentNode.node == this.GoalNode)
                {
                    solution = this.CalculateSolution(currentNode, false); // solution is complete
                    this.TotalProcessingTime = Time.time - this.StartTime;
                    CleanUp();
                    return true;
                }

                // start processing current node
                this.Closed.AddToClosed(currentNode);

                // iterate over nodes connected to current node
                var outConnections = currentNode.node.OutEdgeCount;
                for (int i = 0; i < outConnections; i++)
                {
                    this.ProcessChildNode(currentNode, currentNode.node.EdgeOut(i), i);
                }

            }

            // calculate partial solution if asked
            if (returnPartialSolution)
            {
                solution = this.CalculateSolution(currentNode, true);
                this.TotalProcessingTime = Time.time - this.StartTime;
            }
            else
                solution = null;

            // search did not finished yet
            return false;
        }

        protected NavigationGraphNode Quantize(Vector3 position)
        {
            return this.NavMeshGraph.QuantizeToNode(position, 1.0f);
        }

        protected void CleanUp()
        {
            //I need to remove the connections created in the initialization process
            if (this.StartNode != null)
            {
                ((NavMeshPoly)this.StartNode).RemoveConnectedPoly();
            }

            if (this.GoalNode != null)
            {
                ((NavMeshPoly)this.GoalNode).RemoveConnectedPoly();    
            }
        }

        protected virtual NodeRecord GenerateChildNodeRecord(NodeRecord parent, NavigationGraphEdge connectionEdge)
        {
            var childNode = connectionEdge.ToNode;
            var childNodeRecord = new NodeRecord
            {
                node = childNode,
                parent = parent,
                gValue = parent.gValue + (childNode.LocalPosition-parent.node.LocalPosition).magnitude,
                hValue = this.Heuristic.H(childNode.LocalPosition, this.GoalNode.LocalPosition)
            };

            childNodeRecord.fValue = F(childNodeRecord);

            return childNodeRecord;
        }

        protected GlobalPath CalculateSolution(NodeRecord node, bool partial)
        {
            var path = new GlobalPath
            {
                IsPartial = partial,
                Length = node.gValue
            };
            var currentNode = node;

            path.PathPositions.Add(this.GoalPosition);

            //I need to remove the first Node and the last Node because they correspond to the dummy first and last Polygons that were created by the initialization.
            //And we don't want to be forced to go to the center of the initial polygon before starting to move towards my destination.

            //skip the last node, but only if the solution is not partial (if the solution is partial, the last node does not correspond to the dummy goal polygon)
            if (!partial && currentNode.parent != null)
            {
                currentNode = currentNode.parent;
            }
            
            while (currentNode.parent != null)
            {
                path.PathNodes.Add(currentNode.node); //we need to reverse the list because this operator add elements to the end of the list
                path.PathPositions.Add(currentNode.node.LocalPosition);

                if (currentNode.parent.parent == null) break; //this skips the first node
                currentNode = currentNode.parent;
            }

            path.PathNodes.Reverse();
            path.PathPositions.Reverse();
            // create local paths for solution
            path.CalculateLocalPathsFromPathPositions(this.StartPosition);
            return path;

        }

        public static float F(NodeRecord node)
        {
            return F(node.gValue,node.hValue);
        }

        public static float F(float g, float h)
        {
            return g + h;
        }

    }
}
