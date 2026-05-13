using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance;

    public string baseSceneName = "MainMenu_Base";
    public string[] themeSceneNames = { "Theme_Default", "Theme_Castle", "Theme_Space", "Theme_Robots" };

    private int currentThemeIndex = 0;
    private string currentThemeScene = null;
    private bool isLoading = false;

    public event Action<int> OnThemeChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
       
        Instance = this;
        
        currentThemeIndex = PlayerPrefs.GetInt("SelectedTheme", 0);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if(SceneManager.GetActiveScene().name == baseSceneName && string.IsNullOrEmpty(currentThemeScene))
        StartCoroutine(LoadThemeCoroutine(currentThemeIndex));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == baseSceneName)
        StartCoroutine(LoadThemeCoroutine(currentThemeIndex));
    }

    public void ChangeTheme(int themeIndex)
    {
        if(isLoading)
        return;

        if(themeIndex < 0 || themeIndex >= themeSceneNames.Length)
        return;
        
        currentThemeIndex = themeIndex;
        PlayerPrefs.SetInt("SelectedTheme", currentThemeIndex);
        PlayerPrefs.Save();

        if(SceneManager.GetActiveScene().name == baseSceneName)
        StartCoroutine(SwitchTheme());

        OnThemeChanged?.Invoke(themeIndex);
    }

    private IEnumerator SwitchTheme()
    {
        isLoading = true;
        
        if(!string.IsNullOrEmpty(currentThemeScene))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(currentThemeScene);
            
            if(sceneToUnload.isLoaded)
            yield return SceneManager.UnloadSceneAsync(currentThemeScene);
            
            currentThemeScene = null;
        }

        string newTheme = themeSceneNames[currentThemeIndex];
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newTheme, LoadSceneMode.Additive);
        
        while(!loadOp.isDone)
        yield return null;
        
        currentThemeScene = newTheme;
        isLoading = false;
    }

    private IEnumerator LoadThemeCoroutine(int index)
    {
        if(isLoading)
        yield break;
        
        if(index < 0 || index >= themeSceneNames.Length)
        yield break;
        
        isLoading = true;

        if(!string.IsNullOrEmpty(currentThemeScene))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(currentThemeScene);
            
            if(sceneToUnload.isLoaded)
            yield return SceneManager.UnloadSceneAsync(currentThemeScene);
            
            currentThemeScene = null;
        }

        string newTheme = themeSceneNames[index];
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newTheme, LoadSceneMode.Additive);
        
        while(!loadOp.isDone)
        yield return null;
        
        currentThemeScene = newTheme;
        isLoading = false;
    }

    public int GetCurrentThemeIndex() => currentThemeIndex;

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}