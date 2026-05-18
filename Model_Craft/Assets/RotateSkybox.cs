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
        Material skybox = RenderSettings.skybox;
        
        if(skybox != null && skybox.HasProperty("_Rotation"))
        {
            float currentRotation = skybox.GetFloat("_Rotation");
            float newRotation = currentRotation + rotationSpeed * Time.deltaTime;
            skybox.SetFloat("_Rotation", newRotation);
        }
    }
}