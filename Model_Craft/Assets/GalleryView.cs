using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;

public class GalleryView : MonoBehaviour
{
    public GameObject galleryPanel;
    public Transform contentContainer;
    public GameObject modelButtonPrefab;

    public CameraMovement cameraMovement;
    public Transform previewStage;
    public GameObject previewUICanvas;
    public Button backButton;
    
    public CareerModelDatabase careerDatabase;

    private List<GalleryModelData> currentModels = new List<GalleryModelData>();
    private GameObject currentPreviewInstance;
    private GalleryModelData selectedModel;
    public bool isPreviewMode = false;

    public GameObject confirmDeletePanel;
    public Button confirmDeleteYesButton;
    public Button confirmDeleteNoButton;

    private const int PREVIEW_POINT_INDEX = 5;
    private const int CABINET_POINT_INDEX = 2;

    public ModelPreview modelPreview;

    private GalleryModelData pendingDeleteModel;

    void Awake()
    {
        if(cameraMovement == null)
        cameraMovement = FindFirstObjectByType<CameraMovement>();

        Player player = FindFirstObjectByType<Player>();
        careerDatabase = player.transform.Find("CareerModelDatabase").gameObject.GetComponent<CareerModelDatabase>();
    }

    void Start()
    {
        if(galleryPanel != null)
        galleryPanel.SetActive(false);
        
        if(previewUICanvas != null)
        previewUICanvas.SetActive(false);
        
        if(backButton != null)
        backButton.onClick.AddListener(ExitPreview);
        
        if(cameraMovement == null)
        cameraMovement = Camera.main.GetComponent<CameraMovement>();
        
        if(cameraMovement != null)
        cameraMovement.OnMoveComplete += OnMoveComplete;

        if(confirmDeletePanel != null)
        {
            confirmDeletePanel.SetActive(false);
            
            if(confirmDeleteYesButton != null)
            confirmDeleteYesButton.onClick.AddListener(OnConfirmDelete);
            
            if(confirmDeleteNoButton != null)
            confirmDeleteNoButton.onClick.AddListener(HideDeleteDialog);
        }
        
        else
        Debug.LogWarning("confirmDeletePanel не назначен в GalleryView!");
    }

    void OnDestroy()
    {
        if(cameraMovement != null)
        cameraMovement.OnMoveComplete -= OnMoveComplete;

        if(confirmDeleteYesButton != null)
        confirmDeleteYesButton.onClick.RemoveListener(OnConfirmDelete);
        
        if(confirmDeleteNoButton != null)
        confirmDeleteNoButton.onClick.RemoveListener(HideDeleteDialog);
    }

    public void ShowGallery()
    {
        LoadAndDisplayModels();
        galleryPanel.SetActive(true);
    }

    public void CloseGallery()
    {
        galleryPanel.SetActive(false);
        HideDeleteDialog();
    }

    private void LoadAndDisplayModels()
    {
        var gallery = SaveManager.Instance.LoadGallery();
        
        if(gallery == null || gallery.models == null)
        currentModels = new List<GalleryModelData>();
        
        else
        currentModels = gallery.models;
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach(Transform child in contentContainer)
        Destroy(child.gameObject);

        foreach(var model in currentModels)
        {
            GameObject btnGO = Instantiate(modelButtonPrefab, contentContainer);
            TMP_Text text = btnGO.GetComponentInChildren<TMP_Text>();
            
            if(text != null)    
            text.text = $"{model.name}\n{model.creationDate}";
            
            Button btn = btnGO.GetComponent<Button>();
            
            if(btn != null)
            btn.interactable = false;

            LongPressDetector detector = btnGO.GetComponent<LongPressDetector>();
            
            if(detector == null)
            detector = btnGO.AddComponent<LongPressDetector>();

            detector.onShortPress.RemoveAllListeners();
            detector.onLongPress.RemoveAllListeners();

            GalleryModelData currentModel = model;
            detector.onShortPress.AddListener(() => OnModelSelected(currentModel));
            detector.onLongPress.AddListener(() => OnLongPressDelete(currentModel));

            Image thumbImage = btnGO.GetComponent<Image>();
            
            if(thumbImage != null)
            {
                if(!string.IsNullOrEmpty(model.thumbnailPath) && File.Exists(model.thumbnailPath))
                {
                    byte[] bytes = File.ReadAllBytes(model.thumbnailPath);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.LoadImage(bytes);
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    thumbImage.sprite = sprite;
                    thumbImage.color = Color.white;
                    thumbImage.preserveAspect = true;
                }
                
                else
                {
                    Sprite defaultThumb = Resources.Load<Sprite>("Materials/Download");
                    
                    if(defaultThumb != null)
                    thumbImage.sprite = defaultThumb;
                }
            }
        }
    }

    private void OnLongPressDelete(GalleryModelData model)
    {
        pendingDeleteModel = model;
        
        if(confirmDeletePanel != null)   
        {
            confirmDeletePanel.SetActive(true);
            galleryPanel.SetActive(false);
        }
        
        else
        DeleteModelAndRefresh(model);
    }

    private void OnConfirmDelete()
    {
        if(pendingDeleteModel != null)
        {
            DeleteModelAndRefresh(pendingDeleteModel);
            pendingDeleteModel = null;
        }
     
        HideDeleteDialog();
    }

