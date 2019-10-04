using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{

    public class DynamicWander : DynamicSeek
    {
        public float WanderOffset { get; set; }
        public float WanderRadius { get; set; }
        public float WanderRate { get; set; }
        public float WanderAngle { get; protected set; }

        public Vector3 CircleCenter { get; private set; }

        public GameObject DebugTarget { get; set; }

        public DynamicWander()
        {
            this.Target = new KinematicData();
            this.WanderAngle = 5;
            this.WanderRadius = 5.0f;
        }

        public override string Name
        {
            get { return "Wander"; }
        }


        public override MovementOutput GetMovement()
        {
            this.WanderAngle += RandomHelper.RandomBinomial() * this.WanderRate;
            this.Target.Orientation = this.WanderAngle + this.Character.Orientation;

            this.CircleCenter = this.Character.Position + this.WanderOffset * this.Character.GetOrientationAsVector();
            
            // Set the base target position (seek)
            base.Target.Position = this.CircleCenter + this.WanderRadius * this.Target.GetOrientationAsVector();

            if(this.DebugTarget != null)
            {
                this.DebugTarget.transform.position = this.Target.Position;
                
            }
            return base.GetMovement();
        }
    }
}
