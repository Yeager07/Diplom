using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string freeModeSavePath;
    private Dictionary<string, BlockData> blockDataById;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        freeModeSavePath = Path.Combine(Application.persistentDataPath, "freeModeSave.json");
        BuildBlockDataDictionary();
    }

    private void BuildBlockDataDictionary()
    {
        List<BlockData> all = new List<BlockData>();
        
        foreach(BlockData data in Resources.LoadAll<BlockData>("Models/Blocks/Brick"))
        all.Add(data);
        
        foreach(BlockData data in Resources.LoadAll<BlockData>("Models/Blocks/Cylinders"))
        all.Add(data);
        
        foreach(BlockData data in Resources.LoadAll<BlockData>("Models/Blocks/Plate"))
        all.Add(data);
        
        foreach(BlockData data in Resources.LoadAll<BlockData>("Models/Blocks/RoundPlate"))
        all.Add(data);
        
        foreach(BlockData data in Resources.LoadAll<BlockData>("Models/Blocks/Special"))
        all.Add(data);
        
        foreach(BlockData data in Resources.LoadAll<BlockData>("Models/Blocks/Tile"))
        all.Add(data);
        
        blockDataById = all.ToDictionary(b => b.blockID, b => b);
        
        if(blockDataById.Count == 0)
        Debug.LogWarning("No BlockData found in Resources/BlockData!");
    }

    public void SaveFreeMode()
    {
        FreeModeSaveData data = new FreeModeSaveData();
        data.rootBlocks = new List<BlockSaveData>();

        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        
        foreach(Block block in allBlocks)
        {
            Transform parent = block.transform.parent;
            
            if(parent == null || parent.GetComponent<Block>() == null)
            {
                BlockSaveData rootData = SaveBlockRecursive(block);
                
                if(rootData != null)
                data.rootBlocks.Add(rootData);
            }
        }
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(freeModeSavePath, json);
        Debug.Log("Free mode saved");
    }

    public void SpawnFromSaveData(List<BlockSaveData> rootBlocks, Transform parent = null)
    {
        foreach(BlockSaveData data in rootBlocks)
        {
            BlockData bData = SaveManager.Instance.GetBlockDataById(data.blockDataId);
            
            if(bData == null)
            continue;

            GameObject newBlock = Instantiate(bData.prefab, Vector3.zero, Quaternion.identity);
            newBlock.name = bData.type + " " + bData.blockName;
            newBlock.tag = "Selectable";

            newBlock.transform.position = data.position;
            newBlock.transform.rotation = Quaternion.Euler(data.rotation);

            if(parent != null)
            newBlock.transform.SetParent(parent, true);
 
            Renderer renderer = newBlock.GetComponent<Renderer>();
            
            if(renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = data.color;
                renderer.material = mat;
            }

            Block newBlockComponent = newBlock.GetComponent<Block>();
            
            if(newBlockComponent != null)
            newBlockComponent.blockData = bData;
            
            if(data.children != null && data.children.Count > 0)
            SpawnFromSaveData(data.children, newBlock.transform);
        }
    }

    public BlockData GetBlockDataById(string id)
    {
        blockDataById.TryGetValue(id, out BlockData data);
        return data;
    }

    private BlockSaveData SaveBlockRecursive(Block block)
    {
        BlockSaveData data = new BlockSaveData();
        data.blockDataId = block.blockData.blockID;
        data.position = block.transform.position;
        data.rotation = block.transform.eulerAngles;
        
        Renderer renderer = block.GetComponent<Renderer>();
        data.color = renderer != null ? renderer.material.color : Color.white;
        data.children = new List<BlockSaveData>();
        
        foreach(Transform child in block.transform)
        {
            Block childBlock = child.GetComponent<Block>();
            
            if(childBlock != null)
            data.children.Add(SaveBlockRecursive(childBlock));
        }
        return data;
    }

    public FreeModeSaveData LoadFreeMode()
    {
        if(!File.Exists(freeModeSavePath))
        return null;
        
        string json = File.ReadAllText(freeModeSavePath);
        return JsonUtility.FromJson<FreeModeSaveData>(json);
    }

    public bool HasFreeModeSave() => File.Exists(freeModeSavePath);

    public void DeleteFreeModeSave() => File.Delete(freeModeSavePath);
}

[System.Serializable]
public class BlockSaveData
{
    public string blockDataId;
    public Vector3 position;
    public Vector3 rotation;
    public Color color;
    public List<BlockSaveData> children;
}

[System.Serializable]
public class FreeModeSaveData
{
    public List<BlockSaveData> rootBlocks;
}