using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class OutlineSelection : MonoBehaviour
{
    public Transform highlight;
    private RaycastHit raycastHit;
    public List<Transform> blockChild;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void SetOutline(Transform selectedObject)
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

    void RemoveOutline(Transform selectedObject)
    {
        selectedObject.gameObject.GetComponent<Outline>().enabled = false;        
    }

    void Update()
    {
        if(highlight != null)
        {
            RemoveOutline(highlight);
            
            foreach(Transform child in blockChild)
            {
                if(child.gameObject.GetComponent<Outline>() != null)
                {
                    RemoveOutline(child);
                }
            }
            
            blockChild.Clear();

            highlight = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            highlight = raycastHit.transform;

            if(highlight.CompareTag("Selectable"))
            {
                SetOutline(highlight);
                
                if(blockChild.Count == 0)
                Camera.main.GetComponent<MainScript>().FindAllChild(highlight, blockChild);

                foreach(Transform child in blockChild)
                {
                    if(child != null && child.gameObject.GetComponent<Outline>() != null)
                    SetOutline(child);
                }
            }

            else
            highlight = null;
        }
    }
}
