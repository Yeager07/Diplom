using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class OutlineSelection : MonoBehaviour
{
    public Transform highlight;
    private RaycastHit raycastHit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    public void SetOutline(Transform selectedObject)
    {
        if(selectedObject.gameObject.GetComponent<Outline>() != null)
        selectedObject.gameObject.GetComponent<Outline>().enabled = true;

        else
        {
            Outline outline = selectedObject.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            selectedObject.gameObject.GetComponent<Outline>().OutlineColor = new Color(1.0f, 0.5193217f, 0.0f, 1.0f);
            selectedObject.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
        }
    }

    public void RemoveOutline(Transform selectedObject)
    {
        selectedObject.gameObject.GetComponent<Outline>().enabled = false;        
    }

    void Update()
    {
        if(highlight != null)
        {
            RemoveOutline(highlight);
            
            foreach(Transform child in highlight.transform)
            {
                Block childBlock = child.GetComponent<Block>();
                    
                if(childBlock != null && child.gameObject.GetComponent<Outline>() != null)
                RemoveOutline(child);
            }

            highlight = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            highlight = raycastHit.transform;

            if(highlight.CompareTag("Selectable"))
            {
                SetOutline(highlight);

                foreach(Transform child in highlight.transform)
                {
                    Block childBlock = child.GetComponent<Block>();
                        
                    if(childBlock != null && child.gameObject.GetComponent<Outline>() != null)
                    SetOutline(child);
                }
            }

            else
            highlight = null;
        }
    }
}
