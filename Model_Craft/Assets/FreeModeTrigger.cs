using UnityEngine;

public class FreeModeTrigger : MonoBehaviour
{
    public float hoverScale = 1.1f;
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

    void OnMouseEnter()
    {
        if(cameraMovement != null && cameraMovement.CurrentTargetIndex == 1)
        targetScale = originalScale * hoverScale;
    }

    void OnMouseExit()
    {
        targetScale = originalScale;
    }

    void OnMouseDown()
    {
        if(cameraMovement == null || cameraMovement.CurrentTargetIndex != 1)
        return;

        Player player = FindFirstObjectByType<Player>();
        
        if(player != null && player.typeGame == "MainMenu")
        ZoneManager.Instance?.StartFreeMode();
    }
}