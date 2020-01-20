using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Threading;
using System.Runtime.CompilerServices;

public class Snake : Agent
{
    public const float DEFAULT_HEIGHT = 0.0f;

    // Snake body list
    public List<GameObject> BodyParts = new List<GameObject>();

    // Snake body part prefab
    public GameObject bodyprefab;

    // Minimum distance between body parts
    public float minDistance = 1f;
    public float speed = 0;
    public float rotationSpeed = 0.5f;
    // Number of initial body parts
    public int beginSize = 5;

    // Number of apples
    public int appleNum = 1;


    //Display
    public GameObject pointsDisplay;

    //Speed Variables
    public int SPEED = 3;
    private Vector3 speedVector;

    //Body
    Rigidbody rigidBody;
    public int MAX_BODY = 100;
    private int bodySize = 0;
    public GameObject snake_body;
    private GameObject[] body;

    //Position of the head a few frames before
    private Vector3 old;

    //Selected Apple
    private GameObject appleGoal;

    //Frame counter
    private long frames = 0;

    private bool collided = false;


    // Start is called before the first frame update
    void Start()
    {
        GetApples();
        /*rigidBody = GetComponent<Rigidbody>();
        Application.targetFrameRate = 30;
        body = new GameObject[MAX_BODY];
        speedVector = new Vector3(0.0f, 0.0f, SPEED);*/
        CreateSnakeBody();
    }

    // Update is called once per frame
    void Update()
    {

        //Move();

        if (Input.GetKey(KeyCode.Q))
        {
            AddBodyPart();
        }
        /*frames++;*/
        //pointsDisplay.GetComponent<UnityEngine.UI.Text>().text = "Score : " + bodySize;

        /*if (Input.GetKeyDown("w"))
        {
            speedVector = new Vector3(0.0f, 0.0f, SPEED);

        }

        else if (Input.GetKeyDown("s"))
        {
            speedVector = new Vector3(0.0f, 0.0f, -SPEED);

        }

        else if (Input.GetKeyDown("a"))
        {
            speedVector = new Vector3(-SPEED, 0.0f, 0.0f);

        }

        else if (Input.GetKeyDown("d"))
        {
            speedVector = new Vector3(SPEED, 0.0f, 0.0f);

        }


        //Perform Movement on the head (For Manual testing)
        //transform.Translate(speedVector * Time.deltaTime);

        //Every 4 frames update the body(Higher refresh rate will cause bad behaviour)
        if (frames % 4 == 0)
        {
            if (bodySize > 1)
            {
                for (int i = 0; i < bodySize - 1; i++)
                {

                    //Move body piece relative to the previous
                    body[i].transform.position = body[i + 1].transform.position;

                }
                //Move last piece relative to the head
                body[bodySize - 1].transform.position = old;
            }

            if (bodySize == 1)
            {
                body[0].transform.position = old;
            }
        }

        //Wait for the body Piece to be in the back
        if (frames % 30 == 0)
        {
            for (int i = 0; i < bodySize - 3; i++)
            {
                body[i].tag = "wall";
            }
        }
        //Position of the head
        old = gameObject.transform.position;*/


    }


    // Movement of the snake
    public void Move(float agentAction = 0.0f)
    {
        float currentSpeed = speed;

        if (Input.GetKey(KeyCode.W))
        {
            currentSpeed *= 2;
        }

        BodyParts[0].transform.Translate(BodyParts[0].transform.forward * currentSpeed * Time.smoothDeltaTime, Space.World);

        /*if (agentAction == 0)
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                BodyParts[0].Rotate(Vector3.up * rotationSpeed * Input.GetAxis("Horizontal"));
            }
        }
        else
        {
            
        }*/
        BodyParts[0].transform.Rotate(Vector3.up * rotationSpeed * agentAction);

        for (int i = 1; i < BodyParts.Count; i++)
        {
            var previousBodyPart = BodyParts[i - 1];
            var currentBodyPart = BodyParts[i];

            float distance = Vector3.Distance(previousBodyPart.transform.position, currentBodyPart.transform.position);
            Vector3 newPosition = previousBodyPart.transform.position;

            newPosition.y = BodyParts[0].transform.position.y;

            float deltaTime = Time.deltaTime * distance / minDistance * currentSpeed;

            if (deltaTime > 0.5f)
                deltaTime = 0.5f;

            currentBodyPart.transform.position = Vector3.Slerp(currentBodyPart.transform.position, newPosition, deltaTime);
            currentBodyPart.transform.rotation = Quaternion.Slerp(currentBodyPart.transform.rotation, previousBodyPart.transform.rotation, deltaTime);
        }

        // Update whole snake position and rotation
        //this.transform.position = BodyParts[0].transform.position;
        //this.transform.rotation = BodyParts[0].transform.rotation;
    }

