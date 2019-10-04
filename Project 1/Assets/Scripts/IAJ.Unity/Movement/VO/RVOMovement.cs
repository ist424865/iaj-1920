//class adapted from the HRVO library http://gamma.cs.unc.edu/HRVO/
//adapted to IAJ classes by João Dias

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.VO
{
    public class RVOMovement : DynamicMovement.DynamicVelocityMatch
    {
        public override string Name
        {
            get { return "RVO"; }
        }

        protected List<KinematicData> Characters { get; set; }
        protected List<StaticData> Obstacles { get; set; }
        public float CharacterSize { get; set; }
        public float IgnoreDistance { get; set; }
        public float MaxSpeed { get; set; }
        public List<Vector3> Samples { get; set; }
        public int NumSamples { get; set; }
        public float AvoidanceWeight { get; set; }

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<StaticData> obstacles)
        {
            this.DesiredMovement = goalMovement;
            this.Characters = movingCharacters;
            this.Obstacles = obstacles;
            this.IgnoreDistance = 20.0f;
            this.CharacterSize = 1.0f;
            this.NumSamples = 15;
            this.AvoidanceWeight = 6.0f;
            base.Target = new KinematicData();
        }

        public override MovementOutput GetMovement()
        {
            MovementOutput desiredOutput = this.DesiredMovement.GetMovement();
            // 1 - calculate desired velocity
            // * Time.deltaTime
            Vector3 desiredVelocity = this.Character.velocity + desiredOutput.linear;

            // trim velocity if bigger than maxSpeed
            if (desiredVelocity.magnitude > this.MaxSpeed)
            {
                desiredVelocity.Normalize();
                desiredVelocity *= this.MaxSpeed;
            }

            // 2 - generate samples
            this.Samples = new List<Vector3>
            {
                desiredVelocity
            };
            for (int i = 0; i < this.NumSamples; i++)
            {
                float angle = Random.Range(0f, MathConstants.MATH_2PI);
                float magnitude = Random.Range(this.MaxSpeed / 2, this.MaxSpeed);
                Vector3 velocitySample = MathHelper.ConvertOrientationToVector(angle) * magnitude;
                Samples.Add(velocitySample);
            }

            // 3 - evaluate and get the best sample
            base.Target.velocity = this.GetBestSample(desiredVelocity);

            return base.GetMovement();
        }

        public Vector3 GetBestSample(Vector3 desiredVelocity)
        {
            Vector3 bestSample = Vector3.zero;
            float minimumPenalty = Mathf.Infinity;
            float timePenalty;

            foreach (var sample in this.Samples)
            {
                float distancePenalty = (desiredVelocity - sample).magnitude;
                float maximumTimePenalty = 0;

                foreach (var otherCharacter in this.Characters)
                {
                    if (otherCharacter == this.Character) continue;

                    Vector3 deltaPos = otherCharacter.Position - this.Character.Position;
                    if (deltaPos.magnitude > this.IgnoreDistance)
                        continue;

                    Vector3 rayVector = 2 * sample - this.Character.velocity - otherCharacter.velocity;
                    float timeToCollision = MathHelper.TimeToCollisionBetweenRayAndCircle(this.Character.Position, rayVector, otherCharacter.Position, 2 * this.CharacterSize);

                    // no collision if not changed

                    // future collision
                    if (timeToCollision > 0.001f)
                    {
                        timePenalty = this.AvoidanceWeight / timeToCollision;
                    }
                    // immediate collision
                    else if (System.Math.Abs(timeToCollision) < 0.001f)
                    {
                        timePenalty = Mathf.Infinity;
                    }
                    else
                    {
                        timePenalty = 0;
                    }

                    if (timePenalty > maximumTimePenalty)
                    {
                        maximumTimePenalty = timePenalty;
                    }

                }

                float penalty = distancePenalty + maximumTimePenalty;

                if (penalty < minimumPenalty)
                {
                    minimumPenalty = penalty;
                    bestSample = sample;
                }
            }
            Debug.DrawLine(Character.Position, Character.Position + bestSample, Color.magenta);
            return bestSample;
        }
    }
}
