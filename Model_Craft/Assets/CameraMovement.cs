using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public Transform[] cameraPoints;
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 1.0f;

    private int currentTargetIndex = 0;
    private bool isMoving = false;
    private bool isInitialized = false;

    void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        InitializeCameraPoints();
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // При загрузке любой сцены пробуем переинициализировать точки
        InitializeCameraPoints();
        // Если текущая сцена – главное меню, то сбрасываем позицию в центр
        Player player = FindFirstObjectByType<Player>();
        if (player != null && player.typeGame == "MainMenu" && cameraPoints != null && cameraPoints.Length > 0)
        {
            transform.position = cameraPoints[0].position;
            transform.rotation = cameraPoints[0].rotation;
            currentTargetIndex = 0;
        }
    }

    private void InitializeCameraPoints()
    {
        // Если массив уже заполнен валидными ссылками, не трогаем
        if (cameraPoints != null && cameraPoints.Length == 5)
        {
            bool allValid = true;
            foreach (var p in cameraPoints)
                if (p == null) { allValid = false; break; }
            if (allValid) return;
        }

        // Иначе ищем точки по тегам
        cameraPoints = new Transform[5];
        cameraPoints[0] = GameObject.FindGameObjectWithTag("Center")?.transform;
        cameraPoints[1] = GameObject.FindGameObjectWithTag("Table")?.transform;
        cameraPoints[2] = GameObject.FindGameObjectWithTag("Cabinet")?.transform;
        cameraPoints[3] = GameObject.FindGameObjectWithTag("Settings")?.transform;
        cameraPoints[4] = GameObject.FindGameObjectWithTag("Exit")?.transform;

        // Проверка
        for (int i = 0; i < cameraPoints.Length; i++)
        {
            if (cameraPoints[i] == null)
                Debug.LogWarning($"CameraMovement: point {i} not found! Assign tag correctly.");
        }
    }

    public void MoveToPoint(int index)
    {
        Player player = FindFirstObjectByType<Player>();
        
        if(player.typeGame != "MainMenu")
        return;
        
        if(!isInitialized)
        InitializeCameraPoints();
        
        if(index < 0 || index >= cameraPoints.Length || cameraPoints[index] == null)
        {
            Debug.LogError($"MoveToPoint: invalid index {index}");
            return;
        }
        
        if(isMoving)
        StopAllCoroutines();
        
        StartCoroutine(SmoothMove(index));
    }

    IEnumerator SmoothMove(int targetIndex)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = cameraPoints[targetIndex].position;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = cameraPoints[targetIndex].rotation;

        float t = 0f;
        
        while(t < 1f)
        {
            // Если объект уничтожен – выходим из корутины
            if(this == null || gameObject == null)
            yield break;

            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t * rotationSpeed);
            
            yield return null;
        }

        // Финальная проверка перед присваиванием
        if(this != null && gameObject != null)
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
        }

        isMoving = false;
        currentTargetIndex = targetIndex;

        if(ZoneManager.Instance != null)
        ZoneManager.Instance.OnCameraArrived(targetIndex);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && currentTargetIndex != 0)
        {
            if(ZoneManager.Instance != null)
            ZoneManager.Instance.HideAllPanels();
            
            MoveToPoint(0);
        }
    }
}