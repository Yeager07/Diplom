using UnityEngine;

public class ThemeSkyboxSetter : MonoBehaviour
{
    public Material skyboxMaterial;

    void Start()
    {
        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();
    }
}