    // Add new body part to snake
    public void AddBodyPart()
    {
        GameObject newpart = Instantiate(bodyprefab, BodyParts[BodyParts.Count - 1].transform.position, BodyParts[BodyParts.Count - 1].transform.rotation) as GameObject;
        newpart.transform.SetParent(transform);
        BodyParts.Add(newpart);
    }

    private void GetApples()
    {
        //Get all Apples in the Scene
        var apples = GameObject.FindGameObjectsWithTag("goal");
        appleNum = apples.Length;
        if (appleNum > 1)
        {
            //Choose Random apple to be the goal
            System.Random rnd = new System.Random();
            int index = rnd.Next(0, apples.Length);
            appleGoal = apples[index];
        }
        //Choose the only one
        else if (appleNum == 1)
        {
            appleGoal = apples[0];
        }
    }

    public void ResetApples()
    {
        // Get all apples in the scene
        var apples = GameObject.FindGameObjectsWithTag("goal");

        for (int i = 0; i < apples.Length; i++)
        {
            // Random position with 0 velocity
            apples[i].transform.position = new Vector3(Random.Range(-7.0f, 7.0f), DEFAULT_HEIGHT, Random.Range(-7.0f, 7.0f));
            apples[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            apples[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
        }            
    }



    public override void AgentAction(float[] vectorAction)
    {
        /*int speed = 3;
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        
        //Apply Action
        transform.Translate(controlSignal * Time.deltaTime * speed);*/

        Move(vectorAction[0]);

        // Rewards
        if (appleNum > 0 && appleGoal != null)
        {
            float distanceToTarget = Vector3.Distance(BodyParts[0].transform.position, appleGoal.transform.position);

            // Reached Apple
            Debug.Log(distanceToTarget);
            if (distanceToTarget < 0.75f)
            {
                SetReward(1.0f);
                ResetApples();
                AddBodyPart();
                Done();
            }
        }
        
        // Hit Wall or ate itself
        if (collided)
        {
            Done();
        }

        // Fell off platform
        if (BodyParts[0].transform.position.y < 0)
        {
            Done();
        }

    }




    public override void AgentReset()
    {
        if (BodyParts[0].transform.position.y < 0)
        {
            DestroySnakeBody();
            CreateSnakeBody();
        }

        GetApples();

        // Reset snake head position to center
        collided = false;
    }




    public override void CollectObservations()
    {
        // Target and Agent
        /*if (appleNum > 0 && appleGoal != null)
        {
            AddVectorObs(appleGoal.transform.position);
        }
        
        AddVectorObs(this.transform.position);

        // Agent velocity
        AddVectorObs(rigidBody.velocity.x);
        AddVectorObs(rigidBody.velocity.z);*/
    }



    // Collisions Handler
    void OnCollisionEnter(Collision collision)
    {
        // Snake dies if hits a Wall 
        /*if (collision.collider.CompareTag("wall"))
        {
            collided = true;
        }

        // Snake eats an Apple
        else if (collision.collider.CompareTag("goal"))
        {
            SetReward(1.0f);
            AddBodyPart();
            Done();
            Debug.Log("HERE!");
        }*/
    }

    // Destroy snake body
    public void DestroySnakeBody()
    {
        // Destroy everything except the head
        for (int i = BodyParts.Count - 1; i > 0; i--)
        {
            Destroy(BodyParts[i]);
            BodyParts.RemoveAt(i);
        }


        // Reset head position
        BodyParts[0].transform.position = new Vector3(0, 0.25f, 0);
    }

    // Create snake body
    public void CreateSnakeBody()
    {
        // Create new snake
        for (int i = 0; i < beginSize; i++)
        {
            AddBodyPart();
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[1];
        action[0] = Input.GetAxis("Horizontal");

        return action;
    }






}
