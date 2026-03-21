using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    public AdvancedColorPicker colorPickerPrefab;   // Префаб панели ColorPicker
    private AdvancedColorPicker activePicker;
    public Transform uiRoot;                 // Родитель для инстанса (например, Canvas)
    public Shader standardShader;            // Шейдер для создания материала (например, Standard)
    public string colorProperty = "_Color";  // Имя свойства цвета в шейдере
    public string prefRKey = "LastColorR";
    public string prefGKey = "LastColorG";
    public string prefBKey = "LastColorB";
    private Material currentMaterial;        // Материал, используемый для спавна
    private Color defaultColor = Color.white; // Цвет по умолчанию (будет заменён на материал блока)

    void Start()
    {
        // Загружаем последний сохранённый цвет
        float r = PlayerPrefs.GetFloat(prefRKey, 1f);
        float g = PlayerPrefs.GetFloat(prefGKey, 1f);
        float b = PlayerPrefs.GetFloat(prefBKey, 1f);
        defaultColor = new Color(r, g, b);

        // Создаём начальный материал
        UpdateMaterial(defaultColor);
    }

    // Вызывается при клике на карточку блока
    public void ShowColorPicker(System.Action<Material> onMaterialSelected)
    {
        if (activePicker == null && colorPickerPrefab != null && uiRoot != null)
        activePicker = Instantiate(colorPickerPrefab, uiRoot);

        if (activePicker != null)
        {
            // Показываем панель с текущим цветом (из currentMaterial)
            activePicker.Show(Color.white);
            activePicker.onColorApplied = (selectedColor) => {
                UpdateMaterial(selectedColor);
                onMaterialSelected?.Invoke(currentMaterial);
            };
        }
        
        else
        // Если нет ColorPicker, просто применяем текущий материал
        onMaterialSelected?.Invoke(currentMaterial);
    }

    // Обновление материала при выборе цвета
    private void UpdateMaterial(Color newColor)
    {
        if (standardShader == null)
        {
            Debug.LogError("Standard shader not assigned!");
            return;
        }

        // Создаём новый материал на основе шейдера
        if (currentMaterial == null)
        currentMaterial = new Material(standardShader);
        
        else
        // Можно перезаписывать, но лучше создавать новый экземпляр, чтобы не портить исходный
        currentMaterial = new Material(standardShader);

        currentMaterial.SetColor(colorProperty, newColor);

        // Сохраняем в PlayerPrefs
        PlayerPrefs.SetFloat(prefRKey, newColor.r);
        PlayerPrefs.SetFloat(prefGKey, newColor.g);
        PlayerPrefs.SetFloat(prefBKey, newColor.b);
        PlayerPrefs.Save();
    }

    // Сброс к исходному цвету блока (передаётся из префаба)
    public void ResetToDefault(Color originalBlockColor)
    {
        UpdateMaterial(originalBlockColor);
    }

    // Получить текущий материал (для спавна)
    public Material GetCurrentMaterial()
    {
        return currentMaterial;
    }
}