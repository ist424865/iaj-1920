using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidObstacle : DynamicSeek
    {
        public override string Name
        {
            get { return "AvoidObstacle"; }
        }

        public GameObject obstacleObject { get; set; }
        public Collider collider { get; set; }
        public float MaxLookAhead { get; set; }
        public float WhiskerFactor { get; set; }
        public float AvoidMargin { get; set; }

        public DynamicAvoidObstacle(GameObject Object)
        {
            this.obstacleObject = Object;
            this.collider = Object.GetComponent<Collider>();
            this.Target = new KinematicData();
        }

        public override MovementOutput GetMovement()
        {   
            if (Random.Range(0, 3) == 0) {
                this.collider = this.obstacleObject.GetComponent<Collider>();
            }

            Vector3 characterOrientation = this.Character.GetOrientationAsVector();
            Vector3 characterPosititon = this.Character.Position;
            RaycastHit centralHit, leftHit, rightHit;
            bool hasCenterCollided, hasLeftCollided, hasRightCollided;

            // Central ray
            Ray centerRayVector = new Ray(characterPosititon, characterOrientation);
            hasCenterCollided = this.collider.Raycast(centerRayVector, out centralHit, this.MaxLookAhead);


            // Left whisker ray
            Vector3 leftOrientation = Quaternion.AngleAxis(-30, Vector3.up) * characterOrientation;
            Ray leftRayVector = new Ray(characterPosititon, leftOrientation);
            hasLeftCollided = this.collider.Raycast(leftRayVector, out leftHit, this.MaxLookAhead / this.WhiskerFactor);

            
            // Right whisker ray
            Vector3 rightOrientation = Quaternion.AngleAxis(30, Vector3.up) * characterOrientation;
            Ray rightRayVector = new Ray(characterPosititon, rightOrientation);
            hasRightCollided = this.collider.Raycast(rightRayVector, out rightHit, this.MaxLookAhead / this.WhiskerFactor);

            
            // Gizmos
            Debug.DrawLine(characterPosititon, characterPosititon + characterOrientation * this.MaxLookAhead, this.DebugColor);
            Debug.DrawLine(characterPosititon, characterPosititon + leftOrientation * this.MaxLookAhead / this.WhiskerFactor, this.DebugColor);
            Debug.DrawLine(characterPosititon, characterPosititon + rightOrientation * this.MaxLookAhead / this.WhiskerFactor, this.DebugColor);


            if (hasCenterCollided) {
                base.Target.Position = centralHit.point + centralHit.normal * this.AvoidMargin;
            }
            else if (hasLeftCollided) {
                base.Target.Position = leftHit.point + leftHit.normal * this.AvoidMargin;
            }
            else if (hasRightCollided) {
                base.Target.Position = rightHit.point + rightHit.normal * this.AvoidMargin;
            }
            else {
                return new MovementOutput(); // empty output
            }

            return base.GetMovement();
        }
    }
}