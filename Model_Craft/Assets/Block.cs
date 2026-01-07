using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    private Vector3 pointScreen;
    private Player playerScript;
    public float zPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        playerScript = player.GetComponent<Player>();
    }

    void onMouseDown()
    {
        if(playerScript.isBuildMode)
        pointScreen = Camera.main.WorldToScreenPoint(transform.position);
    }

    void OnMouseDrag()
    {
        if(playerScript.isBuildMode)
        {
            playerScript.target = gameObject.transform.position;
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zPos);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
            transform.position = curPosition;
        }
    }

    void FixedUpdate()
    {
        if(playerScript.transform.position.z < 0)
        zPos = -playerScript.transform.position.z + transform.position.z;

        else
        zPos = -transform.position.z + playerScript.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
