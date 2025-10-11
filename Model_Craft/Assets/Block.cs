using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    private Vector3 pointScreen;
    private Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void onMouseDown()
    {
        pointScreen = Camera.main.WorldToScreenPoint(transform.position);
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2.0f));
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2.0f);
        Vector3 curPosiotion = Camera.main.ScreenToWorldPoint(curScreenPoint);
        transform.position = curPosiotion;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
