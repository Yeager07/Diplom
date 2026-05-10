using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    public float rotationSpeed = 0.3f;
    private Material skyboxMaterial;

    void Start()
    {
        skyboxMaterial = RenderSettings.skybox;
     
        if(skyboxMaterial == null)
        {
            Debug.LogWarning("Skybox material not found. Disabling rotation.");
            enabled = false;
        }
    }

    void Update()
    {
        if(skyboxMaterial != null)
        {
            float currentRotation = skyboxMaterial.GetFloat("_Rotation");
            float newRotation = currentRotation + rotationSpeed * Time.deltaTime;
            skyboxMaterial.SetFloat("_Rotation", newRotation);
        }
    }
}