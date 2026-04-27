using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;           // панель с кнопками меню паузы
    public GameObject controlsImage;        // изображение с управлением (инструкция)

    public Button resumeButton;
    public Button mainMenuButton;
    public Button controlsButton;            // кнопка открытия инструкции
    //public Button closeControlsButton;      // кнопка закрытия инструкции (если есть)

    private bool isPaused = false;
    private bool isControlsOpen = false;

    private Player player;

    void Start()
    {
        player = FindFirstObjectByType<Player>();
        
        // Изначально меню паузы скрыто, панель инструкции скрыта
        if(pausePanel)
        pausePanel.SetActive(false);
        
        if(controlsImage)
        controlsImage.SetActive(false);

        // Подписываем кнопки
        if(resumeButton)
        resumeButton.onClick.AddListener(ResumeGame);
        
        if(mainMenuButton)
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        if(controlsButton)
        controlsButton.onClick.AddListener(ToggleControls);
        
        /*if(closeControlsButton)
        closeControlsButton.onClick.AddListener(CloseControls);*/
    }

    void Update()
    {
        // Проверка нажатия Escape
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
        Time.timeScale = 0.0f;                // останавливаем время
        
        if(pausePanel)
        pausePanel.SetActive(true);
        
        // Блокируем курсор для UI (если нужно)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        // Если открыта инструкция – закрываем её
        if(isControlsOpen)
        CloseControls();

        if(pausePanel)
        pausePanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1.0f;
        
        // Возвращаем курсор в состояние для игры (заблокирован, невидим)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void GoToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
//        Player player = FindFirstObjectByType<Player>();

        InventoryManager inventoryManager = GameObject.FindFirstObjectByType<InventoryManager>();
        
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