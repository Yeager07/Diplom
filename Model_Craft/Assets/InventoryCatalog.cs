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
        
        RefreshCatalog();
    }

    public void ClosePanel()
    {
        cardsPanel.SetActive(false);
    }

    public void RefreshCatalog()
    {
        // Удаляем все старые карточки
        foreach (Transform child in cardsContainer)
        Destroy(child.gameObject);

        // Создаём новые карточки для отфильтрованных блоков
        if(inventoryManager.keys[playerScript.selectedItem - 1] != "" && inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]].Count != 0)
        {
            foreach(var blockData in inventoryManager.materialsCount[inventoryManager.keys[playerScript.selectedItem - 1]])
            {
                GameObject cardGO = Instantiate(cardPrefab, cardsContainer);
                InventoryCard card = cardGO.GetComponent<InventoryCard>();
            
                if(card != null)
                {
                    Debug.Log($"Создаю карточку для {inventoryManager.keys[playerScript.selectedItem - 1]}, передаю колбэк OnBlockSelected");
                    card.Setup(blockData.Key, blockData.Value);
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
