using UnityEngine;
using UnityEngine.UI;

public class InstructionChangeSize : MonoBehaviour
{
    public enum WindowMode { Small, Medium, Full }

    [Header("UI References")]
    public RectTransform windowRect;
    public Button toggleModeButton;

    [Header("Настройки режимов")]
    public WindowMode currentMode = WindowMode.Small;

    [System.Serializable]
    public struct WindowConfig
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
    }

    public WindowConfig smallConfig = new WindowConfig 
    { 
        anchorMin = new Vector2(0, 0.6f),   // левый верхний угол, примерно четверть
        anchorMax = new Vector2(0.25f, 1f) 
    };

    public WindowConfig mediumConfig = new WindowConfig 
    { 
        anchorMin = new Vector2(0.25f, 0.25f), // центр, половина
        anchorMax = new Vector2(0.75f, 0.75f) 
    };

    public WindowConfig fullConfig = new WindowConfig 
    { 
        anchorMin = Vector2.zero,
        anchorMax = Vector2.one
    };

    void Start()
    {
        if (toggleModeButton != null)
            toggleModeButton.onClick.AddListener(NextMode);

        ApplyConfig(GetCurrentConfig());
    }

    WindowConfig GetCurrentConfig()
    {
        switch (currentMode)
        {
            case WindowMode.Small: return smallConfig;
            case WindowMode.Medium: return mediumConfig;
            case WindowMode.Full: return fullConfig;
            default: return smallConfig;
        }
    }

    void ApplyConfig(WindowConfig config)
    {
        if (windowRect == null) return;

        windowRect.anchorMin = config.anchorMin;
        windowRect.anchorMax = config.anchorMax;
        windowRect.offsetMin = Vector2.zero;
        windowRect.offsetMax = Vector2.zero;
    }

    public void NextMode()
    {
        int next = ((int)currentMode + 1) % System.Enum.GetValues(typeof(WindowMode)).Length;
        currentMode = (WindowMode)next;
        ApplyConfig(GetCurrentConfig());
    }

    public void SetMode(WindowMode mode)
    {
        currentMode = mode;
        ApplyConfig(GetCurrentConfig());
    }
}
