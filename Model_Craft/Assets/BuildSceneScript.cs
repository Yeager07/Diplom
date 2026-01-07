using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildSceneScript : MonoBehaviour
{
    private Player playerScript;
    private Rigidbody rigidBody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        playerScript = player.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            playerScript.distance = 2.0f;
        }
    }
}
