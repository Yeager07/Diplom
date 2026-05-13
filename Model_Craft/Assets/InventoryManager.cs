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

    private bool RemoveBlockData(int selectedItem, Color color, out string blockName)
    {
        blockName = keys[selectedItem - 1];
        
        if(string.IsNullOrEmpty(blockName))
        return false;
        
        if(!materialsCount.ContainsKey(blockName))
        return false;

    // Находим словарь с нужным цветом
        Dictionary<Color, int> targetDict = null;
        
        foreach(var dict in materialsCount[blockName])
        {
            if(dict.ContainsKey(color))
            {
                targetDict = dict;
                break;
            }
        }
        
        if(targetDict == null)
        return false;

        int currentCount = targetDict[color];
        
        if(currentCount > 1)
        {
            targetDict[color]--;
            inventory[blockName]--;
            values[selectedItem - 1] = inventory[blockName].ToString();
        }
        
        else
        {
            materialsCount[blockName].Remove(targetDict);
            inventory[blockName] -= currentCount;
            
            if(inventory[blockName] <= 0)
            {
                inventory.Remove(blockName);
                materialsCount.Remove(blockName);
                keys[selectedItem - 1] = "";
                values[selectedItem - 1] = "";
                GameObject.FindGameObjectWithTag("UI").GetComponent<UI>().MakeNone(cell[selectedItem - 1].transform);
                colorListPanel.GetComponent<InventoryCatalog>().ClosePanel();
            }
            
            else
            values[selectedItem - 1] = inventory[blockName].ToString();
        }
        
        UpdateInventoryView();
        return true;
    }

    public void RemoveBlockFromInventoryNoNotify(int selectedItem, Color color)
    {
        if(RemoveBlockData(selectedItem, color, out string blockName))
        {
        }
    }

    public void RemoveBlockFromInventoryWithNotify(int selectedItem, Color color)
    {
        if(RemoveBlockData(selectedItem, color, out string blockName))
        {
            LevelStepManager stepManager = FindFirstObjectByType<LevelStepManager>();
            
            if(stepManager != null)
            stepManager.OnBlockRemoved(blockName, color, 1, spawn: true, updateCounter: false);
        }
    }

    public void RemoveBlockFromInventoryByColor(int selectedItem, Color color, bool notify = false)
    {
        string blockName = keys[selectedItem - 1];
        
        if(string.IsNullOrEmpty(blockName))
        return;
        
        if(!materialsCount.ContainsKey(blockName))
        return;

    // Находим словарь с нужным цветом
        Dictionary<Color, int> targetDict = null;
        
        foreach(var dict in materialsCount[blockName])
        {
            if(dict.ContainsKey(color))
            {
                targetDict = dict;
                break;
            }
        }
        
        if(targetDict == null)
        return;

        int currentCount = targetDict[color];
        
        if(currentCount > 1)
        {
            targetDict[color]--;
            inventory[blockName]--;
            values[selectedItem - 1] = inventory[blockName].ToString();
        }
        
        else
        {
            materialsCount[blockName].Remove(targetDict);
            inventory[blockName] -= currentCount;
            
            if(inventory[blockName] <= 0)
            {
                inventory.Remove(blockName);
                materialsCount.Remove(blockName);
                keys[selectedItem - 1] = "";
                values[selectedItem - 1] = "";
                GameObject.FindGameObjectWithTag("UI").GetComponent<UI>().MakeNone(cell[selectedItem - 1].transform);
                colorListPanel.GetComponent<InventoryCatalog>().ClosePanel();
            }
            
            else
            values[selectedItem - 1] = inventory[blockName].ToString();
        }
        
        UpdateInventoryView();

        if(notify)
        {
            LevelStepManager stepManager = FindFirstObjectByType<LevelStepManager>();
            
            if(stepManager != null)
            stepManager.OnBlockRemoved(blockName, color, 1);
        }
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

    public void AddToInventory(Transform selectedBlock, bool notify = true)
    {
        if(inventory.Count != 5 && !inventory.ContainsKey(selectedBlock.name))
        {
            inventory.Add(selectedBlock.name, 1);
                
            materialsCount.Add(selectedBlock.name, new List<Dictionary<Color, int>>()
            { new Dictionary<Color, int>()  { { selectedBlock.GetComponent<Renderer>().material.color, 1 } }});
            
            UpdateMassive();

            if(notify)
            NotifyBlockRemoved(selectedBlock);
            
            Destroy(selectedBlock.gameObject);
        }

        else if(inventory.ContainsKey(selectedBlock.name))
        {
            inventory[selectedBlock.name.Replace("(Clone)", "")] += 1;

            foreach(Dictionary<Color, int> dictionary in materialsCount[selectedBlock.name])
            {
                foreach(var value in dictionary)
                {
                    count += 1;
                    
                    if(value.Key == selectedBlock.GetComponent<Renderer>().material.color)
                    {
                        dictionary[selectedBlock.GetComponent<Renderer>().material.color] += 1;
                        UpdateMassive();
                        count = 0;
                        
                        if(notify)
                        NotifyBlockRemoved(selectedBlock);
                        
                        Destroy(selectedBlock.gameObject);
                        return;
                    }
                    
                    if(count == materialsCount[selectedBlock.name].Count)
                    {
                        materialsCount[selectedBlock.name].Add(new Dictionary<Color, int>()  { { selectedBlock.GetComponent<Renderer>().material.color, 1 } });
                        UpdateMassive();
                        count = 0;
                        
                        if(notify)
                        NotifyBlockRemoved(selectedBlock);
                        
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

    public int GetCountForColor(string blockName, Color color)
    {
        if(!materialsCount.ContainsKey(blockName))
        return 0;

        foreach(var dict in materialsCount[blockName])
        {
            foreach(var kv in dict)
            {
                if(kv.Key == color)
                return kv.Value;
            }
        }

        return 0;
    }

    private void NotifyBlockRemoved(Transform block)
    {
        LevelStepManager stepManager = FindFirstObjectByType<LevelStepManager>();
     
        if(stepManager != null)
        {
            string blockName = block.name.Replace("(Clone)", "");
            Renderer rend = block.GetComponent<Renderer>();
            Color color = rend != null ? rend.material.color : Color.white;
            stepManager.OnBlockRemoved(blockName, color, 1, spawn: false, updateCounter: true);
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
}