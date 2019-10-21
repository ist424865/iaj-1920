using System.Collections.Generic;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures
{
    public class Cluster : ScriptableObject
    {
        public Vector3 center;
        public Vector3 min;
        public Vector3 max;
        public List<Gateway> gateways;

        public Cluster()
        {
            this.gateways = new List<Gateway>();
        }

        public void Initialize(GameObject clusterObject)
        {
            this.center = clusterObject.transform.position;
            //clusters have a size of 10 multipled by the scale
            this.min = new Vector3(this.center.x - clusterObject.transform.localScale.x * 10/2, 0, this.center.z - clusterObject.transform.localScale.z * 10/2);
            this.max = new Vector3(this.center.x + clusterObject.transform.localScale.x * 10/2, 0, this.center.z + clusterObject.transform.localScale.z * 10/2);
        }

        public bool Contains(Vector3 position)
        {
            // The +- 2 is a correction for margin error
            if     (position.x >= min.x - 2 && position.x <= max.x + 2)
                if (position.z >= min.z - 2 && position.z <= max.z + 2)
                {
                    return true;
                }
            return false;
        }

        public Vector3 Localize()
        {
            return this.center;
        }
    }
}
