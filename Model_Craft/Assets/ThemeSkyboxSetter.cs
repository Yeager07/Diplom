using UnityEngine;

public class ThemeSkyboxSetter : MonoBehaviour
{
    public Material castleSkybox;
    public Material spaceSkybox;
    public Material robotsSkybox;
    public Material defaultSkybox;

    void Start()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.OnThemeChanged += ApplyTheme;
            ApplyTheme(ThemeManager.Instance.GetCurrentThemeIndex());
        }
    }

    private void ApplyTheme(int themeIndex)
    {
        if(themeIndex == 1 && castleSkybox != null)
        RenderSettings.skybox = castleSkybox;

        else if(themeIndex == 2 && spaceSkybox != null)
        RenderSettings.skybox = spaceSkybox;

        else if(themeIndex == 3 && robotsSkybox != null)
        RenderSettings.skybox = robotsSkybox;

        else if(themeIndex == 0 && defaultSkybox != null)
        RenderSettings.skybox = defaultSkybox;
    }

    void OnDestroy()
    {
        if(ThemeManager.Instance != null)
        ThemeManager.Instance.OnThemeChanged -= ApplyTheme;
    }
}