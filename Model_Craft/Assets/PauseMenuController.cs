using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject controlsImage;

    public Button resumeButton;
    public Button mainMenuButton;
    public Button controlsButton;

    private bool isPaused = false;
    private bool isControlsOpen = false;

    private Player player;

    void Start()
    {
        player = FindFirstObjectByType<Player>();
        
        if(pausePanel)
        pausePanel.SetActive(false);
        
        if(controlsImage)
        controlsImage.SetActive(false);

        if(resumeButton)
        resumeButton.onClick.AddListener(ResumeGame);
        
        if(mainMenuButton)
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        if(controlsButton)
        controlsButton.onClick.AddListener(ToggleControls);

        if(LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged += RefreshUI;
    }

    void OnDestroy()
    {
        if(LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        if(resumeButton != null)
        resumeButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.GetText("resume");
        
        if(mainMenuButton != null)
        mainMenuButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.GetText("menu");
        
        if(controlsButton != null)
        controlsButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.GetText("controls");
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.P) && player.typeGame != "MainMenu")
        {
            if(isPaused)
            ResumeGame();
            
            else
            PauseGame();
        }
    }

    void PauseGame()
    {
        Debug.Log("PauseGame called");

        isPaused = true;
        Time.timeScale = 0.0f;
        
        if(pausePanel)
        pausePanel.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        if(isControlsOpen)
        CloseControls();

        if(pausePanel)
        pausePanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1.0f;
        
        if(player.isBuildMode)
        Cursor.lockState = CursorLockMode.None;

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void GoToMainMenu()
    {
        if(player.typeGame == "CareerMode")
        SaveCurrentCareerProgress();

        SaveManager.Instance.SaveFreeMode();

        Cursor.lockState = CursorLockMode.None;

        InventoryManager inventoryManager = GameObject.FindFirstObjectByType<InventoryManager>();
        
        pausePanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;

        player.transform.Find("UI").transform.Find("BlockCatalog").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("BlocksIcon").gameObject.SetActive(false);
        player.transform.Find("UI").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Instruction").gameObject.SetActive(false);

        inventoryManager.inventory.Clear();
        inventoryManager.materialsCount.Clear();
        
        for(int iterator = 0; iterator < inventoryManager.keys.Length; iterator++)
        {
            inventoryManager.keys[iterator] = "";
            inventoryManager.values[iterator] = "";
        }

        inventoryManager.UpdateInventoryView();

        Camera.main.GetComponent<MainScript>().PlacePlayerZero();
    
        if(player != null)
        {
            player.typeGame = "MainMenu";
            player.isBuildMode = false;
        }
        
        SceneManager.LoadScene("01_Menu");
    }

    private void SaveCurrentCareerProgress()
    {
        if(LevelLoader.SelectedLevel == null)
        return;

        CareerSaveData data = new CareerSaveData();
        data.levelId = LevelLoader.SelectedLevel.levelName;
        data.currentStepPage = GetCurrentStepPage();

        LevelStepManager stepManager = FindFirstObjectByType<LevelStepManager>();
        
        if(stepManager != null)
        {
            data.completedSteps = stepManager.GetCompletedSteps();
            data.remainingBlocks = stepManager.GetRemainingForStep();
        }
        
        var remaining = stepManager.GetRemainingForStep();
        Debug.Log("Saving remaining blocks:");
        
        foreach(var r in remaining)
        Debug.Log($"{r.blockFullName}: {r.remaining}");
        
        SaveManager.Instance.SaveCareerMode(data.levelId, data);
    }

    private int GetCurrentStepPage()
    {
        PdfInstructionViewer pdf = FindFirstObjectByType<PdfInstructionViewer>();
        return pdf != null ? pdf.currentPageIndex + 1 : 1;
    }

    void ToggleControls()
    {
        if(controlsImage == null)
        return;
        
        isControlsOpen = !controlsImage.activeSelf;
        controlsImage.SetActive(isControlsOpen);
    }

    void CloseControls()
    {
        if(controlsImage != null)
        controlsImage.SetActive(false);
        
        isControlsOpen = false;
    }

    public void CloseControlsButton()
    {
        CloseControls();
    }
}