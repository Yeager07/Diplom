using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

public class Player : MonoBehaviour
{
    public string typeGame = "MainMenu";
    public string language = "En";
    public Transform colorChoosePanel;
    public Transform colorListPanel;
    public Transform instruction;
    private InventoryManager inventoryManager;
    private float speed = 4.0f;
    public float minDistance = 1.0f;
    private float maxDistance = 10f;
    public float distance = 0.0f;
    public float currentDistance = 0.0f;
    private float buildSpeed = 25.0f;
    private float speedRot = 1.5f;
    private float verRotLim = 60.0f;
    private float speedBuildRot = 3.0f;
    private Rigidbody rigidBody;
    private Vector3 targetOffset = Vector3.zero;
    public Vector3 targetPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 moveDirection;
    public GameObject movedObject;
    public int selectedItem = 0;
    public int previousSelectedItem = 0;
    public Vector3 rotateDirection;
    public bool isBuildMode = false;
    public Vector3 target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        GameObject inventory = GameObject.FindGameObjectWithTag("InventoryManager");
        if(inventory != null)
        inventoryManager = inventory.GetComponent<InventoryManager>();
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

        if(movedObject == null)
        {
            distance -= Input.GetAxis("Mouse ScrollWheel") * speed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            
            currentDistance -= Input.GetAxis("Mouse ScrollWheel") * speed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
            
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

        //if(movedObject == null)
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);

        /*else
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);*/

        if(Input.GetMouseButtonUp(1) && target != new Vector3(0.0f, 0.0f, 0.0f))
        {
            targetPosition = target;
            currentDistance = minDistance;
        }
        
        moveDirection = Quaternion.Euler(rotateDirection) * negDistance + targetPosition;

        transform.rotation = Quaternion.Euler(rotateDirection);
        transform.position = moveDirection;
    }

    public void OutlinedSelectedItem()
    {
        if(selectedItem != 0)
        transform.Find("UI").GetComponent<UI>().SelectItem(previousSelectedItem, selectedItem, inventoryManager.cell);

        else
        {
            if(previousSelectedItem != 0)
            inventoryManager.cell[previousSelectedItem - 1].GetComponent<Image>().material = null;

            else
            inventoryManager.cell[previousSelectedItem].GetComponent<Image>().material = null;
        }
    }

    void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {   
        if(typeGame != "MainMenu")
        {
            colorChoosePanel = transform.Find("UI").transform.Find("AdvancesColorPickerPanelPrefab(Clone)");

            if(Input.GetMouseButtonUp(1) && selectedItem != 0 && 
            inventoryManager.cell[selectedItem - 1].GetComponent<Image>().material == Camera.main.GetComponent<MainScript>().outlineMaterial &&
            !transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
            {
                inventoryManager.inventory.Remove(inventoryManager.keys[selectedItem-1]);
                inventoryManager.materialsCount.Remove(inventoryManager.keys[selectedItem - 1]);
                inventoryManager.keys[selectedItem - 1] = "";
                inventoryManager.values[selectedItem - 1] = "";
                inventoryManager.UpdateInventoryView();
                inventoryManager.cell[selectedItem - 1].GetComponent<Image>().sprite = null;
                colorListPanel.GetComponent<InventoryCatalog>().RefreshCatalog();
                colorListPanel.GetComponent<InventoryCatalog>().ClosePanel();
            }

            if(Input.GetKeyUp(KeyCode.I) && !Input.GetKey(KeyCode.LeftControl) &&
            !transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
            {
                if(colorChoosePanel == null || !colorChoosePanel.gameObject.activeInHierarchy)
                {
                    if(inventoryManager.cell[0].activeInHierarchy &&
                    !transform.Find("UI").GetComponent<UI>().instructionBlock.activeInHierarchy)
                    transform.Find("UI").GetComponent<UI>().OpenCloseInventory();
            
                    else if(transform.Find("UI").GetComponent<UI>().instructionBlock.activeInHierarchy &&
                    !inventoryManager.cell[0].activeInHierarchy)
                    transform.Find("UI").GetComponent<UI>().OpenCloseInstruction();

                    else
                    {
                        transform.Find("UI").GetComponent<UI>().OpenCloseInventory();
                        transform.Find("UI").GetComponent<UI>().OpenCloseInstruction();
                    }
                }

                else
                return;
            }

            if(!transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
            {
                if(!isBuildMode)
                Move();

                else
                MoveBuildMode();
            }
        }
    }
}
