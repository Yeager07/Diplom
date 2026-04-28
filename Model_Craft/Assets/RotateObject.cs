using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 30, 0);
    
    public float hoverScaleMultiplier = 1.45f;
    public float scaleSpeed = 5f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    //private bool isHovering = false;
    private CameraMovement cameraMovement;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        cameraMovement = Camera.main?.GetComponent<CameraMovement>();
    }
   
    void OnMouseEnter()
    {
        if(cameraMovement != null && cameraMovement.CurrentTargetIndex == 1)
        targetScale = originalScale * hoverScaleMultiplier;
        //isHovering = true;
    }

    void OnMouseExit()
    {
        targetScale = originalScale;
        //isHovering = false;
    }

    void Update()
    {
        if(cameraMovement == null || cameraMovement.CurrentTargetIndex != 1)
        {
            transform.localScale = originalScale;
            return;
        }

        transform.Rotate(rotationSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }
}