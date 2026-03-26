using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class AdvancedColorPicker : MonoBehaviour
{
    public RawImage colorPreview;

    public Button rgbModeButton;    // кнопка переключения на RGB режим
    public Button hsvModeButton;    // кнопка переключения на HSV режим
    public Button hexModeButton;    // кнопка переключения на Hex режим

    public Image rgbButtonImage;      // Image компонент кнопки RGB
    public Image hsvButtonImage;      // Image компонент кнопки HSV
    public Image hexButtonImage;      // Image компонент кнопки HEX
    public Color activeColor = new Color(1.0f, 0.5193217f, 1.0f);         // активный цвет
    public Color inactiveColor = Color.white;               // неактивный цвет

    public GameObject rgbPanel;     // панель с RGB слайдерами и полями
    public GameObject hsvPanel;     // панель с HSV слайдерами и полями
    public GameObject hexPanel;     // панель с Hex полем ввода

    private enum ColorEditMode { RGB, HSV, Hex }
    private ColorEditMode currentMode = ColorEditMode.RGB;

    public RawImage colorWheelImage;       // RawImage для отображения цветового круга
    public RectTransform colorWheelRect;   // RectTransform круга (для пересчёта координат)
    public GameObject wheelCursor;         // Маркер позиции на круге
    private Color originalColor;

    public Slider valueSlider;             // Вертикальный слайдер яркости (0..1)
    public RawImage valueGradientImage;    // Изображение градиента яркости

    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public TMP_InputField inputR;
    public TMP_InputField inputG;
    public TMP_InputField inputB;

    public Slider sliderH;
    public Slider sliderS;
    public Slider sliderV;
    public TMP_InputField inputH;
    public TMP_InputField inputS;
    public TMP_InputField inputV;
    public Slider sliderA;
    public TMP_InputField inputA;

    public TMP_InputField hexInput;

    public Transform presetsContainer;          // Контейнер для кнопок пресетов
    public GameObject presetButtonPrefab;       // Префаб кнопки пресета
    public Button addPresetButton;              // Кнопка "Добавить в пресеты"
    public int maxPresets = 10;
    public Button applyButton;
    public Button resetButton;
    public Button cancelButton;

    // Событие, вызываемое при применении цвета
    public Action<Color> onColorApplied;

    // Внутреннее состояние
    private Color currentColor = Color.white;
    private float currentHue, currentSat, currentVal;
    private List<Color> presets = new List<Color>();
    private Texture2D colorWheelTexture;
    private Texture2D valueGradientTexture;
    private int wheelSize = 512;          // размер текстуры круга
    private int gradientHeight = 256;      // высота текстуры полосы яркости
    private bool isUpdating = false;

    void Start()
    {
        if (rgbModeButton != null)
        rgbModeButton.onClick.AddListener(() => SetMode(ColorEditMode.RGB));
        
        if (hsvModeButton != null)
        hsvModeButton.onClick.AddListener(() => SetMode(ColorEditMode.HSV));
        
        if (hexModeButton != null)
        hexModeButton.onClick.AddListener(() => SetMode(ColorEditMode.Hex));

        // Устанавливаем начальный режим
        SetMode(currentMode);

        // Подписка на события слайдеров и полей
        sliderR.onValueChanged.AddListener(OnRGBSliderChanged);
        sliderG.onValueChanged.AddListener(OnRGBSliderChanged);
        sliderB.onValueChanged.AddListener(OnRGBSliderChanged);

        sliderH.onValueChanged.AddListener(OnHSVRGBChanged);
        sliderS.onValueChanged.AddListener(OnHSVRGBChanged);
        sliderV.onValueChanged.AddListener(OnHSVRGBChanged);

        sliderA.onValueChanged.AddListener(OnAlphaChanged);

        inputR.onEndEdit.AddListener(OnRGBInputEndEdit);
        inputG.onEndEdit.AddListener(OnRGBInputEndEdit);
        inputB.onEndEdit.AddListener(OnRGBInputEndEdit);
        inputH.onEndEdit.AddListener(OnHSVInputEndEdit);
        inputS.onEndEdit.AddListener(OnHSVInputEndEdit);
        inputV.onEndEdit.AddListener(OnHSVInputEndEdit);
        inputA.onEndEdit.AddListener(OnAlphaInputEndEdit);
        hexInput.onEndEdit.AddListener(OnHexInputEndEdit);

        applyButton.onClick.AddListener(ApplyColor);
        resetButton.onClick.AddListener(ResetToOriginal);
        cancelButton.onClick.AddListener(() => gameObject.SetActive(false));

        if(addPresetButton != null)
        addPresetButton.onClick.AddListener(AddCurrentColorToPresets);

        // Создание текстуры и UI
        GenerateColorWheelTexture();
        GenerateValueGradientTexture();

        if(valueSlider != null)
        valueSlider.onValueChanged.AddListener(OnValueSliderChanged);

        if(valueGradientImage != null)
        {
            int height = 256;
            Texture2D gradientTex = new Texture2D(1, height, TextureFormat.RGBA32, false);
            
            for(int i = 0; i < height; i++)
            {
                // i = 0 (низ), i = height-1 (верх)
                float t = (float)i / (height - 1); // теперь 0 внизу, 1 вверху
                Color c = new Color(t, t, t, 1);   // чёрный снизу, белый сверху
                gradientTex.SetPixel(0, i, c);
            }
            
            gradientTex.Apply();
            valueGradientImage.texture = gradientTex;
        }

        // Настройка взаимодействия с кругом
        ColorWheelHandler wheelHandler = colorWheelRect.gameObject.AddComponent<ColorWheelHandler>();
        wheelHandler.OnColorSelected += OnWheelSelected;

        // Загружаем пресеты
        LoadPresets();
    }

    private void SetMode(ColorEditMode mode)
    {
        currentMode = mode;

        if(rgbPanel != null)
        {
            rgbPanel.SetActive(mode == ColorEditMode.RGB);
            rgbButtonImage.color = (mode == ColorEditMode.RGB) ? activeColor : inactiveColor;
        }
        
        if(hsvPanel != null)
        {
            hsvPanel.SetActive(mode == ColorEditMode.HSV);
            hsvButtonImage.color = (mode == ColorEditMode.HSV) ? activeColor : inactiveColor;
        }
        
        if(hexPanel != null)
        {
            hexPanel.SetActive(mode == ColorEditMode.Hex);
            hexButtonImage.color = (mode == ColorEditMode.Hex) ? activeColor : inactiveColor;
        }
    }

    void OnDestroy()
    {
        // Освобождаем текстуры
        if(colorWheelTexture)
        Destroy(colorWheelTexture);
        
        if(valueGradientTexture)
        Destroy(valueGradientTexture);
    }

    // Метод для показа панели с начальным цветом
    public void Show(Color initialColor)
    {
        // Всегда устанавливаем белый цвет при открытии
        originalColor = Color.white;
        currentColor = Color.white;
        Color.RGBToHSV(Color.white, out float h, out float s, out float v);
        currentHue = h * 360;
        currentSat = s;
        currentVal = v;
        UpdateAllUI();
        gameObject.SetActive(true);
    }

    // Обновление всех UI-элементов в соответствии с текущим цветом
    private void UpdateAllUI()
    {
        if(isUpdating)
        return;

        isUpdating = true;

        sliderR.value = currentColor.r;
        sliderG.value = currentColor.g;
        sliderB.value = currentColor.b;
        inputR.text = (currentColor.r * 255).ToString("F0");
        inputG.text = (currentColor.g * 255).ToString("F0");
        inputB.text = (currentColor.b * 255).ToString("F0");

        sliderH.value = currentHue / 360f;
        sliderS.value = currentSat;
        sliderV.value = currentVal;

        if(valueSlider != null)
        valueSlider.value = currentVal;

        inputH.text = currentHue.ToString("F0");
        inputS.text = currentSat.ToString("F2");
        inputV.text = currentVal.ToString("F2");

        sliderA.value = currentColor.a;
        inputA.text = (currentColor.a * 255).ToString("F0");

        hexInput.text = ColorUtility.ToHtmlStringRGB(currentColor);

        // Позиция маркера на круге
        UpdateWheelCursor();

        if (colorPreview != null)
        colorPreview.color = currentColor;

        isUpdating = false;
    }

    // Обновление цвета из круга (hue, saturation)
    private void OnWheelSelected(float hue, float saturation)
    {
        currentHue = hue * 360;
        currentSat = saturation;
        Color newColor = Color.HSVToRGB(currentHue / 360, currentSat, currentVal);
        newColor.a = currentColor.a;
        SetColor(newColor);
    }

    // Изменение цвета через RGB
    private void OnRGBSliderChanged(float _)
    {
        if(isUpdating)
        return;

        Color newColor = new Color(sliderR.value, sliderG.value, sliderB.value, sliderA.value);
        SetColor(newColor);
    }

    private void OnRGBInputEndEdit(string _)
    {
        if(isUpdating)
        return;

        float r = ParseFloatInput(inputR.text, 0, 255) / 255f;
        float g = ParseFloatInput(inputG.text, 0, 255) / 255f;
        float b = ParseFloatInput(inputB.text, 0, 255) / 255f;
        Color newColor = new Color(r, g, b, currentColor.a);
        SetColor(newColor);
    }

    // Изменение цвета через HSV
    private void OnHSVRGBChanged(float _)
    {
        if(isUpdating)
        return;

        float h = sliderH.value * 360;
        float s = sliderS.value;
        float v = sliderV.value;
        Color newColor = Color.HSVToRGB(h / 360, s, v);
        newColor.a = currentColor.a;
        SetColor(newColor);
    }

    private void OnHSVInputEndEdit(string _)
    {
        if(isUpdating)
        return;

        float h = ParseFloatInput(inputH.text, 0, 360);
        float s = ParseFloatInput(inputS.text, 0, 1);
        float v = ParseFloatInput(inputV.text, 0, 1);
        Color newColor = Color.HSVToRGB(h / 360, s, v);
        newColor.a = currentColor.a;
        SetColor(newColor);
    }

    private void OnAlphaChanged(float _)
    {
        if(isUpdating)
        return;

        Color newColor = currentColor;
        newColor.a = sliderA.value;
        SetColor(newColor);
    }

    private void OnAlphaInputEndEdit(string _)
    {
        if(isUpdating)
        return;

        float a = ParseFloatInput(inputA.text, 0, 255) / 255f;
        Color newColor = currentColor;
        newColor.a = a;
        SetColor(newColor);
    }

    private void OnHexInputEndEdit(string hex)
    {
        if (isUpdating) return;

        // Убираем # если он есть
        if (hex.StartsWith("#"))
        hex = hex.Substring(1);

        if (ColorUtility.TryParseHtmlString("#" + hex, out Color col))
        {
            col.a = currentColor.a; // сохраняем текущую прозрачность
            SetColor(col);
        }
        
        // Если введён некорректный hex, восстанавливаем предыдущий
        else
        hexInput.text = ColorUtility.ToHtmlStringRGB(currentColor);
    }

    private void OnValueSliderChanged(float val)
    {
        if(isUpdating)
        return;
        
        currentVal = val;
        // Обновляем слайдер V в HSV-панели
        sliderV.value = currentVal;
        // Пересчитываем цвет на основе текущих hue, saturation и новой яркости
        Color newColor = Color.HSVToRGB(currentHue / 360, currentSat, currentVal);
        newColor.a = currentColor.a;
        SetColor(newColor);
    }

    private void SetColor(Color newColor)
    {
        currentColor = newColor;
        Color.RGBToHSV(newColor, out float h, out float s, out float v);
        currentHue = h * 360;
        currentSat = s;
        currentVal = v;
        UpdateAllUI();
    }

    private void ApplyColor()
    {
        onColorApplied?.Invoke(currentColor);
        gameObject.SetActive(false);
    }

    private void ResetToOriginal()
    {
        SetColor(originalColor);
    }

    // Текстуры
    private void GenerateColorWheelTexture()
    {
        if(colorWheelTexture != null)
        Destroy(colorWheelTexture);
        
        colorWheelTexture = new Texture2D(wheelSize, wheelSize, TextureFormat.RGBA32, false);
        float center = wheelSize / 2f;
        float radius = center;

        Color[] colors = new Color[wheelSize * wheelSize];
        
        for(int y = 0; y < wheelSize; y++)
        {
            for(int x = 0; x < wheelSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if(distance > radius)
                {
                    colors[y * wheelSize + x] = Color.clear;
                    continue;
                }
                
                float saturation = distance / radius; // 0..1
                float hue = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                hue = (hue + 360) % 360;
                Color c = Color.HSVToRGB(hue / 360, saturation, 1f);
                colors[y * wheelSize + x] = c;
            }
        }

        colorWheelTexture.SetPixels(colors);
        colorWheelTexture.Apply();
        colorWheelImage.texture = colorWheelTexture;
    }

    private void GenerateValueGradientTexture()
    {
        if(valueGradientTexture != null)
        Destroy(valueGradientTexture);
        
        valueGradientTexture = new Texture2D(1, gradientHeight, TextureFormat.RGBA32, false);
        Color baseColor = Color.HSVToRGB(currentHue / 360, currentSat, 1f);
        
        for(int y = 0; y < gradientHeight; y++)
        {
            float value = (float)y / (gradientHeight - 1);
            Color c = baseColor * value;
            c.a = 1;
            valueGradientTexture.SetPixel(0, y, c);
        }
        
        valueGradientTexture.Apply();
        
        if(valueGradientImage != null)
        valueGradientImage.texture = valueGradientTexture;
    }

    //Маркер на круге
    private void UpdateWheelCursor()
    {
        if(wheelCursor == null)
        return;
        
        // Позиция на круге: угол = currentHue, радиус = currentSat
        float angleRad = currentHue * Mathf.Deg2Rad;
        float radius = currentSat * (colorWheelRect.rect.width / 2);
        Vector2 pos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
        wheelCursor.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    //Пресеты
    private void LoadPresets()
    {
        presets.Clear();
        string json = PlayerPrefs.GetString("ColorPresets", "");
        
        if(!string.IsNullOrEmpty(json))
        {
            var wrapper = JsonUtility.FromJson<ColorListWrapper>(json);
            
            if(wrapper != null && wrapper.colors != null)
            presets = wrapper.colors;
        }

        if(presets.Count == 0)
        {
            presets.Add(Color.red);
            presets.Add(Color.green);
            presets.Add(Color.blue);
            presets.Add(Color.yellow);
            presets.Add(Color.cyan);
            presets.Add(Color.magenta);
        }
        
        RefreshPresetButtons();
    }

    private void SavePresets()
    {
        var wrapper = new ColorListWrapper { colors = presets };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("ColorPresets", json);
        PlayerPrefs.Save();
    }

    private void RefreshPresetButtons()
    {
        foreach(Transform child in presetsContainer)
        Destroy(child.gameObject);

        foreach(Color col in presets)
        {
            GameObject btnGO = Instantiate(presetButtonPrefab, presetsContainer);
            Image img = btnGO.GetComponent<Image>();
            
            if(img != null)
            img.color = col;

            // Удаляем стандартный Button, если есть (чтобы не мешал)
            Button stdBtn = btnGO.GetComponent<Button>();
            
            if(stdBtn != null)
            stdBtn.enabled = false;

            PresetButton presetBtn = btnGO.AddComponent<PresetButton>();
            presetBtn.Initialize(col,
                (c) => SetColor(c),
                (c) => RemovePreset(c));
        }
    }

    private void AddCurrentColorToPresets()
    {
        if(presets.Count >= maxPresets && !presets.Contains(currentColor))
        presets.RemoveAt(presets.Count - 1);
        
        if(!presets.Contains(currentColor))
        presets.Insert(0, currentColor);
        
        else
        {
            presets.Remove(currentColor);
            presets.Insert(0, currentColor);
        }
        
        SavePresets();
        RefreshPresetButtons();
    }

    private void RemovePreset(Color color)
    {
        presets.RemoveAll(c => Approximately(c, color));
        SavePresets();
        RefreshPresetButtons();
    }

    private bool Approximately(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    //Вспомогательное
    private float ParseFloatInput(string text, float min, float max)
    {
        if(float.TryParse(text, out float val))
        return Mathf.Clamp(val, min, max);
        
        return min;
    }

    [Serializable]
    private class ColorListWrapper
    {
        public List<Color> colors;
    }
}

//Обработчик кликов на цветовом круге
public class ColorWheelHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public System.Action<float, float> OnColorSelected; // hue (0..1), saturation (0..1)
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateColorFromPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColorFromPointer(eventData);
    }

    private void UpdateColorFromPointer(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPos);
        
        // localPos от -width/2 до width/2
        float x = (localPos.x / rectTransform.rect.width) + 0.5f;
        float y = (localPos.y / rectTransform.rect.height) + 0.5f;
        float dx = x - 0.5f;
        float dy = y - 0.5f;
        float radius = Mathf.Sqrt(dx * dx + dy * dy);
        
        if(radius > 0.5f)
        return;

        float saturation = radius * 2;
        float hue = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        hue = (hue + 360) % 360;
        OnColorSelected?.Invoke(hue / 360f, saturation);
    }
}