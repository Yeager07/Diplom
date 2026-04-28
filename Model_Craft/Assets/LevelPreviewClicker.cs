using UnityEngine;

public class LevelPreviewClick : MonoBehaviour
{
    public int levelIndex;

    void OnMouseDown()
    {
        CameraMovement cam = Camera.main?.GetComponent<CameraMovement>();
        
        if(cam == null || cam.CurrentTargetIndex != 1)
        return;

        ZoneManager.Instance?.StartCareerModeByIndex(levelIndex);
    }
}