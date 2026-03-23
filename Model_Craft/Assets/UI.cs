using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private Player playerScript;
    private InventoryManager inventoryManager;
    //public GameObject[] cell;
    public GameObject instructionBlock;
    public GameObject blocksCatalog;
    //private GameObject inventoryIcon;
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
        playerScript.selectedItem = 0;
    }

    public void SelectItem(int previousInventoryNumber, int currentInventoryNumber, GameObject[] cell)
    {
        if(previousInventoryNumber != 0)
        cell[previousInventoryNumber-1].GetComponent<Image>().material = null;

        cell[currentInventoryNumber-1].GetComponent<Image>().material = Camera.main.GetComponent<MainScript>().outlineMaterial;
    }

    public void SpawnBlock()
    {   
        foreach(GameObject bucket in inventoryManager.cell)
        {
            if(bucket.GetComponent<Image>().material == Camera.main.GetComponent<MainScript>().outlineMaterial && bucket.transform.Find("Count").GetComponent<TMP_Text>().text != "")
            {
                playerScript.selectedItem = int.Parse(bucket.transform.name[int.Parse(bucket.transform.name.Length.ToString()) - 1].ToString());
                Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.transform.position + Camera.main.transform.forward * playerScript.distance,
                bucket.transform.Find("Name").GetComponent<TMP_Text>().text,
                Camera.main.GetComponent<MainScript>().blockPrefabs[inventoryManager.keys[playerScript.selectedItem - 1].Split(" ")[0]],
                inventoryManager.materials[bucket.transform.Find("Name").GetComponent<TMP_Text>().text][inventoryManager.materials[bucket.transform.Find("Name").GetComponent<TMP_Text>().text].Count - 1]);
                inventoryManager.RemoveBlockfromInventory(playerScript.selectedItem, playerScript.previousSelectedItem);
                return;
            }
        }
    }

    public void SpawnBlockButton()
    {
        Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance)),
        inventoryManager.keys[playerScript.selectedItem - 1],
        Camera.main.GetComponent<MainScript>().blockPrefabs[inventoryManager.keys[playerScript.selectedItem - 1].Split(" ")[0]],
        inventoryManager.materials[inventoryManager.keys[playerScript.selectedItem - 1]][inventoryManager.materials[inventoryManager.keys[playerScript.selectedItem - 1]].Count - 1]);
        inventoryManager.RemoveBlockfromInventory(playerScript.selectedItem, playerScript.previousSelectedItem);
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
        if(Input.GetKey(KeyCode.B))
        {
            if(blockList.gameObject.activeInHierarchy)
            blockList.gameObject.SetActive(false);
            
            else
            blockList.gameObject.SetActive(true);
        }
    }
}
