using UnityEngine;

public class RotateLoader : MonoBehaviour
{
    public float rotationSpeed = 200.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f, 0.0f, rotationSpeed * Time.deltaTime);
    }
}
