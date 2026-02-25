using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
//using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private Player playerScript;
    public GameObject[] cell;
    private GameObject cursor;
    public Material outline;
    public bool isHidden = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        cell = GameObject.FindGameObjectsWithTag("Inventory");
        cursor = GameObject.Find("Cursor");

        if(playerScript.inventory.Count == 0)
        {
            for(int i = 0; i < cell.Length; i++)
            {
                cell[i].transform.Find("Count").gameObject.SetActive(false);
                cell[i].transform.Find("Name").gameObject.SetActive(false);
            }
        }
    }

    public void MakeOutline(Transform marker)
    {
        foreach(GameObject image in cell)
        image.GetComponent<Image>().material = null;
        
        marker.GetComponent<Image>().material = outline;
    }

    public void MakeNone(Transform marker)
    {
        marker.GetComponent<Image>().material = null;
    }

    public void SelectItem(int previousInventoryNumber, int currentInventoryNumber)
    {
        cell[previousInventoryNumber-1].GetComponent<Image>().material = null;
        cell[currentInventoryNumber-1].GetComponent<Image>().material = outline;
    }

    public void UpdateInventoryView()
    {
        for(int i = 0; i <= 4; i++)
        {
            Transform count = cell[i].transform.Find("Count");
            Transform name = cell[i].transform.Find("Name");
            count.gameObject.SetActive(true);
            name.gameObject.SetActive(true);
            count.GetComponent<TMP_Text>().text = playerScript.values[i];
            name.GetComponent<TMP_Text>().text = playerScript.keys[i];
        }
    }

    public void SpawnBlock()
    {
        foreach(GameObject bucket in cell)
        {
            if(bucket.GetComponent<Image>().material == outline && bucket.transform.Find("Count").GetComponent<TMP_Text>().text != "")
            {
                Camera.main.GetComponent<MainScript>().SpawnBlock(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerScript.minDistance)),
                bucket.transform.Find("Name").GetComponent<TMP_Text>().text,
                playerScript.materials[int.Parse(bucket.transform.name[int.Parse(bucket.transform.name.Length.ToString()) - 1].ToString()) - 1]);
                playerScript.selectedItem = int.Parse(bucket.transform.name[int.Parse(bucket.transform.name.Length.ToString()) - 1].ToString());
                playerScript.RemoveBlockfromInventory();
                return;
            }
        }
    }

    void FixedUpdate()
    {
        if(playerScript.isBuildMode)
        cursor.gameObject.SetActive(false);
        else
        cursor.gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
