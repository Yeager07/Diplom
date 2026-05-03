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

    public Sprite GetThumbnail(string levelId)
    {
        foreach (var entry in entries)
        {
            if(entry.levelId == levelId)
            return entry.thumbnail;
        }
        
        return null;
    }
}

[System.Serializable]
public class CareerModelEntry
{
    public string levelId;
    public GameObject prefab;
    public Sprite thumbnail;
}