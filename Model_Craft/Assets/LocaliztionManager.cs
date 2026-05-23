using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public LocalizationData localizationData;
    private Dictionary<string, string> englishDict = new Dictionary<string, string>();
    private Dictionary<string, string> russianDict = new Dictionary<string, string>();
    private Dictionary<string, string> currentDict;

    public System.Action OnLanguageChanged;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        LoadDictionaries();
    }

    private void LoadDictionaries()
    {
        englishDict.Clear();
        russianDict.Clear();
        
        foreach(var item in localizationData.texts)
        {
            englishDict[item.key] = item.english;
            russianDict[item.key] = item.russian;
        }
    }

    public void SetLanguage(string lang)
    {
        if(lang == "En")
        currentDict = englishDict;
        
        else
        currentDict = russianDict;
        
        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if(currentDict != null && currentDict.TryGetValue(key, out string value))
        return value;
        
        Debug.LogWarning($"Localization key '{key}' not found!");
        return key;
    }

    public string CurrentLanguage { get; private set; } = "En";
}