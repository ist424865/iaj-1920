using UnityEngine;
using UnityEditor;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;
using RAIN.Navigation.Graph;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System;

public class IAJMenuItems  {

    [MenuItem("IAJ/Create Cluster Graph")]
    private static void CreateClusterGraph()
    {
        Cluster cluster;
        Gateway gateway;

        //get cluster game objects
        var clusters = GameObject.FindGameObjectsWithTag("Cluster");
        //get gateway game objects
        var gateways = GameObject.FindGameObjectsWithTag("Gateway");
        //get the NavMeshGraph from the current scene
        NavMeshPathGraph navMesh = GameObject.Find("Navigation Mesh").GetComponent<NavMeshRig>().NavMesh.Graph;

        ClusterGraph clusterGraph = ScriptableObject.CreateInstance<ClusterGraph>();

        //create gateway instances for each gateway game object
        for(int i = 0; i < gateways.Length; i++)
        {
            var gatewayGO = gateways[i];
            gateway = ScriptableObject.CreateInstance<Gateway>();
            gateway.Initialize(i,gatewayGO);
            clusterGraph.gateways.Add(gateway);
        }

        //create cluster instances for each cluster game object and check for connections through gateways
        foreach (var clusterGO in clusters)
        {

            cluster = ScriptableObject.CreateInstance<Cluster>();
            cluster.Initialize(clusterGO);
            clusterGraph.clusters.Add(cluster);

            //determine intersection between cluster and gateways and add connections when they intersect
            foreach(var gate in clusterGraph.gateways)
            {
                if (MathHelper.BoundingBoxIntersection(cluster.min, cluster.max, gate.min, gate.max))
                {
                    cluster.gateways.Add(gate);
                    gate.clusters.Add(cluster);
                }
            }
        }

        // Second stage of the algorithm, calculation of the Gateway table

        GlobalPath solution = null;
        float cost;
        Gateway startGate;
        Gateway endGate;

        var pathfindingAlgorithm = new NodeArrayAStarPathFinding(navMesh, new EuclideanDistance());

        // initialize gateway distance table
        clusterGraph.gatewayDistanceTable = new GatewayDistanceTableRow[gateways.Length];

        // find shortest distance between all gates
        for (int i = 0; i < clusterGraph.gateways.Count; i++)
        {
            // create row for current start gateway
            GatewayDistanceTableRow gateRow = ScriptableObject.CreateInstance<GatewayDistanceTableRow>();
            gateRow.entries = new GatewayDistanceTableEntry[gateways.Length];
            startGate = clusterGraph.gateways[i];

            // search for all other gateways
            for (int j = 0; j < clusterGraph.gateways.Count; j++)
            {
                // get goal gate
                endGate = clusterGraph.gateways[j];

                // create entry for each end gateway
                GatewayDistanceTableEntry gateEntry = ScriptableObject.CreateInstance<GatewayDistanceTableEntry>();
                gateEntry.startGatewayPosition = startGate.Localize();
                gateEntry.endGatewayPosition = endGate.Localize();

                // if same gateway
                if (i == j)
                {
                    gateEntry.shortestDistance = 0;
                    // add entry to row
                    gateRow.entries[j] = gateEntry;
                    continue;
                }

                // initialize pathfinding algorithm and start the search
                pathfindingAlgorithm.InitializePathfindingSearch(startGate.Localize(), endGate.Localize());
                bool finished = pathfindingAlgorithm.Search(out solution);

                if (finished && solution != null)
                {
                    // length is cost for goal node
                    cost = solution.Length;
                    gateEntry.shortestDistance = cost;
                }
                else
                {
                    // cannot reach cluster
                    gateEntry.shortestDistance = float.MaxValue;
                }
                // add entry to row
                gateRow.entries[j] = gateEntry;
            }
            // add row to gateway table
            clusterGraph.gatewayDistanceTable[i] = gateRow;
        }

        var nodes = GetNodesHack(navMesh);

        // optimization: save cluster of each node to optimize quantize
        clusterGraph.clusterOfNode = new Cluster[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            var nodePosition = nodes[i].LocalPosition;
            for (int j = 0; j < clusterGraph.clusters.Count; j++)
            {
                var currentCluster = clusterGraph.clusters[j];
                if (MathHelper.PointInsideBoundingBox(nodePosition, currentCluster.min, currentCluster.max))
                {
                    clusterGraph.clusterOfNode[nodes[i].NodeIndex] = currentCluster;
                    break;
                }
            }
        }

        //create a new asset that will contain the ClusterGraph and save it to disk (DO NOT REMOVE THIS LINE)
        clusterGraph.SaveToAssetDatabase();
    }

    private static List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
    {
        //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
        //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
        //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
        //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
        //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
        //that's why we're using the type of the base class in the reflection call
        return (List<NavigationGraphNode>)Assets.Scripts.IAJ.Unity.Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
    }
}
