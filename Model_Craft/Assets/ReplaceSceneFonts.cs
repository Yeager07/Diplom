#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

public class ReplaceSceneFonts : EditorWindow
{
    [MenuItem("Tools/Replace Fonts in Scene")]
    static void ReplaceFonts()
    {
        // Находим все компоненты TMP_Text в текущей сцене (без сортировки, быстрее)
        TMP_Text[] allTextComponents = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

        // Укажите путь к вашему новому TMP_FontAsset (например, "Assets/Fonts/LegoFont.asset")
        TMP_FontAsset targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Themes/legothick SDF.asset");
        
        if(targetFont == null)
        {
            Debug.LogError("Target font not found! Please set correct path.");
            return;
        }

        foreach(var textComponent in allTextComponents)
        {
            Undo.RecordObject(textComponent, "Replace Font");
            textComponent.font = targetFont;
            EditorUtility.SetDirty(textComponent);
        }
        
        Debug.Log($"Replaced font in {allTextComponents.Length} text components.");
    }
}
#endif