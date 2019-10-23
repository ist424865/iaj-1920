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
        public const int MAXTHREHOLD = 100;
        public const int CHARACTER_TYPE = 0;
        public const int OBSTACLE_TYPE = 1;
        public override string Name
        {
            get { return "RVO"; }
        }

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }
        protected List<KinematicData> Characters { get; set; }
        protected List<StaticData> Obstacles { get; set; }
        public List<Vector3> Samples { get; set; }
        public int NumSamples { get; set; }
        public float CharacterSize { get; set; }
        public float ObstacleSize { get; set; }
        public float IgnoreDistance { get; set; }
        public float MaxSpeed { get; set; }
        public float CharacterWeight { get; set; }
        public float ObstacleWeight { get; set; }
        public float GoodEnoughPenalty { get; set; }
        public Vector3 LastSample { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<StaticData> obstacles)
        {
            this.DesiredMovement = goalMovement;
            this.Characters = movingCharacters;
            this.Obstacles = obstacles;
            this.IgnoreDistance = 7.5f;
            this.CharacterSize = 1.2f;
            this.ObstacleSize = 1.6f;
            this.NumSamples = 10;
            this.CharacterWeight = 5.0f;
            this.ObstacleWeight = 6.6f;
            this.LastSample = Vector3.zero;
            this.GoodEnoughPenalty = 0.5f;
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
            this.Samples = new List<Vector3>();
            this.Samples.Add(desiredVelocity); // desired movement
            this.Samples.Add(this.LastSample); // add last sample (maybe best sample)

            for (int i = 0; i < this.NumSamples; i++)
            {
                // generate random angle and magnitude for sample
                float angle = Random.Range(0f, MathConstants.MATH_2PI);
                float magnitude = Random.Range(this.MaxSpeed / 2, this.MaxSpeed);

                Vector3 velocitySample = MathHelper.ConvertOrientationToVector(angle) * magnitude;
                Samples.Add(velocitySample);
            }

            // 3 - evaluate and get the best sample
            base.Target.velocity = this.GetBestSample(desiredVelocity);

            return base.GetMovement();
        }

        // Returns best sample that follows desired movement and avoids collisions
        public Vector3 GetBestSample(Vector3 desiredVelocity)
        {
            Vector3 bestSample = Vector3.zero;
            float minimumPenalty = Mathf.Infinity;

            foreach (var sample in this.Samples)
            {
                float distancePenalty = (desiredVelocity - sample).magnitude;
                float maximumTimePenalty = 0;

                // characters
                foreach (var otherCharacter in this.Characters)
                {
                    // remove this character from collision avoidance
                    if (otherCharacter == this.Character) continue;

                    // verify if otherCharacter is close enough
                    Vector3 deltaPos = otherCharacter.Position - this.Character.Position;
                    if (deltaPos.magnitude > this.IgnoreDistance)
                        continue;

                    // test collision of the ray with the circle
                    Vector3 rayVector = 2 * sample - this.Character.velocity - otherCharacter.velocity;
                    float timeToCollision = MathHelper.TimeToCollisionBetweenRayAndCircle(this.Character.Position, rayVector, otherCharacter.Position, 2 * this.CharacterSize);

                    // get time penalty of choosing this sample
                    float timePenalty = TimePenalty(timeToCollision, CHARACTER_TYPE);

                    // update max time penalty
                    if (timePenalty > maximumTimePenalty)
                    {
                        maximumTimePenalty = timePenalty;
                        
                    }

                }
                // OPTIMIZATION  penalty already too large skip this sample
                if (maximumTimePenalty > MAXTHREHOLD) continue;


                // obstacles
                foreach (var obstacle in this.Obstacles)
                {
                    //Vector3 deltaPos = obstacle.Position - this.Character.Position;
                    //if (deltaPos.magnitude > this.IgnoreDistance)
                    //    continue;

                    Vector3 rayVector = sample;
                    float timeToCollision = MathHelper.TimeToCollisionBetweenRayAndCircle(this.Character.Position, rayVector, obstacle.Position, 2 * this.ObstacleSize);

                    // get time penalty of choosing this sample
                    float timePenalty = TimePenalty(timeToCollision, OBSTACLE_TYPE);

                    // update max time penalty
                    if (timePenalty > maximumTimePenalty) maximumTimePenalty = timePenalty;

                }

                float penalty = distancePenalty + maximumTimePenalty;

                if (penalty < minimumPenalty)
                {
                    minimumPenalty = penalty;
                    bestSample = sample;
                }

                // OPTIMIZATION good enough penalty sample already found
                if (minimumPenalty <= this.GoodEnoughPenalty) break;


            }           
            Debug.DrawLine(Character.Position, Character.Position + bestSample, Color.magenta);
            this.LastSample = bestSample;
            return bestSample;
        }

        // Calculate time penalty depending of time to collision
        private float TimePenalty(float timeToCollision, int objectType)
        {
            float timePenalty;

            // future collision
            if (timeToCollision > 0.00001f)
            {
                if (objectType == 0) timePenalty = this.CharacterWeight / timeToCollision;
                else                 timePenalty = this.ObstacleWeight / timeToCollision;

            }
            // immediate collision
            else if (System.Math.Abs(timeToCollision) < 0.00001f)
            {
                timePenalty = Mathf.Infinity;
            }
            // no collision
            else
            {
                timePenalty = 0;
            }

            return timePenalty;

        }
    }
}
