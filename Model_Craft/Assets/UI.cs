using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private Player playerScript;
    private InventoryManager inventoryManager;
    public GameObject instructionBlock;
    public GameObject blocksCatalog;
    private GameObject cursor;
    public Button blockList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        GameObject inventory = GameObject.FindGameObjectWithTag("InventoryManager");
        if(inventory != null)
        inventoryManager = inventory.GetComponent<InventoryManager>();

        cursor = GameObject.Find("Cursor");

        if(playerScript.isBuildMode)
        blockList.gameObject.SetActive(true);
    }

    public void MakeOutline(Transform marker)
    {
        foreach(GameObject image in inventoryManager.cell)
        image.GetComponent<Image>().material = null;
        
        marker.GetComponent<Image>().material = Camera.main.GetComponent<MainScript>().outlineMaterial;
    }

    public void MakeNone(Transform marker)
    {
        marker.GetComponent<Image>().material = null;
        //playerScript.selectedItem = 0;
    }

    public void SetSelectedItem()
    {
        foreach(GameObject bucket in inventoryManager.cell)
        {
            if(bucket.GetComponent<Image>().material == Camera.main.GetComponent<MainScript>().outlineMaterial && !playerScript.colorListPanel.gameObject.activeInHierarchy)
            playerScript.selectedItem = int.Parse(bucket.name[bucket.name.Length - 1].ToString());
        }
    }

    public void SelectItem(int previousInventoryNumber, int currentInventoryNumber, GameObject[] cell)
    {
        if(previousInventoryNumber != 0)
        cell[previousInventoryNumber-1].GetComponent<Image>().material = null;

        cell[currentInventoryNumber-1].GetComponent<Image>().material = Camera.main.GetComponent<MainScript>().outlineMaterial;
    }

    public void OpenCloseInventory()
    {
        for(int i = 0; i <= inventoryManager.cell.Length - 1; i++)
        {
            if(inventoryManager.cell[i].activeInHierarchy)
            inventoryManager.cell[i].SetActive(false);
            
            else
            inventoryManager.cell[i].SetActive(true);
        }
    }

    public void OpenCloseInstruction()
    {
        if(instructionBlock.activeInHierarchy)
        instructionBlock.SetActive(false);
        else
        instructionBlock.SetActive(true);
    }

    public void OpenCloseBlockList()
    {
        if(blocksCatalog.activeInHierarchy)
        blocksCatalog.SetActive(false);
        else
        blocksCatalog.SetActive(true);
    }

    void FixedUpdate()
    {
        if(playerScript.isBuildMode)
        cursor.gameObject.SetActive(false);
        else
        cursor.gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        Transform colorChoosePanel = transform.Find("AdvancesColorPickerPanelPrefab(Clone)");
        
        if(Input.GetKey(KeyCode.B) && playerScript.typeGame == "CareerMode")
        {   
            if(colorChoosePanel == null || !colorChoosePanel.gameObject.activeInHierarchy)
            {
                if(blockList.gameObject.activeInHierarchy)
                blockList.gameObject.SetActive(false);
            
                else
                blockList.gameObject.SetActive(true);
            }

            else
            return;
        }
    }
}
