using UnityEngine;

public class RotateModelOnDrag : MonoBehaviour
{
    private float sensitivity = 3.0f;
    private float speed = 1.0f;
    private float minDistance = 4.2f;
    private float maxDistance = 10.0f;
    public float currentDistance = 0.0f;

    public Vector3 lastMousePos;

    void OnMouseDrag()
    {   
        lastMousePos.x -= sensitivity * Input.GetAxis("Mouse Y");
        lastMousePos.y += sensitivity * Input.GetAxis("Mouse X");
        lastMousePos.z = 0;
    }

    private void Move()
    {
        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * speed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        Vector3 negDistance = new Vector3(transform.position.x, transform.position.y, -currentDistance);
        transform.position = negDistance;
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(lastMousePos);
        
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        Move();
    }
}