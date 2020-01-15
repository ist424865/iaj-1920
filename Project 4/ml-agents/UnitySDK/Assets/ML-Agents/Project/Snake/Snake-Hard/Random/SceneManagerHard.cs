using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class SceneManagerHard : Academy {

    public GameObject defaultApple;
    public const int APPLE_NUMBER = 6;
    //public const float DERDEFAULT_HEIGHT = 0.23f;
    public const float DERDEFAULT_HEIGHT = 0.0f;

    // Use this for initialization
    void Start () {
        for (int i = 0; i < APPLE_NUMBER; i++)
        {
            var clone = GameObject.Instantiate(defaultApple);
            clone.transform.position = new Vector3(Random.Range(-7.0f, 7.0f), DERDEFAULT_HEIGHT, Random.Range(-7.0f, 7.0f));
        }

    }
	
	// Update is called once per frame
	void Update () {
     
    }
}
