using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Security.Cryptography;

public class InventoryManager : MonoBehaviour
{
    private Player playerScript;
    public GameObject[] cell;
    public GameObject inventoryIcon;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public string[] keys;
    public string[] values;
    public Dictionary<string, List<Material>> materials = new Dictionary<string, List<Material>>();
    public Dictionary<string, Dictionary<Material, int>> materialsCount = new Dictionary<string, Dictionary<Material, int>>();
    public Material material1Material;
    public int material1MaterialCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        for(int i = 0; i < cell.Length; i++)
        {
            cell[i].transform.Find("Count").gameObject.SetActive(false);
            cell[i].transform.Find("Name").gameObject.SetActive(false);
        }
    }

    public void RemoveBlockfromInventory(int selectedItem, int previousSelectedItem)
    {
        if(keys[selectedItem - 1] != "")
        {
            if(inventory[keys[selectedItem-1]] != 1)
            {
                inventory[keys[selectedItem - 1]] -= 1;
                materials[keys[selectedItem - 1]].RemoveAt(materials[keys[selectedItem - 1]].Count - 1);
                values[selectedItem - 1] = (int.Parse(values[selectedItem - 1]) - 1).ToString();
            }

            else
            {
                inventory.Remove(keys[selectedItem-1]);
                materials.Remove(keys[selectedItem - 1]);
                materialsCount.Remove(keys[selectedItem - 1]);
                keys[selectedItem - 1] = "";
                values[selectedItem - 1] = "";
                previousSelectedItem = selectedItem;
                selectedItem = 0;
                GameObject.FindGameObjectWithTag("UI").GetComponent<UI>().MakeNone(cell[previousSelectedItem - 1].transform);
            }

            UpdateInventoryView();
            playerScript.OutlinedSelectedItem();
        }
        else
        return;
    }

    private void UpdateMassive()
    {
        int iterator = 0;
        foreach(var value in inventory)
        {
            keys[iterator] = value.Key;
            values[iterator] = value.Value.ToString();
            iterator += 1;
        }

        UpdateInventoryView();
    }

    public void AddToInventory(Transform selectedBlock)
    {
        if(inventory.Count != 5 && !inventory.ContainsKey(selectedBlock.name))
        {
            inventory.Add(selectedBlock.name, 1);

            materials.Add(selectedBlock.name, new List<Material>());
            materials[selectedBlock.name].Add(selectedBlock.GetComponent<Renderer>().material);
                
            materialsCount.Add(selectedBlock.name, new Dictionary<Material, int>());
            materialsCount[selectedBlock.name].Add(selectedBlock.GetComponent<Renderer>().material, 1);

            foreach(var item in materialsCount[selectedBlock.name])
            {
                material1Material = item.Key;
                material1MaterialCount = item.Value;
            }
            
            UpdateMassive();
        }

        else if(inventory.ContainsKey(selectedBlock.name))
        {

            inventory[selectedBlock.name] += 1;
            
            foreach(Material material in materials[selectedBlock.name])
            {
                if(material == selectedBlock.GetComponent<Renderer>().material)
                {
                    materialsCount[selectedBlock.name][selectedBlock.GetComponent<Renderer>().material] += 1;
                    return;
                }
            }

            materials[selectedBlock.name].Add(selectedBlock.GetComponent<Renderer>().material);
            UpdateMassive();
        }

        else
        return;
    }

    public void UpdateInventoryView()
    {
        int countBlock = 0;
        Transform allCount = inventoryIcon.transform.Find("Count");

        for(int i = 0; i <= 4; i++)
        {
            Transform count = cell[i].transform.Find("Count");
            Transform name = cell[i].transform.Find("Name");
            Transform type = cell[i].transform.Find("Type");

            count.gameObject.SetActive(true);
            name.gameObject.SetActive(true);
            count.GetComponent<TMP_Text>().text = values[i];
            name.GetComponent<TMP_Text>().text = keys[i];
            
            if(values[i] != "")
            countBlock += int.Parse(values[i]);
        }
        
        allCount.GetComponent<TMP_Text>().text = countBlock.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}