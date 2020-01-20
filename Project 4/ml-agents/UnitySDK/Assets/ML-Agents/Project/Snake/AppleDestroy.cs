using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AppleDestroy : MonoBehaviour
{
    public const float DEFAULT_HEIGHT = 0.0f;
    void OnCollisionEnter(Collision collision)
    {   
        /*if(collision.collider.tag == "wall" //|| collision.collider.tag == "Player")
        {
            //Random position with 0 velocity
            gameObject.transform.position = new Vector3(Random.Range(-7.0f, 7.0f), DEFAULT_HEIGHT, Random.Range(-7.0f, 7.0f));
            gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }*/
       
    }
 
}
