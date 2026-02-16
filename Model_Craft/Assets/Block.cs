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
    private List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 bulgePosition;
    private Vector3 localHollowPosition;
    public Vector3 curPosition;
    public Vector3 previousPosition;
    private GameObject place;
    private float bulgeWidth = 0.16f;
    private Vector3 moveX = new Vector3(0.16f, 0.0f, 0.0f);
    private Vector3 moveY = new Vector3(0.0f, 0.16f, 0.0f);
    private Vector3 moveZ = new Vector3(0.0f, 0.0f, 0.16f);
    private Vector3 playerRotation;
    //public int coef = 1;
    public Material rendgenMaterial;
    public Material standartmaterial;
    public float xDistance = 0.0f;
    public float zDistance = 0.0f;
    public List<Transform> hollowChild;
    public List<Transform> bulgeChild;
    public List<Vector3> hollowChildCoordinat = new List<Vector3>();
    public List<Vector3> bulgeChildCoordinat = new List<Vector3>();
    public List<Vector3> hollowChildRotation = new List<Vector3>();
    public List<Vector3> bulgeChildRotation = new List<Vector3>();
    public bool isActive = false;
    public List<Vector3> positionHistory = new List<Vector3>();
    public bool isFree = true;
    public bool isMagnetic = false;
    public Vector3 height;
    public float yCoord;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        playerScript = player.GetComponent<Player>();

        previousPosition = transform.position;
        positionHistory.Add(transform.position);
        previousRotate.Add(rotateDirection);

        FindChild("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);
    }

    private Transform FindMainParent(Transform currentObject)
    {
        while(currentObject.parent != null)
        {
            currentObject = currentObject.parent;
        }
        
        return currentObject;
    }

    private void FindChild(string childName, List<Vector3> childCoordinat, List<Vector3> childRotation, Transform objectTransform, List<Transform> massiveChild)
    {
        childCoordinat.Clear();
        childRotation.Clear();
        massiveChild.Clear();

        foreach(Transform child in objectTransform)
        {
            if(child.name == childName)
            {
                childCoordinat.Add(child.position);
                childRotation.Add(child.rotation.eulerAngles);
                massiveChild.Add(child);
            }
        }
    }

    void SavePosition(Vector3 position)
    {
        if(positionHistory[positionHistory.Count - 1] == position)
        return;

        else
        positionHistory.Add(position);

        if(positionHistory.Count > 100)
        positionHistory.RemoveAt(0);
    }

    private void CalculateMoveVector(float distance, Transform movingObject, Vector3 moveSide, Vector3 moveHeight)
    {
        if(distance > 1)
        {
            if(distance == xDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position += moveSide;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position -= moveHeight;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position -= moveSide;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position += moveHeight;

                movingObject.gameObject.GetComponent<Block>().xDistance = 0.0f;
            }
            
            else if(distance == zDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position += moveHeight;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position += moveSide;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position -= moveHeight;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position -= moveSide;
                
                movingObject.gameObject.GetComponent<Block>().zDistance = 0.0f;
            }
        }

        else if (distance < 1)
        {
            if(distance == xDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position -= moveSide;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position += moveHeight;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position += moveSide;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position -= moveHeight;

                movingObject.gameObject.GetComponent<Block>().xDistance = 0.0f;
            }
            
            else if(distance == zDistance)
            {
                if((playerRotation.y >= 315.0f && playerRotation.y <= 360.0f) || (playerRotation.y >= 0.0f && playerRotation.y <= 45.0f))
                    movingObject.position -= moveHeight;
                
                else if(playerRotation.y >= 45.0f && playerRotation.y <= 135.0f)
                    movingObject.position -= moveSide;
                
                else if(playerRotation.y >= 135.0f && playerRotation.y <= 225.0f)
                    movingObject.position += moveHeight;
                
                else if(playerRotation.y >= 225.0f && playerRotation.y <= 315.0f)
                    movingObject.position += moveSide;

                movingObject.gameObject.GetComponent<Block>().zDistance = 0.0f;
            }
        }
    }
    
    void Move(float distance, GameObject currentObject)
    {
        Transform movingObject = currentObject.transform;
        Block blockScript = currentObject.GetComponent<Block>();

        if(blockScript.isFree)
        {
            curPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
            movingObject.position = Camera.main.ScreenToWorldPoint(curPosition);
        }
        else
        {
            playerRotation = playerScript.transform.rotation.eulerAngles;
            
            if(blockScript.xDistance >= bulgeWidth*20)
            {   
                if(movingObject.rotation.eulerAngles.x == 90.0f || movingObject.rotation.eulerAngles.x == 270.0f)
                    CalculateMoveVector(blockScript.xDistance, movingObject, moveX, moveY);
                else
                    CalculateMoveVector(blockScript.xDistance, movingObject, moveX, moveZ);
            }
            else if(blockScript.xDistance <= -bulgeWidth*20)
            {
                if(movingObject.rotation.eulerAngles.x == 90.0f || movingObject.rotation.eulerAngles.x == 270.0f)
                    CalculateMoveVector(blockScript.xDistance, movingObject, moveX, moveY);
                else
                    CalculateMoveVector(blockScript.xDistance, movingObject, moveX, moveZ);
            }
            else
                blockScript.xDistance += Input.GetAxis("Mouse X");

            if(blockScript.zDistance >= bulgeWidth*20)
            {
                if(movingObject.rotation.eulerAngles.x == 90.0f || movingObject.rotation.eulerAngles.x == 270.0f)
                    CalculateMoveVector(blockScript.zDistance, movingObject, moveX, moveY);
                else
                    CalculateMoveVector(blockScript.zDistance, movingObject, moveX, moveZ);
            }
            else if(blockScript.zDistance <= -bulgeWidth*20)
            {
                if(movingObject.rotation.eulerAngles.x == 90.0f || movingObject.rotation.eulerAngles.x == 270.0f)
                    CalculateMoveVector(blockScript.zDistance, movingObject, moveX, moveY);
                else
                    CalculateMoveVector(blockScript.zDistance, movingObject, moveX, moveZ);
            }
            else
                blockScript.zDistance += Input.GetAxis("Mouse Y");
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
                FindChild("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);

                if(isFree)
                Move(playerScript.distance, gameObject);

                else
                {
                    if(!FindMainParent(transform).gameObject.GetComponent<Block>().isFree)
                        FindMainParent(transform).gameObject.GetComponent<MeshRenderer>().material = rendgenMaterial;

                    Move(playerScript.distance, FindMainParent(transform).gameObject);
                }
            }
        }

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

    private void CalculateDistance(List<Transform> hollows, List<Transform> bulges)
    {
        float minDistance = 100.0f;
        //float yDistance = 0.0f;

        foreach(Transform hollow in hollows)
        {
            foreach(Transform bulge in bulges)
            {
                float distance = (float)Sqrt(Pow(hollow.position.x - bulge.position.x, 2) + Pow(hollow.position.y - bulge.position.y, 2) + Pow(hollow.position.z - bulge.position.z, 2));
                
                if(distance < minDistance)
                {
                    minDistance = distance;
                    localHollowPosition = hollow.localPosition;
                    bulgePosition = bulge.position;
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

                FindChild("Bulge", bulgeChildCoordinat, bulgeChildRotation, other.gameObject.transform, bulgeChild);

                if(bulgeChildCoordinat.Count != 0)
                {
                    if(hollowChildRotation[0].x == bulgeChildRotation[0].x)
                    {
                        isFree = false;
                        CalculateDistance(hollowChild, bulgeChild);
                        transform.position = bulgePosition - transform.TransformVector(localHollowPosition);
                    }
                }
            }
        }
    }

    private void OnTriggerExit()
    {
        if(!isMagnetic)
        {
            transform.parent = null;
            isFree = true;
            GetComponent<MeshRenderer>().material = standartmaterial;
        }
    }

    private void Rotate(Transform currentObject)
    {
        if(Input.GetKeyUp(KeyCode.UpArrow))
        currentObject.Rotate(Vector3.right * 90.0f, Space.World);
            
        if(Input.GetKeyUp(KeyCode.DownArrow))
        currentObject.Rotate(Vector3.right * (-90.0f), Space.World);
            
        if(Input.GetKeyUp(KeyCode.LeftArrow))
        currentObject.Rotate(Vector3.up * (-90.0f), Space.World);

        if(Input.GetKeyUp(KeyCode.RightArrow))
        currentObject.Rotate(Vector3.up * 90.0f, Space.World);

        rotateDirection = currentObject.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if(!isFree)
            {
                transform.SetParent(place.transform);
                isMagnetic = true;
            }

            GetComponent<MeshRenderer>().material = standartmaterial;

            if(transform.position != previousPosition)
            {
                SavePosition(previousPosition);                
                previousPosition = transform.position;
            }
        }

        if(Input.GetKeyUp(KeyCode.M))
        {
            if(isActive)
            {
                if(isMagnetic)
                {
                    isMagnetic = false;
                    transform.parent = null;
                }

                else
                {
                    isFree = true;
                    transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                }
            }

            else
            return;
        }

        if(Input.GetKeyUp(KeyCode.R))
        {
            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(new Vector3(rotateDirection.x % 360, rotateDirection.y % 360, rotateDirection.z));
        }

        if(Input.GetKey(KeyCode.R) && isActive)
        Rotate(FindMainParent(transform));

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

        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Z))
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

                if(positionHistory.Count > 1)
                positionHistory.RemoveAt(positionHistory.Count - 1);

                previousPosition = positionHistory[positionHistory.Count-1];

                return;
            }
        }
    }
}
