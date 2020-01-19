using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Snake : Agent
{
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
    
    //Collision Flag
    private bool collided = false;

    private void getApples()
    {
        //Get all Apples in the Scene
        var apples = GameObject.FindGameObjectsWithTag("goal");
        appleNum = apples.Length;
        
        //Multiple Apples
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
        int speed = 3;
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        
        //Apply Action
        //transform.Translate(controlSignal * Time.deltaTime * speed);
        rigidBody.velocity = controlSignal * speed;
        rigidBody.angularVelocity = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        // Rewards
        if (appleNum > 0 && appleGoal != null)
        {
            float distanceToTarget = Vector3.Distance(gameObject.transform.position, appleGoal.transform.position);
            
            // Reached Apple
            if (distanceToTarget < 1.0f)
            {
                SetReward(1.0f);
            }

            if (distanceToTarget < 2.0f)
            {
                SetReward(0.5f);
            }

            if (distanceToTarget < 3.0f)
            {
                SetReward(0.33f);
            }
        }
        
        // Hit Wall or ate itself
        if (collided)
        {
            Done();
        }

    }




    public override void AgentReset()
    {
 
            //destroy body of the snake
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

            collided = false;
        
    }




    public override void CollectObservations()
    {
        // Target and Agent
        if (appleNum > 0 && appleGoal != null)
        {
            AddVectorObs(appleGoal.transform.position);
        }
        
        AddVectorObs(this.transform.position);

        // Agent velocity
        AddVectorObs(rigidBody.velocity.x);
        AddVectorObs(rigidBody.velocity.z);
    }



    //Collions Handler
    void OnCollisionEnter(Collision collision)
    {
        //Snake dies if hits a Wall 
        if (collision.collider.tag == "wall")
        {
            collided = true;
        }

        //Snake eats an Apple
        if (collision.collider.tag == "goal")
        {
            var newPiece = GameObject.Instantiate(snake_body);
            newPiece.transform.position = gameObject.transform.position;
            old = gameObject.transform.position;
            body[bodySize] = newPiece;
            bodySize++;

        }
    }



    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }

    // Start is called before the first frame update
    void Start()
    {
        getApples();
        rigidBody = GetComponent<Rigidbody>();
        body = new GameObject[MAX_BODY];
        speedVector = new Vector3(0.0f, 0.0f, SPEED);
    }

    // Update is called once per frame
    void Update()
    {
        frames++;
        pointsDisplay.GetComponent<UnityEngine.UI.Text>().text = "Score : " + bodySize;

        if (Input.GetKeyDown("w"))
        {
            speedVector = new Vector3(0.0f, 0.0f, SPEED);
  
        }

        if (Input.GetKeyDown("s"))
        {
            speedVector = new Vector3(0.0f, 0.0f, -SPEED);

        }

        if (Input.GetKeyDown("a"))
        {
            speedVector = new Vector3(-SPEED, 0.0f, 0.0f);
   
        }

        if (Input.GetKeyDown("d"))
        {
            speedVector = new Vector3(SPEED, 0.0f, 0.0f);
   
        }


        //Perform Movement on the head (For Manual testing)
        //transform.Translate(speedVector * Time.deltaTime);

        //Every 4 frames update the body(Higher refresh rate will cause bad behaviour)
        if (frames % 4 == 0)
        {
            //More than one bodypiece
            if (bodySize > 1)
            {
                for (int i = 0; i < bodySize - 1; i++)
                {

                    //Move body piece relative to the next
                    body[i].transform.position = body[i + 1].transform.position;

                }
                //Move last piece relative to the head
                body[bodySize - 1].transform.position = old;
            }
            //Only one body piece
            if (bodySize == 1)
            {
                body[0].transform.position = old;
            }
        }

        //Wait for the body Piece to be in the back
        if (frames % 40 == 0)
        {
            for (int i = 0; i < bodySize - 3; i++)
            {
                body[i].tag = "wall";
            }
        }
        //Position of the head
        old = gameObject.transform.position;


    }
}
