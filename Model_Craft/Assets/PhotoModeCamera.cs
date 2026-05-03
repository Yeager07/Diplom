using UnityEngine;

public class PhotoModeCamera : MonoBehaviour
{
    public Transform modelCenter;

    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float rotationSpeed = 2f;
    public float zoomSpeed = 1f;

    private float currentX = 0f;
    private float currentY = 30f;
    private bool isRotating = false;
    private Vector3 lastMousePosition;

    private void Start()
    {
        if(modelCenter != null)
        {
            Vector3 direction = transform.position - modelCenter.position;
            currentX = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentY = Mathf.Asin(direction.y / direction.magnitude) * Mathf.Rad2Deg;
            distance = direction.magnitude;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void Update()
    {
        if(modelCenter == null)
        return;

        if(Input.GetMouseButtonDown(2))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        
        if(Input.GetMouseButtonUp(2))
        isRotating = false;

        if(isRotating)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            currentX += delta.x * rotationSpeed * 0.1f;
            currentY -= delta.y * rotationSpeed * 0.1f;
            currentY = Mathf.Clamp(currentY, -80f, 80f);
            lastMousePosition = Input.mousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 directionVec = rotation * Vector3.back;
        transform.position = modelCenter.position + directionVec * distance;
        transform.LookAt(modelCenter.position);
    }

    public void SetTarget(Transform target)
    {
        modelCenter = target;
        currentX = 0f;
        currentY = 30f;
        distance = 5f;
        transform.LookAt(modelCenter.position);
    }
}