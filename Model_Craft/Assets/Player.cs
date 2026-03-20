using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

public class Player : MonoBehaviour
{
    private float speed = 4.0f;
    public float minDistance = 1.0f;
    private float maxDistance = 10f;
    private float buildSpeed = 25.0f;
    private float speedRot = 1.5f;
    private float verRotLim = 60.0f;
    private float speedBuildRot = 3.0f;
    private Rigidbody rigidBody;
    private Vector3 targetOffset = Vector3.zero;
    private Vector3 targetPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 moveDirection;
    public GameObject movedObject;
    public int selectedItem = 0;
    public int previousSelectedItem = 0;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public string[] keys;
    public string[] values;
    public Dictionary<string, List<Material>> materials = new Dictionary<string, List<Material>>();
    public List<Material> materials2 = new List<Material>();
    public int lengthDictionary;
    public Vector3 rotateDirection;
    public bool isBuildMode = false;
    public float distance = 0.0f;
    public Vector3 target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Move()
    {
        GetComponent<MeshRenderer>().enabled = !isBuildMode;
        rigidBody.constraints = RigidbodyConstraints.FreezePositionY;

        rotateDirection.x -= speedRot * Input.GetAxis("Mouse Y");
        rotateDirection.y += speedRot * Input.GetAxis("Mouse X");
        rotateDirection.z = 0;

        if(rotateDirection.x < -verRotLim)
        rotateDirection.x = -verRotLim;

        if(rotateDirection.x > verRotLim)
        rotateDirection.x = verRotLim;

        moveDirection = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");

        rigidBody.MovePosition(rigidBody.position + moveDirection * speed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(rotateDirection);
    }
    
    void MoveBuildMode()
    {
        rigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
        distance -= Input.GetAxis("Mouse ScrollWheel") * speed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
            
        if(Input.GetMouseButton(2))
        {
            if(Input.GetKey(KeyCode.LeftShift))
            targetPosition += (-transform.up * Input.GetAxis("Mouse Y") - transform.right * Input.GetAxis("Mouse X")) * Time.deltaTime * buildSpeed;

            else
            {
                rotateDirection.x -= speedBuildRot * Input.GetAxis("Mouse Y");
                rotateDirection.y += speedBuildRot * Input.GetAxis("Mouse X");
                rotateDirection.z = 0;
            }
        }

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);

        if(Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            targetPosition = target;
            distance = minDistance;
        }
        
        moveDirection = Quaternion.Euler(rotateDirection) * negDistance + targetPosition;

        transform.rotation = Quaternion.Euler(rotateDirection);
        transform.position = moveDirection;
    }

    public void RemoveBlockfromInventory()
    {
        if(keys[selectedItem - 1] != "")
        {
            if(inventory[keys[selectedItem-1]] != 1)
            {
                inventory[keys[selectedItem - 1]] -= 1;
                materials[keys[selectedItem - 1]].RemoveAt(materials[keys[selectedItem - 1]].Count - 1);
                values[selectedItem - 1] = (int.Parse(values[selectedItem - 1]) - 1).ToString();
            }

            else
            {
                inventory.Remove(keys[selectedItem-1]);
                materials.Remove(keys[selectedItem - 1]);
                keys[selectedItem - 1] = "";
                values[selectedItem - 1] = "";
                previousSelectedItem = selectedItem;
                selectedItem = 0;
                transform.Find("UI").GetComponent<UI>().MakeNone(transform.Find("UI").GetComponent<UI>().cell[previousSelectedItem - 1].transform);
            }

            transform.Find("UI").GetComponent<UI>().UpdateInventoryView();
            OutlinedSelectedItem();
        }
        else
        return;
    }

    public void OutlinedSelectedItem()
    {
        if(selectedItem != 0)
        transform.Find("UI").GetComponent<UI>().SelectItem(previousSelectedItem, selectedItem);

        else
        {
            if(previousSelectedItem != 0)
            transform.Find("UI").GetComponent<UI>().cell[previousSelectedItem - 1].GetComponent<Image>().material = null;

            else
            transform.Find("UI").GetComponent<UI>().cell[previousSelectedItem].GetComponent<Image>().material = null;
        }
    }

    private void SelectItem()
    {
        if(Input.GetKey(KeyCode.Alpha1))
        {
            previousSelectedItem = selectedItem;
            selectedItem = 1;
            OutlinedSelectedItem();
        }
        
        if(Input.GetKey(KeyCode.Alpha2))
        {
            previousSelectedItem = selectedItem;
            selectedItem = 2;
            OutlinedSelectedItem();
        }
            
        if(Input.GetKey(KeyCode.Alpha3))
        {
            previousSelectedItem = selectedItem;
            selectedItem = 3;
            OutlinedSelectedItem();
        }
            
        if(Input.GetKey(KeyCode.Alpha4))
        {
            previousSelectedItem = selectedItem;
            selectedItem = 4;
            OutlinedSelectedItem();
        }

        if(Input.GetKey(KeyCode.Alpha5))
        {
            previousSelectedItem = selectedItem;
            selectedItem = 5;
            OutlinedSelectedItem();
        }
    }

    /*void FixedUpdate()
    {
    }*/

    // Update is called once per frame
    void Update()
    {
        SelectItem();

        if(Input.GetKey(KeyCode.KeypadEnter) && selectedItem != 0 && isBuildMode)
        {
            if(keys[selectedItem - 1] != "")
            transform.Find("UI").GetComponent<UI>().SpawnBlockButton();

            transform.Find("UI").GetComponent<UI>().MakeNone(transform.Find("UI").GetComponent<UI>().cell[previousSelectedItem - 1].transform);
        }

        if(Input.GetKeyUp(KeyCode.Delete) && selectedItem != 0)
        RemoveBlockfromInventory();

        if(Input.GetKeyUp(KeyCode.I))
        {
            if(transform.Find("UI").GetComponent<UI>().cell[0].activeInHierarchy &&
            !transform.Find("UI").GetComponent<UI>().instructionBlock.activeInHierarchy)
            transform.Find("UI").GetComponent<UI>().OpenCloseInventory();
            
            else if(transform.Find("UI").GetComponent<UI>().instructionBlock.activeInHierarchy &&
            !transform.Find("UI").GetComponent<UI>().cell[0].activeInHierarchy)
            transform.Find("UI").GetComponent<UI>().OpenCloseInstruction();

            else
            {
                transform.Find("UI").GetComponent<UI>().OpenCloseInventory();
                transform.Find("UI").GetComponent<UI>().OpenCloseInstruction();
            }
        }

        if(!isBuildMode)
        Move();

        else
        MoveBuildMode();
    }
}
