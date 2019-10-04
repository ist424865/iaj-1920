using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Movement.Arbitration;
using Assets.Scripts.IAJ.Unity.Movement.VO;
using Assets.Scripts.IAJ.Unity.Movement;

public class MainCharacterController : MonoBehaviour {

    public const float X_WORLD_SIZE = 55;
    public const float Z_WORLD_SIZE = 32.5f;
    private const float MAX_ACCELERATION = 40.0f;
    private const float MAX_SPEED = 20.0f;
    private const float DRAG = 0.1f;
    private const float MAX_LOOK_AHEAD = 7.5f;
    private const float WHISKER_FACTOR = 2;
    private const float MAX_TIME_LOOK_AHEAD = 1.0f;
    private const float COLLISION_RADIUS = 2.0f;
    private const float AVOID_MARGIN = 10.0f;
    private const float STOP_RADIUS = 1.5f;
    private const float SLOW_RADIUS = 12.0f;
    private const float WEIGHT_AVOID_OBSTACLE = 70f;
    private const float WEIGHT_AVOID_CHARACTER = 30f;
    private const float WEIGHT_PATROL = 20f;
    private const int NUM_SAMPLES = 10;
    private const float RVO_WEIGHT = 20;


    public KeyCode stopKey = KeyCode.S;
    public KeyCode priorityKey = KeyCode.P;
    public KeyCode blendedKey = KeyCode.B;

    public GameObject movementText;
    public DynamicCharacter character;

    public PriorityMovement priorityMovement;
    public BlendedMovement blendedMovement;
    public RVOMovement rvoMovement;

    private DynamicPatrol patrolMovement;


    //early initialization
    void Awake()
    {
        this.character = new DynamicCharacter(this.gameObject);
    

        this.priorityMovement = new PriorityMovement
        {
            Character = this.character.KinematicData
        };

        this.blendedMovement = new BlendedMovement
        {
            Character = this.character.KinematicData
        };
    }

    // Use this for initialization
    void Start ()
    {
    }

    public void InitializeMovement(GameObject[] obstacles, List<DynamicCharacter> characters)
    {
        foreach (var obstacle in obstacles)
        {
            var avoidObstacleMovement = new DynamicAvoidObstacle(obstacle)
            {
               MaxAcceleration = MAX_ACCELERATION,
               AvoidMargin = AVOID_MARGIN,
               MaxLookAhead = MAX_LOOK_AHEAD,
               Character = this.character.KinematicData,
               DebugColor = Color.magenta,
               WhiskerFactor = WHISKER_FACTOR,
            };
            this.blendedMovement.Movements.Add(new MovementWithWeight(avoidObstacleMovement, WEIGHT_AVOID_OBSTACLE));
            this.priorityMovement.Movements.Add(avoidObstacleMovement);
        }

        foreach (var otherCharacter in characters)
        {
            if (otherCharacter != this.character)
            {
                var avoidCharacter = new DynamicAvoidCharacter(otherCharacter.KinematicData)
                {
                   Character = this.character.KinematicData,
                   MaxAcceleration = MAX_ACCELERATION,
                   CollisionRadius = COLLISION_RADIUS,
                   DebugColor = Color.cyan,
                   MaxTimeLookAhead = MAX_TIME_LOOK_AHEAD
                };

                this.blendedMovement.Movements.Add(new MovementWithWeight(avoidCharacter, WEIGHT_AVOID_CHARACTER));
                this.priorityMovement.Movements.Add(avoidCharacter);
            }
        }

        var targetPosition = this.character.KinematicData.Position + (Vector3.zero - this.character.KinematicData.Position) * 2;

        this.patrolMovement = new DynamicPatrol(this.character.KinematicData.Position, targetPosition)
        {
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            DebugColor = Color.yellow,
            MaxSpeed = MAX_SPEED,
            SlowRadius = SLOW_RADIUS,
            StopRadius = STOP_RADIUS,
        };

        this.rvoMovement = new RVOMovement(this.patrolMovement, characters.Select(c => c.KinematicData).ToList(), obstacles.Select(o => new StaticData(o.transform)).ToList())
        {
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            NumSamples = NUM_SAMPLES,
            AvoidanceWeight = RVO_WEIGHT
        };

        this.priorityMovement.Movements.Add(patrolMovement);
        this.blendedMovement.Movements.Add(new MovementWithWeight(patrolMovement, WEIGHT_PATROL));
        this.character.Movement = this.priorityMovement;
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            this.patrolMovement.ChangeTarget();
        }
        if (Input.GetKeyDown(this.stopKey))
        {
            this.character.Movement = null;
        }
        else if (Input.GetKeyDown(this.blendedKey))
        {
            this.character.Movement = this.blendedMovement;
        }
        else if (Input.GetKeyDown(this.priorityKey))
        {
            this.character.Movement = this.priorityMovement;
        }
        else if(Input.GetKeyDown(KeyCode.R))
        {
            this.character.Movement = this.rvoMovement;
        }

        this.UpdateMovingGameObject();
    }

    void OnDrawGizmos()
    {
    }

    private void UpdateMovingGameObject()
    {
        if (this.character.Movement != null)
        {
            this.character.Update();
            this.character.KinematicData.ApplyWorldLimit(X_WORLD_SIZE, Z_WORLD_SIZE);
        }
    }
}
