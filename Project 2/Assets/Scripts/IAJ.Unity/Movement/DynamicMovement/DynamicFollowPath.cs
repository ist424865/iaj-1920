using System;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicFollowPath : DynamicArrive
    {
        public override string Name
        {
            get { return "Follow Path"; }
        }

        public DynamicFollowPath()
        {
            base.DestinationTarget = new KinematicData();
            this.CurrentParam = 0;
            this.Nsegments = 0;
        }

        public GlobalPath Path { get; set; }
        public float PathOffset { get; set; }
        public float CurrentParam { get; set; }
        
        //Number of line segments traversed 
        public int Nsegments { get; set; }

        public override MovementOutput GetMovement()
        {
            this.CurrentParam = this.Path.GetParam(this.Character.Position, this.CurrentParam);
            if (this.Path.PathEnd(this.CurrentParam))
            {
                //this.CurrentParam = (int) Math.Ceiling(this.CurrentParam);
                this.PathOffset = 1;
                this.Nsegments++;
                Path.ReplaceSegment(Nsegments);
            }
            else
            {
                this.PathOffset = this.Path.GetLocalPathOffset(this.CurrentParam);
            }

            var targetParam = this.CurrentParam + this.PathOffset;

            //Debug.Log("targetParam:" +targetParam);
            base.DestinationTarget.Position = this.Path.GetPosition(targetParam);
            return base.GetMovement();
        }
    }
}
