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
    public Vector3 curPosition;
    private float blockHeight = 0.064f;
    private GameObject place;
    private float bulgeWidth = 0.16f;
    public Material rendgenMaterial;
    public Material standartmaterial;
    public bool isTrue = false;
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

        foreach(Transform child in transform)
        {
            if(child.name == "Hollow")
            {
                hollowChildCoordinat.Add(child.position);
                hollowChildRotation.Add(child.rotation.eulerAngles);
            }
        }
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
        if(isFree)
        {
            curPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
            transform.position = Camera.main.ScreenToWorldPoint(curPosition);
        }
        else
        {
            if(xDistance >= 3.2f)
            {   
                isTrue = true;
                transform.position = new Vector3(transform.position.x + bulgeWidth, transform.position.y, transform.position.z);
                xDistance = 0.0f;
            }
            else if(xDistance <= -3.2f)
            {
                isTrue = true;
                transform.position = new Vector3(transform.position.x - bulgeWidth, transform.position.y, transform.position.z);
                xDistance = 0.0f;
            }
            else
            {
                isTrue = false;
                xDistance += Input.GetAxis("Mouse X");
            }

            if(zDistance >= 3.2f)
            {
                isTrue = true;
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + bulgeWidth);
                zDistance = 0.0f;
            }
            else if(zDistance <= -3.2f)
            {
                isTrue = true;
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - bulgeWidth);
                zDistance = 0.0f;
            }
            else
            {
                isTrue = false;
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
                if(isFree)
                Move(playerScript.distance);

                else
                {
                    GetComponent<MeshRenderer>().material = rendgenMaterial;
                    Move(0.0f);
                }
            }
        }

        else if (CalculateDistance() < 4.0f)
        Move(CalculateDistance());

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

    private void OnTriggerEnter(Collider other)
    {
        if(isActive)
        {
            if(other.CompareTag("Selectable"))
            {
                place = other.gameObject;
                bulgeChildCoordinat.Clear();
                bulgeChildRotation.Clear();

                foreach(Transform child in other.gameObject.transform)
                {
                    if(child.name == "Bulge")
                    {
                        bulgeChildCoordinat.Add(child.position);
                        bulgeChildRotation.Add(child.rotation.eulerAngles);
                    }
                }

                if(hollowChildRotation[0].x == bulgeChildRotation[0].x)
                {
                    isFree = false;
                    GetComponent<MeshRenderer>().material = rendgenMaterial;
                    bulgeChildCoordinat[0] = new Vector3(bulgeChildCoordinat[0].x , place.transform.position.y + blockHeight * int.Parse(place.name[place.name.Length - 1].ToString()), bulgeChildCoordinat[0].z);
                    transform.position = bulgeChildCoordinat[0];
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

    // Update is called once per frame
    void Update()
    {
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

        if(Input.GetMouseButtonUp(0))
        {
            GetComponent<MeshRenderer>().material = standartmaterial;
            SavePosition(transform.position);
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

        if(Input.GetKey(KeyCode.R) && isActive)
        Rotate();

        if(Input.GetKeyUp(KeyCode.R))
        {
            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(new Vector3(rotateDirection.x % 360, rotateDirection.y % 360, rotateDirection.z));

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
