using System.Security;
using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class InventoryCard : MonoBehaviour
{
    private Player playerScript;
    private InventoryManager inventoryManager;
    private UI uiScript;
    private GameObject ui;
    public Image iconColor;
    public TextMeshProUGUI countText;
    public Button generateButton;   
    public Button deleteButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        GameObject inventory = GameObject.FindGameObjectWithTag("InventoryManager");
        if(inventory != null)
        inventoryManager = inventory.GetComponent<InventoryManager>();

        ui = GameObject.FindGameObjectWithTag("UI");
        if(ui != null)
        uiScript = ui.GetComponent<UI>();
    }

    public void Setup(Material dataMaterial, int dataCount)
    {
        // Заполняем UI
        if(iconColor != null)
        iconColor.GetComponent<Image>().material = dataMaterial;
        
        if(countText != null)
        countText.text = "Count:" + dataCount.ToString();
    }

    public void GenerateBlock()
    {
        Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.transform.position + Camera.main.transform.forward * playerScript.distance,
        inventoryManager.keys[playerScript.selectedItem - 1], Camera.main.GetComponent<MainScript>().blockPrefabs[inventoryManager.keys[playerScript.selectedItem - 1].Split(" ")[0]],
        iconColor.GetComponent<Image>().material);
    }

    public void DeleteBLock()
    {
        if(inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]][iconColor.GetComponent<Image>().material] != 0)
        inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]][iconColor.GetComponent<Image>().material] -= 1;

        else
        {
            inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]].Remove(iconColor.GetComponent<Image>().material);
            inventoryManager.RemoveBlockfromInventory(playerScript.selectedItem, playerScript.previousSelectedItem);
        }

        ui.transform.Find("ColorListPanel").GetComponent<InventoryCatalog>().RefreshCatalog();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
