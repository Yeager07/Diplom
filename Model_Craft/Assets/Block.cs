using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    private Player playerScript;
    private Vector3 pointScreen;
    public List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 curPosition;
    public bool isActive = false;
    public List<Vector3> positionHistory = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        playerScript = player.GetComponent<Player>();

        positionHistory.Add(transform.position);
        previousRotate.Add(rotateDirection);
    }

    void SavePosition(Vector3 position)
    {
        positionHistory.Add(position);

        if(positionHistory.Count > 20)
        positionHistory.RemoveAt(0);
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
            if(!Input.GetKey(KeyCode.R))
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance);
                curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
            }

            transform.position = curPosition;
        }
    }

    void OnMouseEnter()
    {
        if(playerScript.isBuildMode)
        {
            isActive = true;
            playerScript.target = gameObject.transform.position;
        }
    }

    void OnMouseExit()
    {
        if(playerScript.isBuildMode)
        {
            isActive = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        SavePosition(transform.position);

        if(Input.GetKey(KeyCode.R) && isActive)
        {
                
            if(Input.GetKeyUp(KeyCode.UpArrow))
            rotateDirection.x += 90.0f;

            if(Input.GetKeyUp(KeyCode.DownArrow))
            rotateDirection.x -= 90.0f;

            if(Input.GetKeyUp(KeyCode.LeftArrow))
            rotateDirection.y -= 90.0f;

            if(Input.GetKeyUp(KeyCode.RightArrow))
            rotateDirection.y += 90.0f;

            rotateDirection.z = 0;
            transform.rotation = Quaternion.Euler(rotateDirection);
        }

        if(Input.GetKeyUp(KeyCode.R))
        {

            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(new Vector3(rotateDirection.x % 360, rotateDirection.y % 360, rotateDirection.z));

        }

        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            if(positionHistory.Count >= 1)
            {
                if(previousRotate.Count > 1 && previousRotate.Contains(positionHistory[positionHistory.Count-1]))
                {
                    transform.rotation = Quaternion.Euler(positionHistory[positionHistory.Count - 1]);
                    rotateDirection = positionHistory[positionHistory.Count - 1];
                    previousRotate.RemoveAt(previousRotate.Count-1);
                }

                else
                transform.position = positionHistory[positionHistory.Count - 1];

                positionHistory.RemoveAt(positionHistory.Count - 1);
                return;
            }
        }
    }
}
