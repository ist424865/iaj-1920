using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class SceneManagerRandom : Academy {

    public GameObject apple;
    public const int APPLE_NUMBER = 1;

    // Use for game state initialization
    private void Awake()
    {
        for (int i = 0; i < APPLE_NUMBER; i++)
        {
            var clone = Instantiate(apple);
            clone.tag = "goal";
            clone.transform.position = new Vector3(Random.Range(-7.0f, 7.0f), 0, Random.Range(-7.0f, 7.0f));
        }
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
