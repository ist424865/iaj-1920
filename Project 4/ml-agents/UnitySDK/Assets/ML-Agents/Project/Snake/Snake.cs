using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Snake : MonoBehaviour
{   
    //Speed Variables
    public int SPEED = 3;
    private Vector3 speedVector;

    //Body
    public int MAX_BODY = 100;
    private int bodySize = 0;

    public GameObject snake_body;
    private GameObject[] body;

    void OnCollisionEnter(Collision collision)
    {
        //Snake dies if hits a Wall 
        if (collision.collider.tag == "wall")
        {
            Destroy(gameObject);
            Application.Quit();
        }

        //Snake eats an Apple
        if (collision.collider.tag == "goal")
        {
            //var newPiece = GameObject.Instantiate(snake_body);
            //newPiece.transform.position = gameObject.transform.position;
            //body[bodySize] = newPiece;
            //bodySize++;


        }
    }

    // Start is called before the first frame update
    void Start()
    {
        body = new GameObject[MAX_BODY];
        speedVector = new Vector3(0.0f, 0.0f, SPEED);
    }

    // Update is called once per frame
    void Update()
    {
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

        //Perform Movement
       
       
        transform.Translate(speedVector * Time.deltaTime);







    }
}
