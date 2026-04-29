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
    private Shader myShader;
    private Material applyMaterial;
    private GameObject ui;
    public Image iconColor;
    public TextMeshProUGUI countText;
    public Button generateButton;   
    public Button deleteButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        myShader = Shader.Find("Standard");
        applyMaterial = new Material(myShader);
    }
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

    public void Setup(Color dataColor, int dataCount)
    {
        // Заполняем UI
        if(iconColor != null)
        {
            iconColor.GetComponent<Image>().color = dataColor;
            Awake();
            applyMaterial.color = dataColor;
        }
        
        if(countText != null)
        countText.text = "Count:" + dataCount.ToString();
    }

    public void GenerateBlock()
    {
        string blockName = inventoryManager.keys[playerScript.selectedItem - 1];
        
        if(string.IsNullOrEmpty(blockName))
        return;
        
        if(playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        return;

        Color blockColor = iconColor.GetComponent<Image>().color;
        Debug.Log($"GenerateBlock: blockName={blockName}, color={blockColor}");
        LevelStepManager stepManager = FindFirstObjectByType<LevelStepManager>();

        bool isNeeded = stepManager != null && stepManager.IsBlockColorNeeded(blockName, blockColor);
        
        if(!isNeeded && playerScript.typeGame == "CareerMode")
        {
            Debug.Log("Этот блок не нужен для текущего шага! Используйте Delete, чтобы убрать его.");
            return;
        }

        // Спавним блок в зоне сборки
        Camera.main.GetComponent<MainScript>().SpawnBlock(
            Camera.main.transform.position + Camera.main.transform.forward * playerScript.distance,
            blockName,
            Camera.main.GetComponent<MainScript>().blockPrefabs[blockName.Split(" ")[0]],
            applyMaterial, new Vector3 (0.0f, 0.0f, 0.0f));

        // Уведомляем StepManager, что блок использован
        if(stepManager != null && playerScript.typeGame == "CareerMode")
        stepManager.OnBlockUsed(blockName);

        // Удаляем блок из инвентаря без возврата на стол
        inventoryManager.RemoveBlockFromInventoryNoNotify(playerScript.selectedItem, blockColor);
        ui.transform.Find("ColorListPanel").GetComponent<InventoryCatalog>().RefreshCatalog();
    }
    
    public void DeleteBLock()
    {
        if(!playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            Color blockColor = iconColor.GetComponent<Image>().color;
            string blockName = inventoryManager.keys[playerScript.selectedItem - 1];
            LevelStepManager stepManager = FindFirstObjectByType<LevelStepManager>();
            bool isNeeded = stepManager != null && stepManager.IsBlockColorNeeded(blockName, blockColor);
            
            if(isNeeded && playerScript.typeGame == "CareerMode")
            inventoryManager.RemoveBlockFromInventoryWithNotify(playerScript.selectedItem, blockColor);
            
            else
            inventoryManager.RemoveBlockFromInventoryNoNotify(playerScript.selectedItem, blockColor);
            
            ui.transform.Find("ColorListPanel").GetComponent<InventoryCatalog>().RefreshCatalog();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
