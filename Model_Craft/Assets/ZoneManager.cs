using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    public GameObject tableUIPanel;
    public GameObject cabinetUIPanel;
    public GameObject settingsUIPanel;
    public GameObject exitUIPanel;

    public GameObject[] levelPreviews;
    public Transform previewSpawnPoint;
    public Button nextLevelButton;
    public Button prevLevelButton;

    public Slider volumeSlider;
    public Button languageButton;

    public Button confirmExitButton;
    public Button cancelExitButton;

    private int currentLevelIndex = 0;
    private GameObject currentPreviewInstance;
    private CameraMovement camMovement;

    public GameObject confirmLoadPanel;
    public Button continueButton;
    public Button newGameButton;

    public static FreeModeSaveData PendingFreeModeSave;
    public static CareerSaveData PendingCareerSave;

    public ModelPreview modelPreview;

    void Awake()
    {
        if(Instance == null)
        Instance = this;
        
        else
        Destroy(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = FindFirstObjectByType<Player>();
     
        if(player != null && player.typeGame == "MainMenu")
        {
            enabled = true;
            HideAllPanels();
        }
        
        else
        enabled = false;
    }

    void Start()
    {

        Player player = FindFirstObjectByType<Player>();
        
        if(player == null || player.typeGame != "MainMenu")
        {
            Debug.Log("ZoneManager: Not in main menu, disabling.");
            enabled = false;
            return;
        }

        camMovement = Camera.main?.GetComponent<CameraMovement>();

        HideAllPanels();

        if(nextLevelButton)
        nextLevelButton.onClick.AddListener(NextLevel);
        
        if(prevLevelButton)
        prevLevelButton.onClick.AddListener(PreviousLevel);
        
        if(volumeSlider)
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
        
        if(languageButton)
        languageButton.onClick.AddListener(ChangeLanguage);
        
        if(confirmExitButton)
        confirmExitButton.onClick.AddListener(ExitGame);
        
        if(cancelExitButton)
        cancelExitButton.onClick.AddListener(() => exitUIPanel.SetActive(false));
    }

    public void HideAllPanels()
    {
        if(tableUIPanel != null && tableUIPanel.activeSelf)
        ClearTablePreview();

        if(tableUIPanel)
        tableUIPanel.SetActive(false);
        
        if(cabinetUIPanel)
        {
            GalleryView galleryView = FindFirstObjectByType<GalleryView>();
            
            cabinetUIPanel.SetActive(false);
            galleryView.CloseGallery();
            modelPreview.HidePreview();
        }
        
        if(settingsUIPanel)
        settingsUIPanel.SetActive(false);
        
        if(exitUIPanel)
        exitUIPanel.SetActive(false);
    }

    public void OnCameraArrived(int zoneIndex)
    {
        switch (zoneIndex)
        {
            case 1: ShowTableUI(); break;
            case 2: ShowCabinetUI(); break;
            case 3: ShowSettingsUI(); break;
            case 4: ShowExitUI(); break;
            default: break;
        }
    }

    private void ShowTableUI()
    {
        tableUIPanel.SetActive(true);
        ClearTablePreview();
        UpdateLevelPreview();
    }

    public void ShowCabinetUI()
    {
        cabinetUIPanel.SetActive(true);
        GalleryView galleryView = FindFirstObjectByType<GalleryView>();
        
        if(galleryView != null)
        galleryView.ShowGallery();
    }

    private void ShowSettingsUI()
    {
        if(settingsUIPanel)
        settingsUIPanel.SetActive(true);
        
        if(volumeSlider)
        volumeSlider.value = AudioListener.volume;
    }

    private void ShowExitUI()
    {
        if(exitUIPanel)
        exitUIPanel.SetActive(true);
    }

    private void UpdateLevelPreview()
    {
        if(currentPreviewInstance != null)
        Destroy(currentPreviewInstance);

        if(levelPreviews.Length > currentLevelIndex && previewSpawnPoint != null)
        {
            currentPreviewInstance = Instantiate(levelPreviews[currentLevelIndex], previewSpawnPoint.position, previewSpawnPoint.rotation);
            
            RotateObject rot = currentPreviewInstance.GetComponent<RotateObject>();
            
            if(rot == null)
            rot = currentPreviewInstance.AddComponent<RotateObject>();
            
            LevelPreviewClick click = currentPreviewInstance.AddComponent<LevelPreviewClick>();
            click.levelIndex = currentLevelIndex;
        }

        if(prevLevelButton)
        prevLevelButton.interactable = (currentLevelIndex > 0);
        
        if(nextLevelButton)
        nextLevelButton.interactable = (currentLevelIndex < levelPreviews.Length - 1);
    }

    private void ClearTablePreview()
    {
        if(currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }
    }

    private void NextLevel()
    {
        if(currentLevelIndex < levelPreviews.Length - 1)
        {
            currentLevelIndex++;
            UpdateLevelPreview();
        }
    }

    private void PreviousLevel()
    {
        if(currentLevelIndex > 0)
        {
            currentLevelIndex--;
            UpdateLevelPreview();
        }
    }

    private void StartCareerMode()
    {
        LevelData levelData = GetLevelDataByIndex(currentLevelIndex);
        string levelId = levelData.levelName;

        if(SaveManager.Instance.HasCareerSave(levelId))
        ShowCareerLoadDialog(levelId, levelData);
        
        else
        LoadCareerLevel(levelId, levelData, null);
    }

    private void ShowCareerLoadDialog(string levelId, LevelData levelData)
    {
        confirmLoadPanel.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        newGameButton.onClick.RemoveAllListeners();
     
        continueButton.onClick.AddListener(() => {
            confirmLoadPanel.SetActive(false);
            CareerSaveData save = SaveManager.Instance.LoadCareerMode(levelId);
            LoadCareerLevel(levelId, levelData, save);
        });
     
        newGameButton.onClick.AddListener(() => {
            confirmLoadPanel.SetActive(false);
            SaveManager.Instance.DeleteCareerSave(levelId);
            LoadCareerLevel(levelId, levelData, null);
        });
    }

    private void LoadCareerLevel(string levelId, LevelData levelData, CareerSaveData save)
    {
        CameraMovement camMove = Camera.main.GetComponent<CameraMovement>();
        
        if(camMove != null)
        {
            camMove.transform.position = camMove.cameraPoints[0].position;
            camMove.transform.rotation = camMove.cameraPoints[0].rotation;
        }

        LevelLoader.SelectedLevel = levelData;

        Cursor.lockState = CursorLockMode.Locked;
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.typeGame = "CareerMode";
        player.isBuildMode = false;
        player.transform.Find("UI").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Instruction").transform.Find("InstructionDownload").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Instruction").gameObject.SetActive(true);
        Camera.main.GetComponent<MainScript>().PlacePlayerZero();

        LevelLoader.SelectedLevel = levelData;

        ZoneManager.PendingCareerSave = save;

        LevelStepManager.IsLoadingSave = (save != null); 
        SceneManager.LoadScene("02_TestScene");
    }

    public void StartFreeMode()
    {
        if(SaveManager.Instance.HasFreeModeSave())
        ShowFreeModeLoadDialog();
        
        else    
        StartFreeModeNewGame();
    }

    private void ShowFreeModeLoadDialog()
    {
        confirmLoadPanel.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        newGameButton.onClick.RemoveAllListeners();
        
        continueButton.onClick.AddListener(() => {
            confirmLoadPanel.SetActive(false);
            StartFreeModeContinue();
        });
        
        newGameButton.onClick.AddListener(() => {
            confirmLoadPanel.SetActive(false);
            StartFreeModeNewGame();
        });
    }

    private void ClearAllBlocks()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Selectable");
        
        foreach(GameObject block in blocks)
        Destroy(block);

        Block.connections.Clear();
    }

    private void StartFreeModeContinue()
    {
        PendingFreeModeSave = SaveManager.Instance.LoadFreeMode();
        SetupPlayerForFreeMode();
        SceneManager.LoadScene("04_FreeMode");
    }
    
    public void StartFreeModeNewGame()
    {
        SaveManager.Instance.DeleteFreeModeSave();
        PendingFreeModeSave = null;
        SetupPlayerForFreeMode();
        SceneManager.LoadScene("04_FreeMode");
    }

    private void SetupPlayerForFreeMode()
    {
        Player player = FindFirstObjectByType<Player>();
        player.typeGame = "FreeMode";
        player.isBuildMode = true;
        
        Transform ui = player.transform.Find("UI");
        ui.gameObject.SetActive(true);
        ui.Find("Instruction").Find("InstructionDownload").gameObject.SetActive(true);
        ui.Find("Instruction").gameObject.SetActive(false);
        ui.Find("BlocksIcon").gameObject.SetActive(true);
        
        CameraMovement camMove = Camera.main.GetComponent<CameraMovement>();
     
        if(camMove != null)
        {
            camMove.transform.position = camMove.cameraPoints[0].position;
            camMove.transform.rotation = camMove.cameraPoints[0].rotation;
        }
        
        Camera.main.GetComponent<MainScript>().PlacePlayerZero();
    }

    private LevelData GetLevelDataByIndex(int index)
    {
        return Camera.main.GetComponent<MainScript>().levelDatas[index];
    }

    private void ChangeVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ChangeLanguage()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        
        if(player.language == "En")
        player.language = "Ru";
        
        else
        player.language = "En";

        LocalizationManager.Instance.SetLanguage(player.language);
    }

    public void StartCareerModeByIndex(int index)
    {
        currentLevelIndex = index;
        StartCareerMode();
    }

    public void MoveToCenter()
    {
        if(camMovement != null)
        camMovement.MoveToPoint(0); // 0 – индекс центральной позиции

        HideAllPanels();
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        ClearTablePreview();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}