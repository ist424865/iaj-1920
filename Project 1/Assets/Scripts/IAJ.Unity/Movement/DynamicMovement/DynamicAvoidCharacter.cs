using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidCharacter : DynamicMovement
    {

        public float CollisionRadius { get; set; }
        public float MaxTimeLookAhead { get; set; }
        public override string Name
        {
            get { return "AvoidCharacter"; }
        }

        public DynamicAvoidCharacter(KinematicData otherCharacter)
        {
            this.Target = otherCharacter;
            this.Output = new MovementOutput();
        }

        public override MovementOutput GetMovement()
        {
            MovementOutput output = new MovementOutput();
            output.Clear();

            Vector3 deltaPos = this.Target.Position - this.Character.Position;
            Vector3 deltaVel = this.Target.velocity - this.Character.velocity;
            float deltaSqrSpeed = deltaVel.sqrMagnitude;

            if (deltaSqrSpeed < 0.001f) {
                return output; // empty output
            }

            float timeToClosest = -Vector3.Dot(deltaPos, deltaVel) / deltaSqrSpeed;

            if (timeToClosest > this.MaxTimeLookAhead) {
                return output; // empty output
            }

            Vector3 futureDeltaPos = deltaPos + deltaVel * timeToClosest;
            float futureDistance = futureDeltaPos.magnitude;

            if (futureDistance > 2 * this.CollisionRadius) {
                return output; // empty output
            }
            
            if (futureDistance <= 0 || deltaPos.magnitude < 2 * this.CollisionRadius) {
                this.Output.linear = this.Character.Position - this.Target.Position;
            }
            else {
                this.Output.linear = futureDeltaPos * -1;
            }

            if (this.Output.linear.sqrMagnitude > 0) 
            {
                this.Output.linear.Normalize();
                this.Output.linear *= this.MaxAcceleration;
            }

            return this.Output;
        }
    }
}
