using UnityEngine;
using UnityEngine.EventSystems;

public class FreeModeTrigger : MonoBehaviour
{
    public float scaleSpeed = 5f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private CameraMovement cameraMovement;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        cameraMovement = Camera.main?.GetComponent<CameraMovement>();
    }

    void Update()
    {
        if(cameraMovement != null && cameraMovement.CurrentTargetIndex == 1)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        
        else
        transform.localScale = originalScale; // сброс, если камера не у стола
    }

    void OnMouseDown()
    {
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        return;

        if(cameraMovement == null || cameraMovement.CurrentTargetIndex != 1)
        return;

        Player player = FindFirstObjectByType<Player>();
        
        if(player != null && player.typeGame == "MainMenu")
        ZoneManager.Instance?.StartFreeMode();
    }
}