using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;

public class LevelPreviewClick : MonoBehaviour
{
    public int levelIndex;

    void OnMouseDown()
    {
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        return;

        CameraMovement cam = Camera.main?.GetComponent<CameraMovement>();
        
        if(cam == null || cam.CurrentTargetIndex != 1)
        return;

        ZoneManager.Instance?.StartCareerModeByIndex(levelIndex);
    }
}