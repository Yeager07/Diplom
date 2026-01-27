using UnityEngine;
using System;
using System.Collections;
using static System.Math;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    private Player playerScript;
    private GameObject player;
    private Vector3 pointScreen;
    public List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 moveDirection;
    public Vector3 curPosition;
    private float blockHeight = 0.064f;
    private GameObject place;
    private float bulgeWidth = 0.16f;
    private Vector3 playerRotation;
    public int coef = 1;
    public Material rendgenMaterial;
    public Material standartmaterial;
    public float xDistance = 0.0f;
    public float zDistance = 0.0f;
    public List<Vector3> hollowChildCoordinat = new List<Vector3>();
    public List<Vector3> bulgeChildCoordinat = new List<Vector3>();
    public List<Vector3> hollowChildRotation = new List<Vector3>();
    public List<Vector3> bulgeChildRotation = new List<Vector3>();
    public bool isActive = false;
    public List<Vector3> positionHistory = new List<Vector3>();
    public bool isFree = true;
    public Vector3 height;
    public float yCoord;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        playerScript = player.GetComponent<Player>();

        positionHistory.Add(transform.position);
        previousRotate.Add(rotateDirection);

        FindChild("Hollow", hollowChildCoordinat, hollowChildRotation, transform);
    }

    private void FindChild(string childName, List<Vector3> childCoordinat, List<Vector3> childRotation, Transform objectTransform)
    {
        childCoordinat.Clear();
        childRotation.Clear();

        foreach(Transform child in objectTransform)
        {
            if(child.name == childName)
            {
                childCoordinat.Add(child.position);
                childRotation.Add(child.rotation.eulerAngles);
            }
        }
    }

    void SavePosition(Vector3 position)
    {
        positionHistory.Add(position);

        if(positionHistory.Count > 100)
        positionHistory.RemoveAt(0);
    }

    void Move(float distance)
    {
        if(isFree)
        {
            curPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
            transform.position = Camera.main.ScreenToWorldPoint(curPosition);
        }
        else
        {
            playerRotation = playerScript.transform.rotation.eulerAngles;
            
            if(xDistance >= 1.6f)
            {   
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    transform.position = new Vector3(transform.position.x + bulgeWidth, transform.position.y, transform.position.z);
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - bulgeWidth);
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    transform.position = new Vector3(transform.position.x - bulgeWidth, transform.position.y, transform.position.z);
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + bulgeWidth);

                xDistance = 0.0f;
            }
            else if(xDistance <= -1.6f)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    transform.position = new Vector3(transform.position.x - bulgeWidth, transform.position.y, transform.position.z);
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + bulgeWidth);
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    transform.position = new Vector3(transform.position.x + bulgeWidth, transform.position.y, transform.position.z);
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - bulgeWidth);

                xDistance = 0.0f;
            }
            else
            {
                xDistance += Input.GetAxis("Mouse X");
            }

            if(zDistance >= 1.6f)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + bulgeWidth);
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    transform.position = new Vector3(transform.position.x + bulgeWidth, transform.position.y, transform.position.z);
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - bulgeWidth);
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    transform.position = new Vector3(transform.position.x - bulgeWidth, transform.position.y, transform.position.z);
                
                zDistance = 0.0f;
            }
            else if(zDistance <= -1.6f)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - bulgeWidth);
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    transform.position = new Vector3(transform.position.x - bulgeWidth, transform.position.y, transform.position.z);
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + bulgeWidth);
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    transform.position = new Vector3(transform.position.x + bulgeWidth, transform.position.y, transform.position.z);

                zDistance = 0.0f;
            }
            else
            {
                zDistance += Input.GetAxis("Mouse Y");
            }
        }
    }

    private void UpdateMassive()
    {
        int iterator = 0;
        foreach(var value in playerScript.inventory)
        {
            playerScript.keys[iterator] = value.Key;
            playerScript.values[iterator] = value.Value.ToString();
            iterator += 1;
        }

        player.transform.Find("UI").GetComponent<UI>().UpdateInventoryView();
        Destroy(this.gameObject);
    }

    void AddToInventory()
    {
        if(playerScript.inventory.Count != 5 && !playerScript.inventory.ContainsKey(transform.name))
        {
            playerScript.inventory.Add(transform.name, 1);
            UpdateMassive();
        }

        else if(playerScript.inventory.ContainsKey(transform.name))
        {
            playerScript.inventory[transform.name] += 1;
            UpdateMassive();
        }

        else
        return;
    }

    void OnMouseDrag()
    {
        if(playerScript.isBuildMode)
        {   
            if(!Input.GetKey(KeyCode.R))
            {
                FindChild("Hollow", hollowChildCoordinat, hollowChildRotation, transform);

                if(isFree)
                Move(playerScript.distance);

                else
                {
                    GetComponent<MeshRenderer>().material = rendgenMaterial;
                    Move(0.0f);
                }
            }
        }

        /*else if (CalculateDistance() < 4.0f)
        Move(CalculateDistance());*/

        else
        return;
    }

    void OnMouseEnter()
    {
        isActive = true;
        if(playerScript.isBuildMode)
        {
            playerScript.target = gameObject.transform.position;
        }
    }

    void OnMouseExit()
    {
        isActive = false;
    }

    private void CalculateDistance(List<Vector3> hollows, List<Vector3> bulges)
    {
        float minDistance = 100.0f;
        float yDistance = 0.0f;

        foreach(Vector3 hollow in hollows)
        {
            foreach(Vector3 bulge in bulges)
            {
                float distance = (float)Sqrt(Pow(hollow.x - bulge.x, 2) + Pow(hollow.y - bulge.y, 2) + Pow(hollow.z - bulge.z, 2));
                
                if(distance < minDistance)
                {
                    //transform.RotateAround(hollow);
                    minDistance = distance;
                    
                    if(coef == -1)
                        yDistance = place.transform.position.y + coef * blockHeight * int.Parse(place.name[place.name.Length - 1].ToString());
                    
                    else
                        yDistance = place.transform.position.y + coef * blockHeight * int.Parse(transform.name[transform.name.Length - 1].ToString());

                    moveDirection = new Vector3(bulge.x, yDistance, bulge.z);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isActive)
        {
            if(other.CompareTag("Selectable"))
            {
                place = other.gameObject;

                FindChild("Bulge", bulgeChildCoordinat, bulgeChildRotation, other.gameObject.transform);

                if(bulgeChildCoordinat.Count != 0)
                {
                    if(hollowChildRotation[0].x == bulgeChildRotation[0].x)
                    {
                        isFree = false;
                        CalculateDistance(hollowChildCoordinat, bulgeChildCoordinat);
                        transform.position = moveDirection;
                    }
                }
            }
        }
    }

    private void OnTriggerExit()
    {
        isFree = true;
        GetComponent<MeshRenderer>().material = standartmaterial;
    }

    private void Rotate()
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

    void FixedUpdate()
    {
        if(isActive && Input.GetKey(KeyCode.UpArrow))
        coef = 1;

        if(isActive && Input.GetKey(KeyCode.DownArrow))
        coef = -1;

        if(Input.GetKeyUp(KeyCode.R))
        {
            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(new Vector3(rotateDirection.x % 360, rotateDirection.y % 360, rotateDirection.z));
        }

        if(Input.GetKey(KeyCode.R) && isActive)
        Rotate();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            GetComponent<MeshRenderer>().material = standartmaterial;
            SavePosition(transform.position);
        }

        if(Input.GetKey(KeyCode.M))
        {
            if(isActive)
            {
                isFree = true;
                transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }

            else
            return;
        }

        if(isActive && Input.GetKeyUp(KeyCode.E))
        {
            if(playerScript.inventory.Count != 0)
            {
                foreach(var value in playerScript.inventory)
                {
                    if(value.Value == 0)
                    playerScript.inventory.Remove(value.Key);
                }
                AddToInventory();
            }
            else
            AddToInventory();
        }

        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            if(positionHistory.Count >= 1)
            {
                isFree = true;

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
