using UnityEngine;
using System;
using System.Collections;
using static System.Math;
using System.Collections.Generic;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    private InventoryManager inventoryManager;
    private Player playerScript;
    private MainScript mainScript;
    private GameObject player;
    private Vector3 pointScreen;
    private List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 bulgePosition;
    private Vector3 localHollowPosition;
    private Vector3 localBulgePosition;
    private Vector3 placeHollowPosition;
    public Vector3 moveVector = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 curPosition;
    public Vector3 previousPosition;
    public GameObject colorChoosePanel;
    public GameObject place;
    private float bulgeWidth = 0.16f;
    public float blockHeight = 0.064f;
    private Vector3 moveX = new Vector3(0.16f, 0.0f, 0.0f);
    private Vector3 moveY = new Vector3(0.0f, 0.16f, 0.0f);
    private Vector3 moveZ = new Vector3(0.0f, 0.0f, 0.16f);
    private Vector3 playerRotation;
    public Vector3 spaceBetweenBlockCursor;
    public float xDistance = 0.0f;
    public float zDistance = 0.0f;
    public List<Transform> hollowChild;
    public List<Transform> bulgeChild;
    public List<Vector3> hollowChildCoordinat = new List<Vector3>();
    public List<Vector3> bulgeChildCoordinat = new List<Vector3>();
    public List<Vector3> hollowChildRotation = new List<Vector3>();
    public List<Vector3> bulgeChildRotation = new List<Vector3>();
    public Transform nearestBulge;
    public List<Transform> blockChild;
    public List<GameObject> previousBlock = new List<GameObject>();
    public bool isActive = false;
    public List<Vector3> positionHistory = new List<Vector3>();
    public bool isFree = true;
    public bool isMagnetic = false;
    public bool isPlaced = false;
    public string blockType;
    private Material mainBlockMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mainScript = Camera.main.GetComponent<MainScript>();

        GameObject inventory = GameObject.FindGameObjectWithTag("InventoryManager");
        if(inventory != null)
        inventoryManager = inventory.GetComponent<InventoryManager>();

        if (player != null)
        playerScript = player.GetComponent<Player>();

        previousPosition = transform.position;
        positionHistory.Add(transform.position);
        previousRotate.Add(rotateDirection);

        FindChildPoint("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);
        FindChildPoint("Bulge", bulgeChildCoordinat, bulgeChildRotation, transform, bulgeChild);

        mainBlockMaterial = gameObject.GetComponent<Renderer>().material;
    }

    private Transform FindMainParent(Transform currentObject)
    {
        while(currentObject.parent != null)
        {
            currentObject = currentObject.parent;
        }
        
        return currentObject;
    }

    private void FindChildPoint(string childName, List<Vector3> childCoordinat, List<Vector3> childRotation, Transform objectTransform, List<Transform> massiveChild)
    {
        childCoordinat.Clear();
        childRotation.Clear();
        massiveChild.Clear();

        foreach(Transform child in objectTransform)
        {
            if(child.name == childName)
            {
                childCoordinat.Add(child.position);
                childRotation.Add(transform.localRotation.eulerAngles);
                massiveChild.Add(child);
            }
        }
    }

    void SavePosition(Vector3 position)
    {
        positionHistory.Add(position);

        if(positionHistory.Count > 100)
        positionHistory.RemoveAt(0);
    }

    void PlaceObjectCorrectly(Transform movingObject)
    {
        List<Transform> transformHollows = new List<Transform>();

        foreach(Transform child in movingObject)
        {
            if(child.name == "Hollow")
            transformHollows.Add(child);
        }

        Vector3 hollowPosition = transformHollows[0].position;
        
        if(hollowPosition.x % bulgeWidth != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.x = hollowPosition.x % bulgeWidth;

        if(hollowPosition.y % blockHeight != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.y = hollowPosition.y % (4 * bulgeWidth / 10);

        if(hollowPosition.z % bulgeWidth != 0)
        FindMainParent(movingObject).gameObject.GetComponent<Block>().moveVector.z = hollowPosition.z % bulgeWidth;    
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
    
    public void Move(float distance, GameObject currentObject)
    {
        Transform movingObject = currentObject.transform;
        Block blockScript = currentObject.GetComponent<Block>();

        if(blockScript.isFree)
        {
            curPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
            movingObject.position = Camera.main.ScreenToWorldPoint(curPosition);
            PlaceObjectCorrectly(movingObject);
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

    void OnMouseDrag()
    {   
        if(playerScript.colorChoosePanel == null || !playerScript.colorChoosePanel.gameObject.activeInHierarchy)
        {
            if(Input.GetMouseButton(0))
            playerScript.movedObject = FindMainParent(transform).gameObject;
    
            if(playerScript.isBuildMode)
            {   
                if(!Input.GetKey(KeyCode.R))
                {
                    FindChildPoint("Hollow", hollowChildCoordinat, hollowChildRotation, transform, hollowChild);
                    FindChildPoint("Bulge", bulgeChildCoordinat, bulgeChildRotation, transform, bulgeChild);

                    if(FindMainParent(transform) == null)
                    Move(playerScript.distance, gameObject);

                    else
                    {   
                        if(!FindMainParent(transform).gameObject.GetComponent<Block>().isFree)
                        {
                            FindMainParent(transform).gameObject.GetComponent<MeshRenderer>().material = mainScript.rendgenMaterial;

                            if(FindMainParent(transform).GetComponent<Block>().blockChild.Count == 0)
                            mainScript.FindAllChild(FindMainParent(transform), FindMainParent(transform).GetComponent<Block>().blockChild);

                            foreach(Transform child in FindMainParent(transform).GetComponent<Block>().blockChild)
                            child.GetComponent<MeshRenderer>().material = mainScript.rendgenMaterial;
                        }

                        Move(playerScript.distance, FindMainParent(transform).gameObject);
                    }
                }
            }
    
            else
            Move(playerScript.minDistance, gameObject);
        }
    }

    void OnMouseEnter()
    {
        isActive = true;

        if(playerScript.isBuildMode)
        playerScript.target = transform.position + new Vector3(0.0f, -0.4f, 0.0f);
    }

    void OnMouseExit()
    {
        isActive = false;
        
        if(playerScript.isBuildMode)
        playerScript.target = new Vector3(0.0f, 0.0f, 0.0f);

        //FindMainParent(transform).GetComponent<Block>().blockChild.Clear();
    }

    private void CalculateDistance(GameObject currentBlock, GameObject place, List<Transform> hollows, List<Transform> bulges)
    {
        float minDistance = 100.0f;
        List<Transform> transformBulges = new List<Transform>();
        
        foreach(Transform child in currentBlock.transform)
        transformBulges.Add(child);

        int jterator = 0;
        
        while(jterator < hollows.Count)
        {
            int iterator = 0;
            
            while(iterator < bulges.Count)
            {
                float distance = (float)Sqrt(Pow(hollows[jterator].position.x - bulges[iterator].position.x, 2) + Pow(hollows[jterator].position.y - bulges[iterator].position.y, 2) + Pow(hollows[jterator].position.z - bulges[iterator].position.z, 2));
                
                if(distance < minDistance)
                {
                    minDistance = distance;
                    currentBlock.GetComponent<Block>().localHollowPosition = hollows[jterator].localPosition;
                    currentBlock.GetComponent<Block>().localBulgePosition = transformBulges[jterator].localPosition;
                    currentBlock.GetComponent<Block>().bulgePosition = bulges[iterator].position;
                    currentBlock.GetComponent<Block>().nearestBulge = bulges[iterator];
                    currentBlock.GetComponent<Block>().placeHollowPosition = place.transform.GetComponent<Block>().hollowChild[iterator].position;
                }

                iterator += 1;
            }

            jterator += 1;
        }
    }

    private void MakeConnection(GameObject currentBlock, GameObject place)
    {
        if(currentBlock.GetComponent<Block>().bulgeChildCoordinat.Count != 0)
        {
            CalculateDistance(currentBlock, place, currentBlock.GetComponent<Block>().hollowChild, place.GetComponent<Block>().bulgeChild);
            
            if(Vector3.Dot(currentBlock.GetComponent<Block>().hollowChild[0].up, currentBlock.GetComponent<Block>().nearestBulge.up) > 0.99f)
            {
                currentBlock.GetComponent<Block>().isFree = false;
                        
                if(currentBlock.GetComponent<Block>().bulgeChild[0].transform.position.y < place.GetComponent<Block>().bulgeChild[0].transform.position.y)
                currentBlock.transform.position = currentBlock.GetComponent<Block>().placeHollowPosition - currentBlock.transform.TransformVector(currentBlock.GetComponent<Block>().localBulgePosition);
                        
                else
                currentBlock.transform.position = currentBlock.GetComponent<Block>().bulgePosition - currentBlock.transform.TransformVector(localHollowPosition);
            }
        }

        place.GetComponent<Block>().blockChild.Add(currentBlock.transform);
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(playerScript.isBuildMode && !isMagnetic && gameObject == playerScript.movedObject)
        {
            if(other.CompareTag("Selectable"))
            {
                place = other.gameObject;
                
                if(!previousBlock.Contains(place))
                previousBlock.Add(place);

                if(FindMainParent(transform) != null)
                MakeConnection(FindMainParent(transform).gameObject, place);
            
                else
                MakeConnection(gameObject, place);
            }
        }

        else if(isFree && Input.GetMouseButtonUp(0))
        MakeConnection(gameObject, other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if(!isMagnetic && transform.parent == null)
        {   
            if(other.gameObject == place)
            {
                GetComponent<MeshRenderer>().material = mainBlockMaterial;

                if(FindMainParent(transform).GetComponent<Block>().blockChild.Count == 0)
                mainScript.FindAllChild(FindMainParent(transform), FindMainParent(transform).GetComponent<Block>().blockChild);

                foreach(Transform child in FindMainParent(transform).GetComponent<Block>().blockChild)
                child.GetComponent<MeshRenderer>().material = child.GetComponent<Block>().mainBlockMaterial;
                
                isFree = true;
                place = null;

                if(blockChild.Count != 0)
                blockChild.RemoveAt(0);
            }
        }

        else if(other.gameObject == place)
        {
            transform.parent = null;
            isFree = true;

            if(blockChild.Count != 0)
            blockChild.RemoveAt(0);
        }

        isMagnetic = false;
        
        if(previousBlock.Count != 0)
        previousBlock.RemoveAt(0);

        if(blockChild.Count != 0)
        blockChild.RemoveAt(0);
    }

    private void Rotation(Transform currentObject)
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

    private void MoveSpawnedObject()
    {
        if(Input.GetMouseButtonUp(0))
        Camera.main.GetComponent<MainScript>().newBlock.GetComponent<Block>().isPlaced = true;
        
        else
        Camera.main.GetComponent<MainScript>().newBlock.GetComponent<Block>().Move(playerScript.distance, Camera.main.GetComponent<MainScript>().newBlock);
    }

    /*void FixedUpdate()
    {   
    }*/

    // Update is called once per frame
    void Update()
    {   
        if(Camera.main.GetComponent<MainScript>().newBlock != null && !Camera.main.GetComponent<MainScript>().newBlock.GetComponent<Block>().isPlaced)
        MoveSpawnedObject(); 

        else if(Input.GetMouseButtonUp(0))
        {   
            GetComponent<MeshRenderer>().material = mainBlockMaterial;

            if(isFree && playerScript.movedObject == gameObject)
            {
                FindMainParent(transform).position -= FindMainParent(transform).gameObject.GetComponent<Block>().moveVector;
                FindMainParent(transform).gameObject.GetComponent<Block>().moveVector = new Vector3(0.0f, 0.0f, 0.0f);
                playerScript.movedObject = null;
            }

            if(!isFree)
            {
                isMagnetic = true;
                transform.SetParent(place.transform);
            }

            if(previousBlock.Count != 1)
            {
                foreach(GameObject block in previousBlock)
                {
                    if(block != null && block != transform.parent)
                    {
                        block.transform.SetParent(transform);
                        blockChild.Add(block.transform);
                        block.GetComponent<Block>().place = gameObject;
                        block.GetComponent<Block>().isMagnetic = true;
                        previousPosition = transform.localPosition;
                    }
                }
            }

            if(transform.position != previousPosition)
            {
                SavePosition(previousPosition);
                previousPosition = transform.localPosition;
            }

            else
            SavePosition(transform.localPosition);
        }

        else if(Input.GetKeyUp(KeyCode.M))
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

        else if(Input.GetKeyUp(KeyCode.R))
        {
            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(new Vector3(rotateDirection.x % 360, rotateDirection.y % 360, rotateDirection.z));
        }

        else if(Input.GetKey(KeyCode.R) && isActive)
        Rotation(FindMainParent(transform));

        else if(isActive && Input.GetKeyUp(KeyCode.E) && blockChild.Count == 0 && transform.parent == null)
        {
            if(inventoryManager.inventory.Count != 0)
            {
                foreach(var value in inventoryManager.inventory)
                {
                    if(value.Value == 0)
                    inventoryManager.inventory.Remove(value.Key);
                }
                
                inventoryManager.AddToInventory(transform);
            }
            
            else
            inventoryManager.AddToInventory(transform);
            
            playerScript.target = new Vector3(0.0f, 0.0f, 0.0f);
        }

        else if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.I))
        {
            if(positionHistory.Count >= 1)
            {
                if(previousRotate.Count > 1 && previousRotate.Contains(positionHistory[positionHistory.Count - 1]))
                {
                    if(transform.parent == null)
                    {
                        transform.rotation = Quaternion.Euler(positionHistory[positionHistory.Count - 1]);
                        rotateDirection = positionHistory[positionHistory.Count - 1];
                    }

                    previousRotate.RemoveAt(previousRotate.Count - 1);
                }

                else
                {
                    if(Math.Abs(positionHistory[positionHistory.Count - 1].x) < 0.1f)
                    transform.localPosition = positionHistory[positionHistory.Count - 1];

                    else
                    {
                        transform.parent = null;
                        transform.localPosition = positionHistory[positionHistory.Count - 1];
                    }
                }

                if(positionHistory.Count > 1)
                positionHistory.RemoveAt(positionHistory.Count - 1);

                previousPosition = transform.position;

                return;
            }
        }
    }
}
