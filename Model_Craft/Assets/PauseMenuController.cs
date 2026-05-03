using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public GameObject saveDialogPanel;
    public TMP_InputField modelNameInput;
    public Button saveModelButton;
    public GameObject successSavePanel;

    public GameObject photoModePanel;
    public Button takePhotoButton;
    public Button cancelPhotoButton;
    public GameObject photoReviewPanel;
    public RawImage photoPreviewImage;
    public Button acceptPhotoButton;
    public Button retakePhotoButton;

    private bool isPhotoMode = false;
    private PhotoModeCamera photoCamera;
    private GameObject tempPhotoCenter;
    private Texture2D capturedTexture;
    private string tempThumbnailPath;

    private CameraMovement originalCameraMovement;
    private Transform cameraOriginalParent;
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private Player playerScript;

    private List<Renderer> playerRenderers = new List<Renderer>(); 

    void Start()
    {
        player = FindFirstObjectByType<Player>();
        playerScript = player;
        
        if(pausePanel)
        pausePanel.SetActive(false);
        
        if(controlsImage)
        controlsImage.SetActive(false);

        if(photoModePanel)
        photoModePanel.SetActive(false);
        
        if(photoReviewPanel)
        photoReviewPanel.SetActive(false);

        if(resumeButton)
        resumeButton.onClick.AddListener(ResumeGame);
        
        if(mainMenuButton)
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        if(controlsButton)
        controlsButton.onClick.AddListener(ToggleControls);

        if(takePhotoButton)
        takePhotoButton.onClick.AddListener(TakePhotoAndSave);
        
        if(cancelPhotoButton)
        cancelPhotoButton.onClick.AddListener(CancelPhotoMode);
        
        if(acceptPhotoButton)
        acceptPhotoButton.onClick.AddListener(AcceptPhoto);
        
        if(retakePhotoButton)
        retakePhotoButton.onClick.AddListener(RetakePhoto);

        if(LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged += RefreshUI;

        if(player != null)
        {
            playerRenderers.AddRange(player.GetComponentsInChildren<Renderer>());
        }
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
        if(player.typeGame != "FreeMode")
        saveModelButton.gameObject.SetActive(false);

        else
        saveModelButton.gameObject.SetActive(true);

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

        if(isPhotoMode)
        ExitPhotoMode(false);

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

        else if(player.typeGame == "FreeMode")
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

    public void ShowSaveDialog()
    {
        EnterPhotoMode();
    }

    public void OnSaveConfirm()
    {
        string modelName = modelNameInput.text;
     
        if(string.IsNullOrEmpty(modelName))
        modelName = "Модель " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        SaveCurrentFreeModeModelWithThumbnail(modelName, tempThumbnailPath);
        saveDialogPanel.SetActive(false);
        successSavePanel.SetActive(true);
        tempThumbnailPath = null;
    }

    public void OnConfirm()
    {
        successSavePanel.SetActive(false);
        ResumeGame();
    }

    public void OnSaveCancel()
    {
        saveDialogPanel.SetActive(false);
        pausePanel.SetActive(true);
        ResumeGame();
    }

    private void SaveCurrentFreeModeModelWithThumbnail(string modelName, string thumbnailPath)
    {
        List<BlockSaveData> rootBlocks = SaveManager.Instance.CollectRootBlocks();
     
        if(rootBlocks.Count == 0)
        {
            Debug.Log("Нет блоков на сцене для сохранения.");
            return;
        }
        
        SaveManager.Instance.SaveFreeModeModelToGallery(modelName, rootBlocks, thumbnailPath);
        Debug.Log("Модель сохранена в галерею с миниатюрой!");
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

    public void EnterPhotoMode()
    {
        if(player.typeGame != "FreeMode")
        return;

        player.transform.Find("UI").transform.Find("BlockCatalog").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("BlocksIcon").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("InventoryIcon").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Instruction").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("IconInstruction").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Inventory1").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Inventory2").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Inventory3").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Inventory4").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("Inventory5").gameObject.SetActive(false);
        player.transform.Find("UI").transform.Find("ColorListPanel").gameObject.SetActive(false);
        
        foreach(var rend in playerRenderers)
        rend.enabled = false;

        pausePanel.SetActive(false);
        photoModePanel.SetActive(true);
        isPhotoMode = true;
        // Time.timeScale уже 0, оставляем

        if(playerScript != null)
        playerScript.enabled = false;
        
        originalCameraMovement = Camera.main.GetComponent<CameraMovement>();
        
        if(originalCameraMovement != null)
        originalCameraMovement.enabled = false;

        cameraOriginalParent = Camera.main.transform.parent;
        Camera.main.transform.SetParent(null);
        originalCameraPos = Camera.main.transform.position;
        originalCameraRot = Camera.main.transform.rotation;

        photoCamera = Camera.main.GetComponent<PhotoModeCamera>();

        if(photoCamera == null)
        photoCamera = Camera.main.gameObject.AddComponent<PhotoModeCamera>();


        GameObject centerObj = new GameObject("PhotoCenter");
        Vector3 center = CalculateModelCenter();
        centerObj.transform.position = center;
        tempPhotoCenter = centerObj;
        photoCamera.SetTarget(tempPhotoCenter.transform);

        photoModePanel.SetActive(true);
        photoReviewPanel.SetActive(false);
        pausePanel.SetActive(false);
        isPhotoMode = true;
    }

    private Vector3 CalculateModelCenter()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Selectable");
        
        if(blocks.Length == 0)
        return Vector3.zero;
        
        Vector3 sum = Vector3.zero;
     
        foreach(var block in blocks)
        sum += block.transform.position;
        
        return sum / blocks.Length;
    }

    private void ExitPhotoMode(bool restorePause = true)
    {
        if(!isPhotoMode)
        return;

        if(photoCamera != null)
        Destroy(photoCamera);
        
        if(tempPhotoCenter != null)
        Destroy(tempPhotoCenter);

        Camera.main.transform.SetParent(cameraOriginalParent);
        Camera.main.transform.position = originalCameraPos;
        Camera.main.transform.rotation = originalCameraRot;

        if(originalCameraMovement != null)
        originalCameraMovement.enabled = true;
        
        if(playerScript != null)
        playerScript.enabled = true;

        player.transform.Find("UI").transform.Find("BlocksIcon").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("InventoryIcon").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("IconInstruction").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Inventory1").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Inventory2").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Inventory3").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Inventory4").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Inventory5").gameObject.SetActive(true);    
        
        foreach(var rend in playerRenderers)
        rend.enabled = true;

        photoModePanel.SetActive(false);
        photoReviewPanel.SetActive(false);
        isPhotoMode = false;

        if(restorePause)
        pausePanel.SetActive(true);
    }

    private void TakePhotoAndSave()
    {
        if(!isPhotoMode)
        return;
        
        photoModePanel.SetActive(false);
        StartCoroutine(CaptureThumbnailCoroutine());
    }

    private IEnumerator CaptureThumbnailCoroutine()
    {
        yield return new WaitForEndOfFrame();

        Camera cam = Camera.main;
        int width = 512;
        int height = 512;
        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture prevRT = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        capturedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        capturedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capturedTexture.Apply();

        cam.targetTexture = prevRT;
        RenderTexture.active = null;
        Destroy(rt);

        photoModePanel.SetActive(false);
        photoReviewPanel.SetActive(true);
        photoPreviewImage.texture = capturedTexture;
    }

    private void AcceptPhoto()
    {
        string thumbnailsDir = Path.Combine(Application.persistentDataPath, "Thumbnails");
     
        if(!Directory.Exists(thumbnailsDir))
        Directory.CreateDirectory(thumbnailsDir);
        
        string fileName = Guid.NewGuid().ToString() + ".png";
        tempThumbnailPath = Path.Combine(thumbnailsDir, fileName);
        byte[] bytes = capturedTexture.EncodeToPNG();
        File.WriteAllBytes(tempThumbnailPath, bytes);
        Destroy(capturedTexture);

        ExitPhotoMode(true);
        saveDialogPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    private void RetakePhoto()
    {
        Destroy(capturedTexture);
        photoReviewPanel.SetActive(false);
        photoModePanel.SetActive(true);
    }

    private void CancelPhotoMode()
    {
        ExitPhotoMode(true);
        pausePanel.SetActive(true);
        photoReviewPanel.SetActive(false);
        photoModePanel.SetActive(false);
    }
}