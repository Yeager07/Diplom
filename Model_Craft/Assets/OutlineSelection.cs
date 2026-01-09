using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class OutlineSelection : MonoBehaviour
{
    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(highlight != null)
        {
            highlight.gameObject.GetComponent<Outline>().enabled = false;
            highlight = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            highlight = raycastHit.transform;
            if(highlight.CompareTag("Selectable") && highlight != selection)
            {
                if(highlight.gameObject.GetComponent<Outline>() != null)
                highlight.gameObject.GetComponent<Outline>().enabled = true;

                else
                {
                    Outline outline = highlight.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                    highlight.gameObject.GetComponent<Outline>().OutlineColor = new Color(1.0f, 0.5193217f, 0.0f, 1.0f);
                    highlight.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
                }
            }

            else
            highlight = null;
        }

        if(Input.GetMouseButtonDown(0))
        {
            if(highlight)
            {
                if(selection != null)
                selection.gameObject.GetComponent<Outline>().enabled = false;

                selection = raycastHit.transform;
                selection.gameObject.GetComponent<Outline>().enabled = true;
                highlight = null;
            }

            else
            {
                if(selection)
                {
                    selection.gameObject.GetComponent<Outline>().enabled = false;
                    selection = null;
                }
            }
        }        
    }
}
