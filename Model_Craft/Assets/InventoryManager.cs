using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    private Player playerScript;
    public GameObject colorListPanel;
    public GameObject[] cell;
    public GameObject inventoryIcon;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public string[] keys;
    public string[] values;
    public Dictionary<string, List<Dictionary<Color, int>>> materialsCount = new Dictionary<string, List<Dictionary<Color, int>>>();
    public int count = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();

        for(int i = 0; i < cell.Length; i++)
        cell[i].transform.Find("Count").gameObject.SetActive(false);
    }

    public void RemoveBlockfromInventory(int selectedItem, int previousSelectedItem)
    {
        if(keys[selectedItem - 1] != "")
        {
            if(inventory[keys[selectedItem - 1]] != 1)
            {
                inventory[keys[selectedItem - 1]] -= 1;
                values[selectedItem - 1] = (int.Parse(values[selectedItem - 1]) - 1).ToString();
                
                foreach(Dictionary<Color, int> dictionary in materialsCount[keys[selectedItem - 1]])
                {
                    foreach(var value in dictionary)
                    {
                        if(value.Value == 0)
                        {
                            materialsCount[keys[selectedItem - 1]].Remove(dictionary);
                            UpdateInventoryView();
                            return;
                        }
                    }
                }
            }

            else
            {
                inventory.Remove(keys[selectedItem-1]);
                materialsCount.Remove(keys[selectedItem - 1]);
                keys[selectedItem - 1] = "";
                values[selectedItem - 1] = "";
                previousSelectedItem = selectedItem;
                selectedItem = 0;
                GameObject.FindGameObjectWithTag("UI").GetComponent<UI>().MakeNone(cell[previousSelectedItem - 1].transform);
                colorListPanel.GetComponent<InventoryCatalog>().ClosePanel();
            }

            UpdateInventoryView();
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
            
            if(keys[iterator] != "")
            {
                if(keys[iterator].Split(" ").Length == 2)
                cell[iterator].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Icon/{keys[iterator].Split(" ")[0]}/{keys[iterator].Split(" ")[1]}");

                else
                cell[iterator].GetComponent<Image>().sprite = Resources.Load<Sprite>($"Icon/{keys[iterator].Split(" ")[0]}/{keys[iterator].Split(" ")[1]} {keys[iterator].Split(" ")[2]}");

            }
            
            iterator += 1;
        }

        UpdateInventoryView();
    }

    public void AddToInventory(Transform selectedBlock)
    {
        if(inventory.Count != 5 && !inventory.ContainsKey(selectedBlock.name))
        {
            inventory.Add(selectedBlock.name, 1);
                
            materialsCount.Add(selectedBlock.name, new List<Dictionary<Color, int>>()
            { new Dictionary<Color, int>()  { { selectedBlock.GetComponent<Renderer>().material.color, 1 } }});

            Debug.Log($"Добавляю новый цвет {selectedBlock.GetComponent<Renderer>().material.color}");
            
            UpdateMassive();

            Destroy(selectedBlock.gameObject);
        }

        else if(inventory.ContainsKey(selectedBlock.name))
        {
            inventory[selectedBlock.name] += 1;

            foreach(Dictionary<Color, int> dictionary in materialsCount[selectedBlock.name])
            {
                foreach(var value in dictionary)
                {
                    count += 1;
                    
                    if(value.Key == selectedBlock.GetComponent<Renderer>().material.color)
                    {
                        Debug.Log($"Такой материал есть, увеличиваю количество для {selectedBlock.GetComponent<Renderer>().material.color}");
                        dictionary[selectedBlock.GetComponent<Renderer>().material.color] += 1;
                        UpdateMassive();
                        count = 0;
                        Destroy(selectedBlock.gameObject);
                        return;
                    }
                    
                    if(count == materialsCount[selectedBlock.name].Count)
                    {
                        Debug.Log($"Такого цвета нет, добавляю его как новый {selectedBlock.GetComponent<Renderer>().material.color}");
                        materialsCount[selectedBlock.name].Add(new Dictionary<Color, int>()  { { selectedBlock.GetComponent<Renderer>().material.color, 1 } });
                        UpdateMassive();
                        count = 0;
                        Destroy(selectedBlock.gameObject);
                        return;
                    }
                }

            }
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
            count.gameObject.SetActive(true);
            count.GetComponent<TMP_Text>().text = values[i];

            if(values[i] != "")
            countBlock += int.Parse(values[i]);

            if(values[i] == "")
            cell[i].GetComponent<Image>().sprite = null;
        }
        
        allCount.GetComponent<TMP_Text>().text = countBlock.ToString();
        
        if(playerScript.selectedItem != 0)
        colorListPanel.GetComponent<InventoryCatalog>().RefreshCatalog();
    }

    // Update is called once per frame
    void Update()
    {
    }
}