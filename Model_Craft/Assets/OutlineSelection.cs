using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class OutlineSelection : MonoBehaviour
{
    private Transform currentHighlight;
    private bool updatesEnabled = true;

    void Update()
    {
        if(!updatesEnabled)
        return;

        Transform newHighlight = GetBlockUnderMouse();
        
        if(currentHighlight != newHighlight)
        {
            if(currentHighlight != null)
            SetOutlineOnSelfAndDescendants(currentHighlight, false);

            currentHighlight = newHighlight;
            
            if(currentHighlight != null)
            SetOutlineOnSelfAndDescendants(currentHighlight, true);
        }
    }

    private Transform GetBlockUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if(!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit))
        {
            Transform hitTransform = hit.transform;
            
            if(hitTransform.CompareTag("Selectable"))
            return hitTransform;
        }
        
        return null;
    }

    private void SetOutlineOnSelfAndDescendants(Transform root, bool enable)
    {
        if(root == null)
        return;
        
        SetOutline(root, enable);
        
        foreach (Transform child in root.GetComponentsInChildren<Transform>())
        {
            if(child != root && child.CompareTag("Selectable"))
            SetOutline(child, enable);
        }
    }

    private void SetOutline(Transform obj, bool enable)
    {
        if(obj == null)
        return;
        
        Outline outline = obj.GetComponent<Outline>();
        
        if(enable)
        {
            if(outline == null)
            {
                outline = obj.gameObject.AddComponent<Outline>();
                outline.OutlineColor = new Color(1f, 0.519f, 0f, 1f);
                outline.OutlineWidth = 7f;
            }
            
            outline.enabled = true;
        }
        
        else
        {
            if(outline != null)
            outline.enabled = false;
        }
    }

    public void ClearCurrentHighlight()
    {
        updatesEnabled = false;

        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        
        foreach(Block block in allBlocks)
        {
            Outline outline = block.GetComponent<Outline>();
            
            if(outline != null)
            Destroy(outline);
        }
        
        currentHighlight = null;

        StartCoroutine(EnableUpdatesNextFrame());
    }

    private IEnumerator EnableUpdatesNextFrame()
    {
        yield return null;
        
        updatesEnabled = true;
    }
}