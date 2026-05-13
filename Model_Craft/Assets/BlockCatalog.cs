using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Security.Cryptography;

public class BlockCatalog : MonoBehaviour
{
    private Player playerScript;
    public GameObject typeSelectionPanel;
    public GameObject cardsPanel;
    public Transform cardsContainer;
    public GameObject cardPrefab;
    public Button[] typeButtons;
    public Button backButton;
    public BlockData[] allBlocks;
    private List<BlockData> currentFilteredBlocks = new List<BlockData>();
    public ColorSelector colorSelector;
    public ScrollRect scrollRect;
    private BlockData selectedBlock;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(player != null)
        playerScript = player.GetComponent<Player>();

        for(int i = 0; i < typeButtons.Length; i++)
        {
            int index = i; // важно для лямбды
            typeButtons[i].onClick.AddListener(() => ShowType((BlockType)index));
        }

        if(backButton != null)
        backButton.onClick.AddListener(ShowTypeSelection);

        ShowTypeSelection();
    }

    public void ShowTypeSelection()
    {
        if(!playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            if(typeSelectionPanel != null)
            {
                typeSelectionPanel.SetActive(true);
                backButton.gameObject.SetActive(false);
            }

            if(cardsPanel != null)
            cardsPanel.SetActive(false);
        }
    }

    public void ShowType(BlockType type)
    {
        if(!playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            currentFilteredBlocks.Clear();
            
            foreach(var block in allBlocks)
            {
                if(block.type == type)
                currentFilteredBlocks.Add(block);
            }

            if(typeSelectionPanel != null)
            typeSelectionPanel.SetActive(false);
        
            if(cardsPanel != null)
            {
                cardsPanel.SetActive(true);
                backButton.gameObject.SetActive(true);
            }

            RefreshCatalog();
        }
    }

    void RefreshCatalog()
    {
        foreach (Transform child in cardsContainer)
        Destroy(child.gameObject);
        
        foreach(var blockData in currentFilteredBlocks)
        {
            GameObject cardGO = Instantiate(cardPrefab, cardsContainer);
            BlockCard card = cardGO.GetComponent<BlockCard>();
            
            if(card != null)
            card.Setup(blockData, OnBlockSelected);

            if(UISkinManager.Instance != null)
            UISkinManager.Instance.ApplyToGameObject(cardGO);
        }
    }

    public void OnBlockSelected(BlockData selectedBlock)
    {
        if(!playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {
            Debug.Log($"Выбран блок: {selectedBlock.type.ToString()} {selectedBlock.blockName}");
        
            if(selectedBlock.prefab == null)
            {
                Debug.LogWarning("У блока нет префаба!");
                return;
            }

            if(colorSelector != null)
            {
                colorSelector.ShowColorPicker((Material selectedMaterial) => {
                Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.transform.position + Camera.main.transform.forward * playerScript.distance,
                selectedBlock.type + " " + selectedBlock.blockName, Camera.main.GetComponent<MainScript>().blockPrefabs[selectedBlock.type.ToString()],
                Camera.main.GetComponent<MainScript>().standartMaterial, new Vector3(0.0f, 0.0f, 0.0f));
                Camera.main.GetComponent<MainScript>().newBlock.GetComponent<Renderer>().material = selectedMaterial;
                });
            }
            
            else
            {
                Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.transform.position + Camera.main.transform.forward * playerScript.distance,
                selectedBlock.type + " " + selectedBlock.blockName, Camera.main.GetComponent<MainScript>().blockPrefabs[selectedBlock.type.ToString()],
                Camera.main.GetComponent<MainScript>().standartMaterial, new Vector3(0.0f, 0.0f, 0.0f));
            }
        }
    }
}