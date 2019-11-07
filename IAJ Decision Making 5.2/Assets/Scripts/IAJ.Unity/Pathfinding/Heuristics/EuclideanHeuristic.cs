using System;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float H(Vector3 node, Vector3 goalNode)
        {
            float xArgument = (float)Math.Pow((goalNode.x - node.x), 2);
            float zArgument = (float)Math.Pow((goalNode.z - node.z), 2);
            return Mathf.Sqrt(xArgument + zArgument);
        }
    }
}
