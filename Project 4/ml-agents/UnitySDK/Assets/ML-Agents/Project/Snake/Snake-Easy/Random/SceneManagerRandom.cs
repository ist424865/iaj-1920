using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class SceneManagerRandom : Academy {

    public GameObject apple;
    public const int APPLE_NUMBER = 5;

    // Use this for initialization
    void Start () {
        for (int i = 0; i < APPLE_NUMBER; i++)
        {
            var clone = GameObject.Instantiate(apple);
            clone.tag = "goal";
            clone.transform.position = new Vector3(Random.Range(-7.0f, 7.0f), 0, Random.Range(-7.0f, 7.0f));
        }


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
