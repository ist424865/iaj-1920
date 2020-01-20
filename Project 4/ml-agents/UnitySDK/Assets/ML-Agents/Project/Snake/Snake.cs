using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Snake : Agent
{
    // Snake body list
    public List<Transform> BodyParts = new List<Transform>();

    // Snake body part prefab
    public GameObject bodyprefab;

    // Minimum distance between body parts
    public float minDistance = 1f;
    public float speed = 0;
    public float rotationSpeed = 0.5f;
    // Number of initial body parts
    public int beginSize = 5;





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
    private int appleNum;

    //Frame counter
    private long frames = 0;

    private bool collided = false;


    // Start is called before the first frame update
    void Start()
    {
        getApples();
        /*rigidBody = GetComponent<Rigidbody>();
        Application.targetFrameRate = 30;
        body = new GameObject[MAX_BODY];
        speedVector = new Vector3(0.0f, 0.0f, SPEED);*/
        for (int i = 0; i < beginSize; i++)
        {
            AddBodyPart();
        }
    }

    // Update is called once per frame
    void Update()
    {

        Move();

        if (Input.GetKey(KeyCode.Q))
        {
            AddBodyPart();
        }
        /*frames++;
        pointsDisplay.GetComponent<UnityEngine.UI.Text>().text = "Score : " + bodySize;

        if (Input.GetKeyDown("w"))
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
    public void Move()
    {
        float currentSpeed = speed;

        if (Input.GetKey(KeyCode.W))
        {
            currentSpeed *= 2;
        }

        BodyParts[0].Translate(BodyParts[0].forward * currentSpeed * Time.smoothDeltaTime, Space.World);

        if (Input.GetAxis("Horizontal") != 0)
        {
            BodyParts[0].Rotate(Vector3.up * rotationSpeed * Input.GetAxis("Horizontal"));
        }

        for (int i = 1; i < BodyParts.Count; i++)
        {
            var previousBodyPart = BodyParts[i - 1];
            var currentBodyPart = BodyParts[i];

            float distance = Vector3.Distance(previousBodyPart.position, currentBodyPart.position);
            Vector3 newPosition = previousBodyPart.position;

            newPosition.y = BodyParts[0].position.y;

            float deltaTime = Time.deltaTime * distance / minDistance * currentSpeed;

            if (deltaTime > 0.5f)
                deltaTime = 0.5f;

            currentBodyPart.position = Vector3.Slerp(currentBodyPart.position, newPosition, deltaTime);
            currentBodyPart.rotation = Quaternion.Slerp(currentBodyPart.rotation, previousBodyPart.rotation, deltaTime);
        }
    }

    // Add new body part to snake
    public void AddBodyPart()
    {
        Transform newpart = (Instantiate(bodyprefab, BodyParts[BodyParts.Count - 1].position, BodyParts[BodyParts.Count - 1].rotation) as GameObject).transform;
        newpart.SetParent(transform);
        BodyParts.Add(newpart);
    }

    private void getApples()
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
        if (appleNum == 1)
        {
            appleGoal = apples[0];
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
        transform.Translate(controlSignal * Time.deltaTime * speed);

        // Rewards
        if (appleNum > 0 && appleGoal != null)
        {
            float distanceToTarget = Vector3.Distance(gameObject.transform.position, appleGoal.transform.position);
            
            // Reached Apple
            if (distanceToTarget < 1.0f)
            {
                SetReward(1.0f);
            }
        }
        
        // Hit Wall or ate itself
        if (collided)
        {
            Done();
        }*/

    }




    public override void AgentReset()
    {

        /*//destroy body of the snake
        foreach (GameObject piece in body)
        {
            Destroy(piece);
        }

        //get new goal
            getApples();


        //reset snake head at (0,0,0)
        this.transform.position = new Vector3(0, 0.12f, 0);
        this.transform.rotation = Quaternion.identity;
        this.rigidBody.angularVelocity = Vector3.zero;
        this.rigidBody.velocity = Vector3.zero;
        bodySize = 0;
        body = new GameObject[MAX_BODY];

        collided = false; */
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
        if (collision.collider.CompareTag("wall"))
        {
            collided = true;
        }

        // Snake eats an Apple
        else if (collision.collider.CompareTag("goal"))
        {
            AddBodyPart();
        }
    }



    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = 0;//Input.GetAxis("Horizontal");
        action[1] = 0;//Input.GetAxis("Vertical");

        return action;
    }






}
