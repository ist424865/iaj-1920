using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding
{
    //The Dijkstra algorithm is similar to the A* but with a couple of differences
    //1) no heuristic function
    //2) it will not stop until the open list is empty
    //3) we dont need to execute the algorithm in multiple steps (because it will be executed offline)
    //4) we don't need to return any path (partial or complete)
    //5) we don't need to do anything when a node is already in closed
    public class GoalBoundsDijkstraMapFlooding
    {
        public NavMeshPathGraph NavMeshGraph { get; protected set; }
        public NavigationGraphNode StartNode { get; protected set; }
        public NodeGoalBounds NodeGoalBounds { get; protected set; }
        protected NodeRecordArray NodeRecordArray { get; set; }

        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }
        
        public GoalBoundsDijkstraMapFlooding(NavMeshPathGraph graph)
        {
            this.NavMeshGraph = graph;
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
        }

        public void Search(NavigationGraphNode startNode, NodeGoalBounds nodeGoalBounds)
        {
            // initialize attributes
            this.StartNode = startNode;
            this.NodeGoalBounds = nodeGoalBounds;

            // get start node record
            var startNodeRecord = this.NodeRecordArray.GetNodeRecord(this.StartNode);

            // initialize open, closed and add initial node to open
            this.Open.Initialize();
            this.Open.AddToOpen(startNodeRecord);
            this.Closed.Initialize();

            // TODO: is this needed?
            // initialize start node out connection for all edges
            var edgeOutConnections = startNode.OutEdgeCount;
            for (int i = 0; i < edgeOutConnections; i++)
            {
                var edgeNode = startNode.EdgeOut(i).ToNode;
                var edgeNodeRecord = this.NodeRecordArray.GetNodeRecord(edgeNode);
                edgeNodeRecord.StartNodeOutConnectionIndex = i;
            }

            // start dijkstra
            while (this.Open.CountOpen() > 0)
            {
                // get best node from open and add to closed
                var currentNode = this.Open.GetBestAndRemove();
                this.Closed.AddToClosed(currentNode);
                // update goal bounds
                this.NodeGoalBounds.connectionBounds[currentNode.StartNodeOutConnectionIndex].UpdateBounds(currentNode.node.Position);

                // process connections as child nodes
                var outConnections = currentNode.node.OutEdgeCount;
                for (int i = 0; i < outConnections; i++)
                {
                    // update goal bounds for the edge it came from (StartNodeOutConnectionIndex)
                    this.ProcessChildNode(currentNode, currentNode.node.EdgeOut(i), currentNode.StartNodeOutConnectionIndex);
                }
            }

        }

       
        protected void ProcessChildNode(NodeRecord parent, NavigationGraphEdge connectionEdge, int connectionIndex)
        {
            // get child node record from connectionEdge
            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            // if already processed, continue
            if (this.Closed.SearchInClosed(childNodeRecord) == null) return;

            // calculate new cost
            var newCost = parent.gValue + (childNode.LocalPosition - parent.node.LocalPosition).magnitude;

            // if already in open set, replace if current cost is lower
            if (this.Open.SearchInOpen(childNodeRecord) != null)
            {
                if (newCost < childNodeRecord.fValue)
                {
                    childNodeRecord.fValue = childNodeRecord.gValue = newCost;
                    childNodeRecord.parent = parent;
                    // update original out connection
                    childNodeRecord.StartNodeOutConnectionIndex = connectionIndex;
                }
            }
            // not processed yet, add to open
            else
            {
                childNodeRecord.fValue = childNodeRecord.gValue = newCost;
                childNodeRecord.parent = parent;
                this.Open.AddToOpen(childNodeRecord);
                // update original out connection
                childNodeRecord.StartNodeOutConnectionIndex = connectionIndex;
            }
        }

        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>)Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }

    }
}
