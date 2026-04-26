using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    [Header("Zone Panels")]
    public GameObject tableUIPanel;
    public GameObject cabinetUIPanel;
    public GameObject settingsUIPanel;
    public GameObject exitUIPanel;

    [Header("Table Zone")]
    public GameObject[] levelPreviews;          // массив префабов (кубы с текстом)
    public Transform previewSpawnPoint;
    public Button nextLevelButton;
    public Button prevLevelButton;
    public Button careerModeButton;
    public Button freeModeButton;

    [Header("Settings Zone")]
    public Slider volumeSlider;
    public Button languageButton;               // кнопка переключения языка
    public TMP_Text languageButtonText;         // текст на кнопке

    [Header("Exit Zone")]
    public Button confirmExitButton;
    public Button cancelExitButton;

    private int currentLevelIndex = 0;
    private GameObject currentPreviewInstance;
    private CameraMovement camMovement;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        camMovement = Camera.main.GetComponent<CameraMovement>();
        HideAllPanels();

        // Подписки
        if (nextLevelButton) nextLevelButton.onClick.AddListener(NextLevel);
        if (prevLevelButton) prevLevelButton.onClick.AddListener(PreviousLevel);
        if (careerModeButton) careerModeButton.onClick.AddListener(StartCareerMode);
        if (freeModeButton) freeModeButton.onClick.AddListener(StartFreeMode);
        if (volumeSlider) volumeSlider.onValueChanged.AddListener(ChangeVolume);
        if (languageButton) languageButton.onClick.AddListener(ChangeLanguage);
        if (confirmExitButton) confirmExitButton.onClick.AddListener(ExitGame);
        if (cancelExitButton) cancelExitButton.onClick.AddListener(() => exitUIPanel.SetActive(false));
    }

    private void HideAllPanels()
    {
        if (tableUIPanel) tableUIPanel.SetActive(false);
        if (cabinetUIPanel) cabinetUIPanel.SetActive(false);
        if (settingsUIPanel) settingsUIPanel.SetActive(false);
        if (exitUIPanel) exitUIPanel.SetActive(false);
    }

    public void OnCameraArrived(int zoneIndex)
    {
        HideAllPanels();
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
        UpdateLevelPreview();
    }

    private void ShowCabinetUI()
    {
        cabinetUIPanel.SetActive(true);
        // TODO: загрузка списка собранных моделей
    }

    private void ShowSettingsUI()
    {
        settingsUIPanel.SetActive(true);
        // Загружаем текущие настройки громкости, языка
        volumeSlider.value = AudioListener.volume;
        UpdateLanguageButtonText();
    }

    private void ShowExitUI()
    {
        exitUIPanel.SetActive(true);
    }

    // ==================== Зона стола ====================

    private void UpdateLevelPreview()
    {
        if (currentPreviewInstance != null) Destroy(currentPreviewInstance);
        if (levelPreviews.Length > currentLevelIndex && previewSpawnPoint != null)
        {
            currentPreviewInstance = Instantiate(levelPreviews[currentLevelIndex], previewSpawnPoint.position, previewSpawnPoint.rotation);
            // Добавляем обработчик клика на модель
            LevelPreviewClick click = currentPreviewInstance.AddComponent<LevelPreviewClick>();
            click.levelIndex = currentLevelIndex;
        }
        // Обновляем активность стрелок
        if (prevLevelButton) prevLevelButton.interactable = (currentLevelIndex > 0);
        if (nextLevelButton) nextLevelButton.interactable = (currentLevelIndex < levelPreviews.Length - 1);
    }

    private void NextLevel()
    {
        if (currentLevelIndex < levelPreviews.Length - 1)
        {
            currentLevelIndex++;
            UpdateLevelPreview();
        }
    }

    private void PreviousLevel()
    {
        if (currentLevelIndex > 0)
        {
            currentLevelIndex--;
            UpdateLevelPreview();
        }
    }

    private void StartCareerMode()
    {
        // Получаем LevelData для выбранного уровня
        LevelData levelData = GetLevelDataByIndex(currentLevelIndex);
        LevelLoader.SelectedLevel = levelData;

        // Настройка игрока и сцены
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.typeGame = "CareerMode";
        player.isBuildMode = true;
        player.transform.Find("UI").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Instruction").transform.Find("InstructionDownload").gameObject.SetActive(false);
        Camera.main.GetComponent<MainScript>().PlacePlayerZero();

        SceneManager.LoadScene("02_TestScene");
    }

    private void StartFreeMode()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.typeGame = "FreeMode";
        player.isBuildMode = true;
        player.transform.Find("UI").gameObject.SetActive(true);
        player.transform.Find("UI").transform.Find("Instruction").transform.Find("InstructionDownload").gameObject.SetActive(true);
        player.transform.Find("UI").Find("BlocksIcon").gameObject.SetActive(true);

        SceneManager.LoadScene("04_FreeMode");
    }

    private LevelData GetLevelDataByIndex(int index)
    {
        // Здесь нужно взять список LevelData из вашего MainScript или другого хранилища
        return Camera.main.GetComponent<MainScript>().levelDatas[index];
    }

    // ==================== Зона настроек ====================

    private void ChangeVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ChangeLanguage()
    {
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (player.language == "En")
            player.language = "Ru";
        else
            player.language = "En";

        UpdateLanguageButtonText();
        // Здесь можно обновить тексты всех UI элементов в зонах (например, кнопок)
    }

    private void UpdateLanguageButtonText()
    {
        if (languageButtonText == null) return;
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        languageButtonText.text = player.language == "En" ? "Русский" : "English";
    }

    public void StartCareerModeByIndex(int index)
    {
        currentLevelIndex = index;
        StartCareerMode();
    }

    // ==================== Зона выхода ====================

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}