using UnityEngine;
using System.Collections;
using static System.Math;
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
    public bool isFree = true;

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

        if(positionHistory.Count > 100)
        positionHistory.RemoveAt(0);
    }

    private float CalculateDistance()
    {
        float result = (float)Sqrt(Pow((transform.position.x - playerScript.transform.position.x), 2) + Pow((transform.position.z - playerScript.transform.position.z), 2));
        return result;
    }

    void Move(float distance)
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        transform.position = curPosition;
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
                Move(playerScript.distance);
            }
        }

        else if (CalculateDistance() < 4.0f)
        Move(CalculateDistance());

        else
        return;
    }

    void OnMouseEnter()
    {
        if(playerScript.isBuildMode)
        {
            isActive = true;
            transform.Find("Pupirka").gameObject.SetActive(false);
            playerScript.target = gameObject.transform.position;
        }
    }

    void OnMouseExit()
    {
        if(playerScript.isBuildMode)
        {
            isActive = false;
            transform.Find("Pupirka").gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Selectable"))
        isFree = false;
        else
        isFree = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        SavePosition(transform.position);

        if(Input.GetKey(KeyCode.R) && isActive)
        {     
            if(Input.GetKeyUp(KeyCode.UpArrow))
            transform.Rotate(Vector3.right * 90.0f, Space.World);
            
            if(Input.GetKeyUp(KeyCode.DownArrow))
            transform.Rotate(Vector3.right * (-90.0f), Space.World);
            
            if(Input.GetKeyUp(KeyCode.LeftArrow))
            transform.Rotate(Vector3.up * (-90.0f), Space.World);

            if(Input.GetKeyUp(KeyCode.RightArrow))
            transform.Rotate(Vector3.up * 90.0f, Space.World);

            rotateDirection = transform.rotation.eulerAngles;
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
