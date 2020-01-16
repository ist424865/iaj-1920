using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Snake : MonoBehaviour
{
    int component = 3;
    Vector3 speed;

    void OnCollisionEnter(Collision collision)
    {
        //Snake dies if hits a Wall 
        if (collision.collider.tag == "wall")
        {
            Destroy(gameObject);
            Application.Quit();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        speed = new Vector3(0.0f, 0.0f, component);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("w"))
        {
            speed = new Vector3(0.0f, 0.0f, component);
        }

        if (Input.GetKeyDown("s"))
        {
            speed = new Vector3(0.0f, 0.0f, -component);
        }

        if (Input.GetKeyDown("a"))
        {
            speed = new Vector3(-component, 0.0f, -0.0f);
        }

        if (Input.GetKeyDown("d"))
        {
            speed = new Vector3(component, 0.0f, -0.0f);
        }

        //Perform Movement
        transform.Translate(speed * Time.deltaTime);


    }
}
