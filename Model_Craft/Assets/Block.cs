using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    private Vector3 pointScreen;
    private Player playerScript;
    public List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 curPosition;
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
            playerScript.target = gameObject.transform.position;
            
            if(Input.GetKey(KeyCode.R))
            {
                rotateDirection.x -= playerScript.speedBuildRot * Input.GetAxis("Mouse Y");
                rotateDirection.y += playerScript.speedBuildRot * Input.GetAxis("Mouse X");
                rotateDirection.z = 0;
            }

            else
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance);
                curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
            }

            transform.position = curPosition;
            transform.rotation = Quaternion.Euler(rotateDirection);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        SavePosition(transform.position);

        if(Input.GetKeyUp(KeyCode.R))
        {

            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(rotateDirection);

        }

        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            if(positionHistory.Count > 0)
            {
                if(previousRotate.Count > 0 && previousRotate.Contains(positionHistory[positionHistory.Count-1]))
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
