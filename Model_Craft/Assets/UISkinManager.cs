using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.U2D;

public class UISkinManager : MonoBehaviour
{
    public static UISkinManager Instance;

    public SpriteAtlas defaultAtlas;
    public SpriteAtlas spaceAtlas;
    public SpriteAtlas castleAtlas;
    public SpriteAtlas robotsAtlas;
    
    public ThemeManager themeManager;
    private SpriteAtlas currentAtlas;

    private string currentLanguage = "En";
    
    private void Awake()
    {
        if(Instance == null)
        Instance = this;
        
        else
        Destroy(gameObject);
        
    }
    
    private void Start()
    {
        if(themeManager == null)
        themeManager = FindFirstObjectByType<ThemeManager>();
        
        if(themeManager != null)
        {
            themeManager.OnThemeChanged += ApplyTheme;
            ApplyTheme(themeManager.GetCurrentThemeIndex());
        }

        if(LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;

        Player player = FindFirstObjectByType<Player>();
        
        if(player != null)
        currentLanguage = player.language;
        
        ApplyTheme(themeManager?.GetCurrentThemeIndex() ?? 0);
    }

    private void OnLanguageChanged()
    {
        Player player = FindFirstObjectByType<Player>();
        
        if(player != null)
        currentLanguage = player.language;
    }

    public void SetLanguageAndUpdate(string newLanguage, GameObject controlsTarget)
    {
        currentLanguage = newLanguage;
        UpdateControlsImage(controlsTarget);
    }

    private SpriteAtlas GetAtlasByTheme(int themeIndex)
    {
        if(themeIndex == 1)
        return castleAtlas;
        
        if(themeIndex == 2)
        return spaceAtlas;
        
        if(themeIndex == 3)
        return robotsAtlas;
        
        return defaultAtlas;
    }

    private bool IsPartOfThemeButton(Component comp)
    {
        Transform t = comp.transform;
        
        while(t != null)
        {
            if(t.CompareTag("ThemeButton"))
            return true;
            
            t = t.parent;
        }
        
        return false;
    }

    public void UpdateControlsImage(GameObject target)
    {   
        if(currentAtlas == null || target == null)
        return;
        
        Image img = target.GetComponent<Image>();

        if(img == null)
        return;
        
        string langSuffix = (currentLanguage == "Ru") ? "Ru" : "En";
        string spriteName = "Movement" + langSuffix;
        
        Sprite newSprite = currentAtlas.GetSprite(spriteName);
        
        if(newSprite != null)
        img.sprite = newSprite;
        
        else
        Debug.LogWarning($"Sprite {spriteName} not found in atlas {currentAtlas.name}");
    }

    private void ApplyTheme(int themeIndex)
    {
        currentAtlas = GetAtlasByTheme(themeIndex);
        
        if(currentAtlas == null)
        return;

        Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);

        foreach(Image img in allImages)
        {
            if(IsPartOfThemeButton(img))
            continue;
            
            if(img.sprite == null)
            continue;
            
            string cleanName = img.sprite.name;
            
            if(cleanName.EndsWith("(Clone)"))
            cleanName = cleanName.Substring(0, cleanName.Length - 7);
            
            Sprite newSprite = currentAtlas.GetSprite(cleanName);
            
            if(newSprite != null)
            img.sprite = newSprite;
        }

        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach(Button btn in allButtons)
        {
            if(btn.CompareTag("ThemeButton"))
            continue;
            
            if(btn.transition != Selectable.Transition.SpriteSwap)
            continue;
            
            SpriteState oldState = btn.spriteState;
            SpriteState newState = new SpriteState();
            newState.highlightedSprite = GetSpriteWithSuffix(oldState.highlightedSprite);
            newState.pressedSprite = GetSpriteWithSuffix(oldState.pressedSprite);
            newState.selectedSprite = GetSpriteWithSuffix(oldState.selectedSprite);
            newState.disabledSprite = GetSpriteWithSuffix(oldState.disabledSprite);
            btn.spriteState = newState;
        }
    }

    private Sprite GetSpriteWithSuffix(Sprite originalSprite)
    {
        if(originalSprite == null)
        return null;
        
        string baseName = originalSprite.name;
        
        if(baseName.EndsWith("(Clone)"))
        baseName = baseName.Substring(0, baseName.Length - 7);
        
        Sprite s = currentAtlas.GetSprite(baseName);
        
        if(s == null)
        Debug.LogWarning($"Sprite not found in atlas {currentAtlas.name}: {baseName}");
        
        return s;
    }

    public void ApplyToGameObject(GameObject target)
    {
        if(currentAtlas == null || target == null)
        return;

        if(IsPartOfThemeButton(target.GetComponent<Component>()))
        return;

        Image[] images = target.GetComponentsInChildren<Image>(true);
        
        foreach(Image img in images)
        {
            if(img.sprite == null)
            continue;
            
            string cleanName = img.sprite.name;
            
            if(cleanName.EndsWith("(Clone)"))
            cleanName = cleanName.Substring(0, cleanName.Length - 7);
            
            Sprite newSprite = currentAtlas.GetSprite(cleanName);
            
            if(newSprite != null)
            img.sprite = newSprite;
        }

        Button[] buttons = target.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if(btn.transition != Selectable.Transition.SpriteSwap)
            continue;
            
            SpriteState oldState = btn.spriteState;
            SpriteState newState = new SpriteState();
            newState.highlightedSprite = GetSpriteWithSuffix(oldState.highlightedSprite);
            newState.pressedSprite = GetSpriteWithSuffix(oldState.pressedSprite);
            newState.selectedSprite = GetSpriteWithSuffix(oldState.selectedSprite);
            newState.disabledSprite = GetSpriteWithSuffix(oldState.disabledSprite);
            btn.spriteState = newState;
        }
    }

    void OnDestroy()
    {
        if(themeManager != null)
        themeManager.OnThemeChanged -= ApplyTheme;

        if(LocalizationManager.Instance != null)
        LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }
}