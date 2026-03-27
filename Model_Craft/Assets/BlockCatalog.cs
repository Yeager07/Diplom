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
    public GameObject typeSelectionPanel;   // Панель с кнопками выбора типа (включая сами кнопки)
    public GameObject cardsPanel;           // Панель с карточками (ScrollView)
    public Transform cardsContainer;        // Content внутри ScrollView
    public GameObject cardPrefab;           // Префаб карточки
    public Button[] typeButtons;            // Кнопки выбора типа
    public Button backButton;               // Кнопка "Назад"
    public BlockData[] allBlocks;           // Все доступные блоки
    private List<BlockData> currentFilteredBlocks = new List<BlockData>();
    public ColorSelector colorSelector;
    public ScrollRect scrollRect;
    private BlockData selectedBlock;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        // Подписываем кнопки типов
        for (int i = 0; i < typeButtons.Length; i++)
        {
            int index = i; // важно для лямбды
            typeButtons[i].onClick.AddListener(() => ShowType((BlockType)index));
        }

        // Подписываем кнопку "Назад"
        if (backButton != null)
            backButton.onClick.AddListener(ShowTypeSelection);

        // Начальное состояние: показываем панель выбора типа
        ShowTypeSelection();
    }

    // Показать панель выбора типа
    public void ShowTypeSelection()
    {
        if (typeSelectionPanel != null)
        {
            typeSelectionPanel.SetActive(true);
            backButton.gameObject.SetActive(false);
        }

        if (cardsPanel != null)
        cardsPanel.SetActive(false);
    }

    // Показать карточки выбранного типа
    public void ShowType(BlockType type)
    {
        // Фильтруем блоки по типу
        currentFilteredBlocks.Clear();
        foreach (var block in allBlocks)
        {
            if (block.type == type)
                currentFilteredBlocks.Add(block);
        }

        // Скрываем панель выбора типа, показываем панель карточек
        if (typeSelectionPanel != null)
        typeSelectionPanel.SetActive(false);
        
        if (cardsPanel != null)
        {
            cardsPanel.SetActive(true);
            backButton.gameObject.SetActive(true);
        }

        // Обновляем карточки
        RefreshCatalog();
    }

    // Очистить старые карточки и создать новые
    void RefreshCatalog()
    {
        // Удаляем все старые карточки
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Создаём новые карточки для отфильтрованных блоков
        foreach (var blockData in currentFilteredBlocks)
        {
            GameObject cardGO = Instantiate(cardPrefab, cardsContainer);
            BlockCard card = cardGO.GetComponent<BlockCard>();
            if (card != null)
            {
                Debug.Log($"Создаю карточку для {blockData.blockName}, передаю колбэк OnBlockSelected");
                card.Setup(blockData, OnBlockSelected);
            }
        }
    }

    // Обработчик клика по карточке
    public void OnBlockSelected(BlockData selectedBlock)
    {
        Debug.Log($"Выбран блок: {selectedBlock.type.ToString()} {selectedBlock.blockName}");
        
        if (selectedBlock.prefab == null)
        {
            Debug.LogWarning("У блока нет префаба!");
            return;
        }

        if (colorSelector != null)
        {
            // Показываем ColorPicker и передаём колбэк для спавна с выбранным материалом
            colorSelector.ShowColorPicker((Material selectedMaterial) => {
            Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.transform.position + Camera.main.transform.forward * playerScript.distance,
            selectedBlock.type + " " + selectedBlock.blockName, Camera.main.GetComponent<MainScript>().blockPrefabs[selectedBlock.type.ToString()], Camera.main.GetComponent<MainScript>().standartMaterial);
            Camera.main.GetComponent<MainScript>().newBlock.GetComponent<Renderer>().material = selectedMaterial;
            });
            //Camera.main.GetComponent<MainScript>().newBlock.GetComponent<Renderer>().material = selectedMaterial;
        }
        else
        {
            // Если ColorSelector нет, спавним с материалом по умолчанию
            Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.transform.position + Camera.main.transform.forward * playerScript.distance,
            selectedBlock.type + " " + selectedBlock.blockName, Camera.main.GetComponent<MainScript>().blockPrefabs[selectedBlock.type.ToString()],
            Camera.main.GetComponent<MainScript>().standartMaterial);
        }
    }
}