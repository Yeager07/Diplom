using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
//using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private Player playerScript;
    public GameObject[] cell;
    public GameObject instructionBlock;
    private GameObject inventoryIcon;
    private GameObject cursor;
    //private Material outline;
    public bool isHidden = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        cell = GameObject.FindGameObjectsWithTag("Inventory");
        instructionBlock = GameObject.FindGameObjectWithTag("Instruction");
        cursor = GameObject.Find("Cursor");

        inventoryIcon = transform.Find("InventoryIcon").gameObject;

        if(playerScript.inventory.Count == 0)
        {
            for(int i = 0; i < cell.Length; i++)
            {
                cell[i].transform.Find("Count").gameObject.SetActive(false);
                cell[i].transform.Find("Name").gameObject.SetActive(false);
            }
        }
    }

    public void MakeOutline(Transform marker)
    {
        foreach(GameObject image in cell)
        image.GetComponent<Image>().material = null;
        
        marker.GetComponent<Image>().material = Camera.main.GetComponent<MainScript>().outlineMaterial;
    }

    public void MakeNone(Transform marker)
    {
        marker.GetComponent<Image>().material = null;
        playerScript.selectedItem = 0;
    }

    public void SelectItem(int previousInventoryNumber, int currentInventoryNumber)
    {
        if(previousInventoryNumber != 0)
        cell[previousInventoryNumber-1].GetComponent<Image>().material = null;

        cell[currentInventoryNumber-1].GetComponent<Image>().material = Camera.main.GetComponent<MainScript>().outlineMaterial;
    }

    public void UpdateInventoryView()
    {
        int countBlock = 0;
        Transform allCount = inventoryIcon.transform.Find("Count");

        for(int i = 0; i <= 4; i++)
        {
            Transform count = cell[i].transform.Find("Count");
            Transform name = cell[i].transform.Find("Name");

            count.gameObject.SetActive(true);
            name.gameObject.SetActive(true);
            count.GetComponent<TMP_Text>().text = playerScript.values[i];
            name.GetComponent<TMP_Text>().text = playerScript.keys[i];
            
            if(playerScript.values[i] != "")
            countBlock += int.Parse(playerScript.values[i]);
        }
        
        allCount.GetComponent<TMP_Text>().text = countBlock.ToString();
    }

    public void SpawnBlock()
    {   
        foreach(GameObject bucket in cell)
        {
            if(bucket.GetComponent<Image>().material == Camera.main.GetComponent<MainScript>().outlineMaterial && bucket.transform.Find("Count").GetComponent<TMP_Text>().text != "")
            {
                Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance)),
                bucket.transform.Find("Name").GetComponent<TMP_Text>().text,
                playerScript.materials[int.Parse(bucket.transform.name[int.Parse(bucket.transform.name.Length.ToString()) - 1].ToString()) - 1]);
                playerScript.selectedItem = int.Parse(bucket.transform.name[int.Parse(bucket.transform.name.Length.ToString()) - 1].ToString());
                playerScript.RemoveBlockfromInventory();
                return;
            }
        }
    }

    public void SpawnBlockButton()
    {
        Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance)),
        playerScript.keys[playerScript.selectedItem - 1],
        playerScript.materials[playerScript.selectedItem - 1]);
        playerScript.RemoveBlockfromInventory();
    }

    public void OpenCloseInventory()
    {
        for(int i = 0; i <= cell.Length - 1; i++)
        {
            if(cell[i].activeInHierarchy)
            cell[i].SetActive(false);
            else
            cell[i].SetActive(true);
        }
    }

    public void OpenCloseInstruction()
    {
        if(instructionBlock.activeInHierarchy)
        instructionBlock.SetActive(false);
        else
        instructionBlock.SetActive(true);
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
    }
}
