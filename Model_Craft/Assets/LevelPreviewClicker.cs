using UnityEngine;

public class LevelPreviewClick : MonoBehaviour
{
    public int levelIndex; // устанавливается из ZoneManager

    void OnMouseDown()
    {
        // Загружаем карьерный режим для выбранного уровня
        ZoneManager.Instance.StartCareerModeByIndex(levelIndex);
    }
}