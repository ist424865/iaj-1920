using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.Graph;
using UnityEngine;
using System;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Path
{
    public class GlobalPath : Path
    {
        public List<NavigationGraphNode> PathNodes { get; protected set; }
        public List<Vector3> PathPositions { get; protected set; } 
        public bool IsPartial { get; set; }
        public float Length { get; set; }
        public List<LocalPath> LocalPaths { get; protected set; } 


        public GlobalPath()
        {
            this.PathNodes = new List<NavigationGraphNode>();
            this.PathPositions = new List<Vector3>();
            this.LocalPaths = new List<LocalPath>();
        }

        public void ProcessLocalPaths()
        {
            for (int i = 0; i < this.PathPositions.Count - 1; i++)
            {
                var localPath = new LineSegmentPath(this.PathPositions[i], this.PathPositions[i + 1]);
                this.LocalPaths.Add(localPath);
            }
        }

        public override float GetParam(Vector3 position, float previousParam)
        {
            // Local path index is the decimal part (2.45 represents local path number 3)
            int localIndex = (int) Math.Floor(previousParam);
            var localParam = previousParam - localIndex;

            var localPath = this.LocalPaths[localIndex];

            return localPath.GetParam(position, localParam);
        }

        public override Vector3 GetPosition(float param)
        {
            // Local path index is the decimal part (2.45 represents local path 3)
            int localIndex = (int) Math.Floor(param);
            var localPath = this.LocalPaths[localIndex];

            return localPath.GetPosition(param - localIndex);
        }

        public override bool PathEnd(float param)
        {
            // Local path index is the decimal part (2.45 represents local path 3)
            int localIndex = (int)Math.Floor(param);
            var localPath = this.LocalPaths[localIndex];

            return localPath.PathEnd(param - localIndex);
        }

        public float GetLocalPathOffset(float param)
        {
            // Local path index is the decimal part (2.45 represents local path 3)
            int localIndex = (int)Math.Floor(param);
            return this.LocalPaths[localIndex].GetOffset(param);
        }
    }
}
