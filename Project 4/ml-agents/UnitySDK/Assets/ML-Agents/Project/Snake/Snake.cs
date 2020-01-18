using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Snake : MonoBehaviour
{   
    //Speed Variables
    public int SPEED = 3;
    public GameObject pointsDisplay;
    private Vector3 speedVector;

    //Body
    public int MAX_BODY = 100;
    private int bodySize = 0;

    public GameObject snake_body;
    private GameObject[] body;

    //Position of the head a few frames before
    private Vector3 old;

    //Frame counter
    private long frames = 0;
    void OnCollisionEnter(Collision collision)
    {
        //Snake dies if hits a Wall 
        if (collision.collider.tag == "wall")
        {
            //Destroy head of the snake
            Destroy(gameObject);

            //Destroy body of the snake
            foreach (GameObject piece in body)
            {
                Destroy(piece);
            }
            Application.Quit();
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

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
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

        //Perform Movement on the head
        transform.Translate(speedVector * Time.deltaTime);

        //Every 4 frames update the body (Higher refresh rate will cause bad behaviour)
        if (frames % 4 == 0)
        {
            if(bodySize > 1)
            {
                for(int i = 0; i< bodySize-1; i++)
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
        old = gameObject.transform.position;
       

    }
}
