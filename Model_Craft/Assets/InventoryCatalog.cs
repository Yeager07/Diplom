using System.Security;
using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Security.Cryptography;

public class InventoryCatalog : MonoBehaviour
{
    private List<Dictionary<Color, int>> currentFilteredBlocks = new List<Dictionary<Color, int>>();
    public Player playerScript;
    public InventoryManager inventoryManager;
    public GameObject cardsPanel;
    private GameObject newCardsPanel;
    public GameObject cardPrefab;
    public Transform cardsContainer; 
    public Button backButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        GameObject inventory = GameObject.FindGameObjectWithTag("InventoryManager");
        if(inventory != null)
        inventoryManager = inventory.GetComponent<InventoryManager>();
    }

    public void OpenPanel(Transform cell)
    {
        cardsPanel.SetActive(true);
        cardsPanel.transform.localPosition = cell.transform.localPosition + new Vector3(0.0f, 230.0f, 0.0f);
        
        if(inventoryManager.keys[playerScript.selectedItem - 1] != "")
        ShowType(inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]]);

        else
        RefreshCatalog();
    }

    public void ClosePanel()
    {
        foreach (Transform child in cardsContainer)
        Destroy(child.gameObject);
        
        cardsPanel.SetActive(false);
    }

    public void ShowType(List<Dictionary<Color, int>> dictionary)
    {
        // Фильтруем блоки по типу
        currentFilteredBlocks.Clear();
        
        foreach (var block in dictionary)
        currentFilteredBlocks.Add(block);

        // Обновляем карточки
        RefreshCatalog();
    }

    public void RefreshCatalog()
    {
        // Удаляем все старые карточки
        foreach (Transform child in cardsContainer)
        Destroy(child.gameObject);

        // Создаём новые карточки для отфильтрованных блоков
        if(inventoryManager.keys[playerScript.selectedItem - 1] != "" && inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]] != null)
        {
            foreach(var blockData in inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]])
            {
                GameObject cardGO = Instantiate(cardPrefab, cardsContainer);
                InventoryCard card = cardGO.GetComponent<InventoryCard>();
            
                foreach(var param in blockData)
                {
                    Debug.Log($"Создаю карточку для {inventoryManager.keys[playerScript.selectedItem - 1]}, передаю колбэк OnBlockSelected");
                    card.Setup(param.Key, param.Value);
                }
            }
        }
        
        else
        Debug.Log($"пустой словарь");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
