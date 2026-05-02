using UnityEngine;
using System.Collections.Generic;

public class CareerModelDatabase : MonoBehaviour
{
    public List<CareerModelEntry> entries;

    public GameObject GetPrefab(string levelId)
    {
        foreach(var entry in entries)
        {
            if(entry.levelId == levelId)
            return entry.prefab;
        }
        
        return null;
    }
}

[System.Serializable]
public class CareerModelEntry
{
    public string levelId;
    public GameObject prefab;
}