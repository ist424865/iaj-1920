using System;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class GatewayHeuristic : IHeuristic
    {
        public ClusterGraph clusterGraph;

        public GatewayHeuristic()
        {
            this.clusterGraph = Resources.Load<ClusterGraph>("ClusterGraph");
        }

        public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
        {
            Cluster startCluster = clusterGraph.Quantize(node);
            Cluster endCluster = clusterGraph.Quantize(goalNode);

            // if node and goal node are in the same room/cluster
            if (startCluster == endCluster)
            {
                // return a normal distance heuristic (euclidean)
                return this.EuclideanDistance(node.Position, goalNode.Position);
            }
            // if not, we have to cross gateways
            else
            {
                // get gateway set for each node
                var startGateways = startCluster.gateways;
                var endGateways = endCluster.gateways;

                float heuristicValue = float.MaxValue;

                // h(n, g) = min(h'(n, Gi) + H(Gi, Gj) + h'(Gj, g))
                // where:
                // - Gi belongs to StartGateways and Gj to EndGateways
                // - H is the Gateway Table
                // - h' is a normal distance heuristic (euclidean in this case)
                for (int i = 0; i < startGateways.Count; i++)
                {
                    for (int j = 0; j < endGateways.Count; j++)
                    {
                        // H[Gi, Gj] (entry from gateway i to gateway j)
                        // get table row of start gateway index
                        var row = this.clusterGraph.gatewayDistanceTable[startGateways[i].id];
                        // get table entry of end gateway index
                        var entry = row.entries[endGateways[j].id];

                        // current h value
                        float value =
                            // h'(n, Gi) (distance from start node to start gateway)
                            this.EuclideanDistance(node.Position, startGateways[i].center) +
                            // H(Gi, Gj) (distance from gateway i to gateway j)
                            entry.shortestDistance +
                            // h'(Gj, g) (distance from end gateway to end node)
                            this.EuclideanDistance(endGateways[j].center, goalNode.Position);

                        if (value < heuristicValue) heuristicValue = value;
                    }
                }

                return heuristicValue;
            }
        }

        public float EuclideanDistance(Vector3 node, Vector3 goalNode)
        {
            float xArgument = (float)Math.Pow((goalNode.x - node.x), 2);
            float zArgument = (float)Math.Pow((goalNode.z - node.z), 2);
            return Mathf.Sqrt(xArgument + zArgument);
        }
    }
}
