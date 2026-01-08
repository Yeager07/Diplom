using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    private Player playerScript;
    private Vector3 pointScreen;
    public List<Vector3> previousRotate = new List<Vector3>();
    private Vector3 rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 curPosition;
    private Color outlineColor = Color.red;
    public float outlineWidth = 0.03f;
    private bool copyOriginalTexture = true;
    private Material outlineMaterial;
    private Material[] originalMaterials;
    private Renderer objectRenderer;
    public bool isActive = false;
    public List<Vector3> positionHistory = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        playerScript = player.GetComponent<Player>();

        positionHistory.Add(transform.position);
        previousRotate.Add(rotateDirection);
        //обводка
        objectRenderer = GetComponent<Renderer>();
        originalMaterials = objectRenderer.materials;
        
        outlineMaterial = new Material(Shader.Find("Custom/OutlineShader"));
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);

        if(copyOriginalTexture && originalMaterials.Length > 0)
        CopyMaterialProperties(originalMaterials[0], outlineMaterial);
    }

    void CopyMaterialProperties(Material source, Material destination)
    {
        if(source.HasProperty("_Color"))
        destination.SetColor("_Color", source.GetColor("_Color"));

        string[] possibleTextureProperties = { "_MainTex", "_BaseMap", "_BaseColorMap", "_Albedo" };
        
        foreach (string propName in possibleTextureProperties)
        {
            if (source.HasProperty(propName))
            {
                Texture texture = source.GetTexture(propName);
                if (texture != null)
                {
                    destination.SetTexture("_MainTex", texture);
                    
                    // Копируем масштаб и смещение текстуры
                    if (source.HasProperty(propName + "_ST"))
                    {
                        Vector4 st = source.GetVector(propName + "_ST");
                        destination.SetVector("_MainTex_ST", st);
                    }
                    break;
                }
            }
        }
    }

    void ApplyOutline()
    {
        if (objectRenderer == null || outlineMaterial == null) return;
        
        // Заменяем все материалы на материал с обводкой
        Material[] outlineMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < outlineMaterials.Length; i++)
        outlineMaterials[i] = outlineMaterial;

        objectRenderer.materials = outlineMaterials;
    }

    void RemoveOutline()
    {
        if (objectRenderer == null)
        return;
        
        objectRenderer.materials = originalMaterials;
    }

    void SavePosition(Vector3 position)
    {
        positionHistory.Add(position);

        if(positionHistory.Count > 20)
        positionHistory.RemoveAt(0);
    }

    void onMouseDown()
    {
        if(playerScript.isBuildMode)
        pointScreen = Camera.main.WorldToScreenPoint(transform.position);
    }

    void OnMouseDrag()
    {
        if(playerScript.isBuildMode)
        {   
            if(!Input.GetKey(KeyCode.R))
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.distance);
                curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
            }

            transform.position = curPosition;
        }
    }

    void OnMouseEnter()
    {
        if(playerScript.isBuildMode)
        {
            isActive = true;
            playerScript.target = gameObject.transform.position;
            ApplyOutline();
        }
    }

    void OnMouseExit()
    {
        if(playerScript.isBuildMode)
        {
            isActive = false;
            RemoveOutline();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        SavePosition(transform.position);

        if(Input.GetKey(KeyCode.R) && isActive)
        {
                
            if(Input.GetKeyUp(KeyCode.UpArrow))
            rotateDirection.x += 90.0f;

            if(Input.GetKeyUp(KeyCode.DownArrow))
            rotateDirection.x -= 90.0f;

            if(Input.GetKeyUp(KeyCode.LeftArrow))
            rotateDirection.y -= 90.0f;

            if(Input.GetKeyUp(KeyCode.RightArrow))
            rotateDirection.y += 90.0f;

            rotateDirection.z = 0;
            transform.rotation = Quaternion.Euler(rotateDirection);
        }

        if(Input.GetKeyUp(KeyCode.R))
        {

            SavePosition(previousRotate[previousRotate.Count - 1]);
            previousRotate.Add(new Vector3(rotateDirection.x % 360, rotateDirection.y % 360, rotateDirection.z));

        }

        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            if(positionHistory.Count >= 1)
            {
                if(previousRotate.Count > 1 && previousRotate.Contains(positionHistory[positionHistory.Count-1]))
                {
                    transform.rotation = Quaternion.Euler(positionHistory[positionHistory.Count - 1]);
                    rotateDirection = positionHistory[positionHistory.Count - 1];
                    previousRotate.RemoveAt(previousRotate.Count-1);
                }

                else
                transform.position = positionHistory[positionHistory.Count - 1];

                positionHistory.RemoveAt(positionHistory.Count - 1);
                return;
            }
        }
    }
}
