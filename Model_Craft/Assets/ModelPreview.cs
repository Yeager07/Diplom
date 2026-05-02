using UnityEngine;
using System.Collections.Generic;

public class ModelPreview : MonoBehaviour
{
    public Transform previewParent;
    public Transform previewSpawnPoint;
    public bool enableRotation = true;
    public float rotationSpeed = 2f;

    private bool isDragging = false;
    private Vector3 lastMousePosition;

    public void ShowModel(List<BlockSaveData> rootBlocks)
    {
        ClearPreview();
        
        if(rootBlocks == null || rootBlocks.Count == 0)
        return;

        SaveManager.Instance.SpawnFromSaveData(rootBlocks, previewParent, true); // true – локальные координаты

        Vector3 min = Vector3.one * float.MaxValue;
        Vector3 max = Vector3.one * float.MinValue;

        foreach(Transform child in previewParent)
        {
            Vector3 pos = child.localPosition;
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }
        
        Vector3 center = (min + max) * 0.5f;
        
        foreach(Transform child in previewParent)        
        child.localPosition -= center;

        previewParent.position = previewSpawnPoint.position;
        previewParent.rotation = previewSpawnPoint.rotation;

        if(enableRotation)
        AddColliderRecursive(previewParent);
    }

    public void ClearPreview()
    {
        if(previewParent != null)
        {
            foreach(Transform child in previewParent)
            Destroy(child.gameObject);
        }

        Block.connections.Clear();
    }

    private void AddColliderRecursive(Transform parent)
    {
        foreach(Transform child in parent)
        {
            Block block = child.GetComponent<Block>();
            
            if(block != null && block.GetComponent<Collider>() == null)
            {
                BoxCollider bc = child.gameObject.AddComponent<BoxCollider>();
                bc.isTrigger = false;
            }
            AddColliderRecursive(child);
        }
    }

    public void HidePreview()
    {
        ClearPreview();
    }

    void Update()
    {
        if(!enableRotation || previewParent == null)
        return;

        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
          
            if(Physics.Raycast(ray, out hit) && hit.transform.IsChildOf(previewParent))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
        }
        
        else if(Input.GetMouseButtonUp(0))
        isDragging = false;

        if(isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            previewParent.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            previewParent.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
            lastMousePosition = Input.mousePosition;
        }
    }
}