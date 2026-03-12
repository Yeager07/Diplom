using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ColorPicker : MonoBehaviour
{
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public TMP_InputField inputR;
    public TMP_InputField inputG;
    public TMP_InputField inputB;
    public Image previewColor;

    public Transform presetsContainer;          // Родительский объект для кнопок-пресетов
    public GameObject presetButtonPrefab;       // Префаб кнопки пресета
    public Button addPresetButton;               // Кнопка "Добавить в пресеты"
    public int maxPresets = 20;                  // Максимальное количество сохраняемых пресетов


    public Button applyButton;
    public Button resetButton;
    public Button cancelButton;
    public System.Action<Color> onColorApplied;
    private Color originalColor;
    private List<Color> presets = new List<Color>();

    void Start()
    {
        // Подписка на события слайдеров
        sliderR.onValueChanged.AddListener(OnSliderChanged);
        sliderG.onValueChanged.AddListener(OnSliderChanged);
        sliderB.onValueChanged.AddListener(OnSliderChanged);

        // Подписка на события полей ввода
        inputR.onEndEdit.AddListener(OnInputEndEdit);
        inputG.onEndEdit.AddListener(OnInputEndEdit);
        inputB.onEndEdit.AddListener(OnInputEndEdit);

        // Кнопки
        applyButton.onClick.AddListener(ApplyColor);
        resetButton.onClick.AddListener(ResetToOriginal);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(() => gameObject.SetActive(false));

        if (addPresetButton != null)
            addPresetButton.onClick.AddListener(AddCurrentColorToPresets);

        // Загружаем сохранённые пресеты
        LoadPresets();
    }

    public void Show(Color currentColor)
    {
        originalColor = currentColor;
        SetColor(currentColor);
        gameObject.SetActive(true);
    }

    private void SetColor(Color color)
    {
        // Обновляем слайдеры
        sliderR.value = color.r;
        sliderG.value = color.g;
        sliderB.value = color.b;

        // Обновляем поля ввода (формат с двумя знаками после запятой)
        inputR.text = color.r.ToString("F2");
        inputG.text = color.g.ToString("F2");
        inputB.text = color.b.ToString("F2");

        // Обновляем превью
        previewColor.color = color;
    }

    private void OnSliderChanged(float value)
    {
        // Получаем текущие значения слайдеров и обновляем UI
        Color newColor = new Color(sliderR.value, sliderG.value, sliderB.value);
        // Обновляем поля ввода и превью без повторного вызова событий
        inputR.text = sliderR.value.ToString("F2");
        inputG.text = sliderG.value.ToString("F2");
        inputB.text = sliderB.value.ToString("F2");
        previewColor.color = newColor;
    }

    private void OnInputEndEdit(string text)
    {
        // Парсим введенное значение как float
        float val;
        if (float.TryParse(text, out val))
        {
            val = Mathf.Clamp01(val); // ограничиваем диапазоном 0-1

            // Определяем, какое поле было изменено
            if (inputR.isFocused)
            sliderR.value = val;
            
            else if (inputG.isFocused)
            sliderG.value = val;
        
            else if (inputB.isFocused)
            sliderB.value = val;
        }
        else
        // Если введено не число, восстанавливаем предыдущее значение из слайдера
        UpdateInputsFromSliders();
    }

    private void UpdateInputsFromSliders()
    {
        inputR.text = sliderR.value.ToString("F2");
        inputG.text = sliderG.value.ToString("F2");
        inputB.text = sliderB.value.ToString("F2");
    }

    private void ApplyColor()
    {
        Color selectedColor = new Color(sliderR.value, sliderG.value, sliderB.value);
        onColorApplied?.Invoke(selectedColor);
        gameObject.SetActive(false);
    }

    private void ResetToOriginal()
    {
        SetColor(originalColor);
    }

    private void LoadPresets()
    {
        presets.Clear();
        // Загружаем из PlayerPrefs (например, как список строк "R,G,B")
        string presetsJson = PlayerPrefs.GetString("ColorPresets", "");
        if (!string.IsNullOrEmpty(presetsJson))
        {
            try
            {
                var wrapper = JsonUtility.FromJson<ColorListWrapper>(presetsJson);
                if (wrapper != null && wrapper.colors != null)
                presets = wrapper.colors;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Не удалось загрузить пресеты цветов: " + e.Message);
            }
        }

        // Если нет сохранённых, можно добавить несколько стандартных
        if (presets.Count == 0)
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
        string presetsJson = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("ColorPresets", presetsJson);
        PlayerPrefs.Save();
    }

    private void RefreshPresetButtons()
    {
        // Удаляем старые кнопки
        foreach (Transform child in presetsContainer)
        {
            Destroy(child.gameObject);
        }

        // Создаём новые кнопки для каждого пресета
        foreach (Color col in presets)
        {
            GameObject btnGO = Instantiate(presetButtonPrefab, presetsContainer);
        
        // Настраиваем цвет кнопки
            Image btnImage = btnGO.GetComponent<Image>();
            if (btnImage != null)
            btnImage.color = col;

            btnGO.GetComponent<Image>().color = col;

        // Добавляем компонент PresetButton
            PresetButton presetBtn = btnGO.AddComponent<PresetButton>();
            presetBtn.Initialize(
                col,
                (color) => SetColor(color),    // обычный клик
                (color) => RemovePreset(color) // долгое нажатие
            );
        }
    }

    private void RemovePreset(Color color)
    {
        presets.RemoveAll(c => Approximately(c, color));
        SavePresets();
        RefreshPresetButtons();
    }

    private void AddCurrentColorToPresets()
    {
        Color currentColor = new Color(sliderR.value, sliderG.value, sliderB.value);

        // Если такой цвет уже есть, можно ничего не делать или переместить в начало
        if (!presets.Any(c => Approximately(c, currentColor)))
        {
            // Добавляем в начало списка
            presets.Insert(0, currentColor);

            // Ограничиваем максимальное количество
            if (presets.Count > maxPresets)
            presets.RemoveAt(presets.Count - 1);
        }
        else
        {
            // Если уже есть, можно переместить его в начало (как "недавний")
            presets.RemoveAll(c => Approximately(c, currentColor));
            presets.Insert(0, currentColor);
        }

        SavePresets();
        RefreshPresetButtons();
    }

    // Вспомогательная функция для сравнения цветов с допуском
    private bool Approximately(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    // Вспомогательный класс для сериализации списка цветов
    [System.Serializable]
    private class ColorListWrapper
    {
        public List<Color> colors;
    }
}