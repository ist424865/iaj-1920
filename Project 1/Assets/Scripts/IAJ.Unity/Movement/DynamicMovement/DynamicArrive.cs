using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;
namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement {
    public class DynamicArrive : DynamicVelocityMatch
    {
        public float StopRadius { get; set; }
        public float SlowRadius { get; set; }
        public float MaxSpeed { get; set; }
        public KinematicData DestinationTarget { get; set; }

        public DynamicArrive()
        {
            base.Target = new KinematicData();
        }
        public override string Name
        {
            get { return "Arrive"; }
        }

        public override MovementOutput GetMovement()
        {
            Vector3 direction = this.DestinationTarget.Position - this.Character.Position;
            float distance = direction.magnitude;

            float desiredSpeed = 0;

            if (distance < this.StopRadius) {
                desiredSpeed = 0;
            }
            else if (distance > this.SlowRadius) {
                desiredSpeed = this.MaxSpeed;
            }
            else {
                desiredSpeed = this.MaxSpeed * (distance/this.SlowRadius);
            }
            direction.Normalize();
            base.Target.velocity = direction * desiredSpeed;

            return base.GetMovement();
        }
    }
}