    private void HideDeleteDialog()
    {
        if(confirmDeletePanel != null)
        confirmDeletePanel.SetActive(false);

        galleryPanel.SetActive(true);
        
        pendingDeleteModel = null;
    }

    private void OnModelSelected(GalleryModelData model)
    {
        Debug.Log($"Выбрана модель: {model.name}");

        selectedModel = model;
        isPreviewMode = true;

        if(cameraMovement == null)
        {
            cameraMovement = FindFirstObjectByType<CameraMovement>();
            
            if(cameraMovement != null)
            cameraMovement.OnMoveComplete += OnMoveComplete;
        }
    
        if(cameraMovement != null)
        cameraMovement.MoveToPoint(PREVIEW_POINT_INDEX);
        
        else
        Debug.LogError("CameraMovement not assigned!");
    }

    private void DeleteModelAndRefresh(GalleryModelData model)
    {
        Debug.Log($"Удаление модели: {model.name}");
        SaveManager.Instance.DeleteGalleryModel(model.id);
        LoadAndDisplayModels();
    }

    private void OnMoveComplete(int pointIndex)
    {
        if(pointIndex == PREVIEW_POINT_INDEX && isPreviewMode)
        {
            SpawnPreviewModel(selectedModel);

            if(previewUICanvas != null) 
            {
                previewUICanvas.SetActive(true);
                galleryPanel.SetActive(false);
            }
        }
    }

    private void SpawnPreviewModel(GalleryModelData model)
    {
        SaveManager.IsSpawningBlocks = true;

        if(model.type == "FreeMode" && model.blocks != null && model.blocks.Count > 0)
        {
            SaveManager.Instance.SpawnFromSaveData(model.blocks, modelPreview.previewParent);
            DisableCollidersRecursive(modelPreview.previewParent);
            DisableBlockComponents(modelPreview.previewParent.gameObject);
            
            BoxCollider bc = modelPreview.previewParent.gameObject.AddComponent<BoxCollider>();
            bc.size = new Vector3(2, 2, 2);
            modelPreview.previewParent.gameObject.AddComponent<RotateModelOnDrag>();
            modelPreview.previewParent.gameObject.GetComponent<RotateModelOnDrag>().lastMousePos = new Vector3(0.0f, 0.0f, 0.0f);

            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;

            foreach(Transform child in modelPreview.previewParent)
            {
                Vector3 pos = child.localPosition;
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }
            
            Vector3 center = (min + max) * 0.5f;
            
            foreach(Transform child in modelPreview.previewParent)        
            child.localPosition -= center;

            modelPreview.previewParent.position = previewStage.position;
            modelPreview.previewParent.rotation = previewStage.rotation;
        }
        
        else if(model.type == "Career")
        {
            GameObject prefab = GetCareerPrefab(model.levelId);
            
            if(prefab != null)
            {
                currentPreviewInstance = Instantiate(prefab, previewStage.position, previewStage.rotation);
                DisableBlockComponents(currentPreviewInstance);
                currentPreviewInstance.transform.SetParent(modelPreview.previewParent);
                modelPreview.previewParent.gameObject.AddComponent<RotateModelOnDrag>();
                modelPreview.previewParent.gameObject.GetComponent<RotateModelOnDrag>().lastMousePos = new Vector3(0.0f, 0.0f, 0.0f);
            
                BoxCollider bc = modelPreview.previewParent.gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(2, 2, 2);
            }
            
            else
            {
                Debug.LogWarning($"Префаб для уровня {model.levelId} не найден!");
            }
        }
        
        else
        {
            Debug.Log("Не удалось спавнить модель: нет данных блоков или неизвестный тип.");
        }

        SaveManager.IsSpawningBlocks = false;
    }

    private GameObject GetCareerPrefab(string levelId)
    {
        if(careerDatabase != null)
        return careerDatabase.GetPrefab(levelId);
        
        return null;
    }

    private void ExitPreview()
    {
        foreach(Transform child in modelPreview.previewParent)
        Destroy(child.gameObject);
        
        modelPreview.previewParent.localRotation = Quaternion.identity;
        modelPreview.previewParent.localScale = Vector3.one;

        BoxCollider bc = modelPreview.previewParent.GetComponent<BoxCollider>();
        
        if(bc != null)
        Destroy(bc);
        
        RotateModelOnDrag rot = modelPreview.previewParent.GetComponent<RotateModelOnDrag>();
        
        if(rot != null)
        Destroy(rot);

        if(previewUICanvas != null)
        previewUICanvas.SetActive(false);

        if(cameraMovement != null)
        cameraMovement.MoveToPoint(CABINET_POINT_INDEX);

        galleryPanel.SetActive(true);
        isPreviewMode = false;
    }

    private void DisableBlockComponents(GameObject root)
    {
        if(root == null)
        return;
        
        var blocks = root.GetComponentsInChildren<Block>(true);
        var points = root.GetComponentsInChildren<BlockPoint>(true);
        
        foreach(var b in blocks)
        b.enabled = false;
        
        foreach(var p in points)
        p.enabled = false;
    }

    private void DisableCollidersRecursive(Transform parent)
    {
        foreach(Transform child in parent)
        {
            Collider col = child.GetComponent<Collider>();
            
            if(col != null)
            col.enabled = false;
            
            DisableCollidersRecursive(child);
        }
    }
}