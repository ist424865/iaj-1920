using System;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
        {
            float xArgument = (float)Math.Pow((goalNode.Position.x - node.Position.x), 2);
            float zArgument = (float)Math.Pow((goalNode.Position.z - node.Position.z), 2);
            return Mathf.Sqrt(xArgument + zArgument);
        }
    }
}
