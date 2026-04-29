using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FreeModeLoader : MonoBehaviour
{
    void Start()
    {
        if(ZoneManager.PendingFreeModeSave != null)
        {
            LoadBlocks(ZoneManager.PendingFreeModeSave);
            ZoneManager.PendingFreeModeSave = null;
        }
    }

    private void LoadBlocks(FreeModeSaveData data)
    {
        ClearAllBlocks();
        SpawnBlocksFromData(data.rootBlocks);

        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        
        foreach(Block block in allBlocks)
        {
            RotateObject rot = block.GetComponent<RotateObject>();
            
            if(rot != null)
            Destroy(rot);

            if(block.transform.parent != null)
            {
                block.isFree = false;
                block.isMagnetic = true;
            }

            else
            {
                block.isFree = true;
                block.isMagnetic = false;
            }
        }
    
        StartCoroutine(RecalculateAfterLoad());
    }

    IEnumerator RecalculateAfterLoad()
    {
        yield return null;
        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        
        foreach(Block block in allBlocks)
        {
            //block.transform.hasChanged = true;

            if(block.transform.parent == null || block.transform.parent.GetComponent<Block>() == null)
            block.RecalculateAllPoints();
        }
    }

    private void ClearAllBlocks()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Selectable");
        
        foreach(GameObject b in blocks)
        Destroy(b);
        
        Block.connections.Clear();
    }

    private void SpawnBlocksFromData(List<BlockSaveData> rootBlocks, Transform parent = null)
    {
        SaveManager.Instance.SpawnFromSaveData(rootBlocks, parent);
    }
}