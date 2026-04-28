using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedTextComponent : MonoBehaviour
{
    public string key;

    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        
        if(textComponent == null)
        {
            Debug.LogError($"LocalizedTextComponent on {gameObject.name}: TMP_Text component missing!");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        if(LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        
        UpdateText();
    }

    void OnDisable()
    {
        if(LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    void UpdateText()
    {
        if(textComponent == null)
        return;
        
        if(LocalizationManager.Instance == null)
        {
            Debug.LogWarning("LocalizationManager not ready, text will be updated later.");
            return;
        }
        
        textComponent.text = LocalizationManager.Instance.GetText(key);
    }
}