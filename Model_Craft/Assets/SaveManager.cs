using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string freeModeSavePath;
    private Dictionary<string, BlockData> blockDataById;
    private string gallerySavePath;

    public static bool IsSpawningBlocks = false;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        /*DontDestroyOnLoad(gameObject);*/
        freeModeSavePath = Path.Combine(Application.persistentDataPath, "freeModeSave.json");
        BuildBlockDataDictionary();

        gallerySavePath = Path.Combine(Application.persistentDataPath, "gallery.json");
    }

    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented,
        ContractResolver = new CustomVector3ContractResolver()
    };

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
        data.rootBlocks = CollectRootBlocks();
        string json = JsonConvert.SerializeObject(data, JsonSettings);
        File.WriteAllText(freeModeSavePath, json);
        Debug.Log("Free mode saved");
    }

    public FreeModeSaveData LoadFreeMode()
    {
        if(!File.Exists(freeModeSavePath))
        return null;
        
        string json = File.ReadAllText(freeModeSavePath);
        return JsonConvert.DeserializeObject<FreeModeSaveData>(json, JsonSettings);
    }

    public bool HasFreeModeSave() => File.Exists(freeModeSavePath);

    public void DeleteFreeModeSave() => File.Delete(freeModeSavePath);

    private string GetCareerSavePath(string levelId)
    {
        return Path.Combine(Application.persistentDataPath, $"career_{levelId}.json");
    }

    public void SaveCareerMode(string levelId, CareerSaveData data)
    {
        data.levelId = levelId;
        data.rootBlocks = CollectRootBlocks();
        string json = JsonConvert.SerializeObject(data, JsonSettings);
        File.WriteAllText(GetCareerSavePath(levelId), json);
        Debug.Log($"Career mode saved for level {levelId}");
    }

    public CareerSaveData LoadCareerMode(string levelId)
    {
        string path = GetCareerSavePath(levelId);
        
        if(!File.Exists(path))
        return null;
        
        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<CareerSaveData>(json, JsonSettings);
    }

    public bool HasCareerSave(string levelId) => File.Exists(GetCareerSavePath(levelId));

    public void DeleteCareerSave(string levelId)
    {
        if(File.Exists(GetCareerSavePath(levelId)))
        File.Delete(GetCareerSavePath(levelId));
    }

    public List<BlockSaveData> CollectRootBlocks()
    {
        List<BlockSaveData> roots = new List<BlockSaveData>();
        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
    
        foreach(Block block in allBlocks)
        {
            if(block.transform.parent == null || block.transform.parent.GetComponent<Block>() == null)
            roots.Add(SaveBlockRecursive(block));
        }
        
        return roots;
    }

    public BlockSaveData SaveBlockRecursive(Block block)
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

    public void SpawnFromSaveData(List<BlockSaveData> rootBlocks, Transform parent = null, bool useLocalPosition = false)
    {
        IsSpawningBlocks = true;

        foreach(BlockSaveData data in rootBlocks)
        {
            BlockData bData = GetBlockDataById(data.blockDataId);
            
            if(bData == null)
            continue;

            GameObject newBlock = Instantiate(bData.prefab, Vector3.zero, Quaternion.identity);
            
            Rigidbody rb = newBlock.GetComponent<Rigidbody>();
            
            if(rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            newBlock.name = bData.type + " " + bData.blockName;
            newBlock.tag = "Selectable";

            Block newBlockComponent = newBlock.GetComponent<Block>();
            
            if(newBlockComponent != null)
            {
                newBlockComponent.blockData = bData;
            }

            if(parent != null)
            {   
                newBlock.transform.SetParent(parent, true); // false – не сохранять мировую позицию

                newBlockComponent.place = parent.gameObject;
                
                if(useLocalPosition)
                newBlock.transform.localPosition = data.position;
                
                else
                newBlock.transform.position = data.position;
                
                newBlock.transform.localRotation = Quaternion.Euler(data.rotation);

                newBlockComponent.isFree = false;
                newBlockComponent.isMagnetic = true;
            }
            
            else
            {
                newBlock.transform.position = data.position;
                newBlock.transform.rotation = Quaternion.Euler(data.rotation);
                
                newBlockComponent.isFree = true;
                newBlockComponent.isMagnetic = false;
            }

            Debug.Log($"Spawning {newBlock.name} with parent = {parent?.name}");
 
            Renderer renderer = newBlock.GetComponent<Renderer>();
            
            if(renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = data.color;
                renderer.material = mat;
            }
            
            if(data.children != null && data.children.Count > 0)
            SpawnFromSaveData(data.children, newBlock.transform);
        }
        IsSpawningBlocks = false;
    }

    public BlockData GetBlockDataById(string id)
    {
        blockDataById.TryGetValue(id, out BlockData data);
        return data;
    }

    public void SaveCareerModelToGallery(string levelName, List<BlockSaveData> rootBlocks, string thumbnailPath = null)
    {
        GallerySaveData gallery = LoadGallery();
        
        if(gallery.models.Exists(m => m.type == "Career" && m.levelId == levelName))
        return;
        
        GalleryModelData newModel = new GalleryModelData();
        newModel.id = Guid.NewGuid().ToString();
        newModel.name = levelName;
        newModel.type = "Career";
        newModel.levelId = levelName;
        newModel.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        newModel.blocks = rootBlocks;

        if(string.IsNullOrEmpty(thumbnailPath))
        {
            CareerModelDatabase db = FindFirstObjectByType<CareerModelDatabase>();
            
            if(db != null)
            {
                Sprite thumb = db.GetThumbnail(levelName);
                
                if(thumb != null)
                {
                    string thumbnailsDir = Path.Combine(Application.persistentDataPath, "Thumbnails");
                    
                    if(!Directory.Exists(thumbnailsDir))
                    Directory.CreateDirectory(thumbnailsDir);
                    
                    string fileName = newModel.id + ".png";
                    thumbnailPath = Path.Combine(thumbnailsDir, fileName);
                    Texture2D tex = thumb.texture;
                    byte[] bytes = tex.EncodeToPNG();
                    File.WriteAllBytes(thumbnailPath, bytes);
                }
            }
        }

        newModel.thumbnailPath = thumbnailPath;
        gallery.models.Add(newModel);
        SaveGallery(gallery);
    }

    public void SaveFreeModeModelToGallery(string modelName, List<BlockSaveData> rootBlocks, string thumbnailPath = null)
    {
        GallerySaveData gallery = LoadGallery();
        GalleryModelData newModel = new GalleryModelData();
        newModel.id = Guid.NewGuid().ToString();
        newModel.name = modelName;
        newModel.type = "FreeMode";
        newModel.creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        newModel.blocks = rootBlocks;
        newModel.thumbnailPath = thumbnailPath;
        gallery.models.Add(newModel);
        SaveGallery(gallery);
        Debug.Log($"Модель {modelName} сохранена в галерею{(thumbnailPath != null ? " с миниатюрой" : "")}");
    }

    public GallerySaveData LoadGallery()
    {
        if(!File.Exists(gallerySavePath))
        return new GallerySaveData { models = new List<GalleryModelData>() };
        
        string json = File.ReadAllText(gallerySavePath);
        var data = JsonConvert.DeserializeObject<GallerySaveData>(json, JsonSettings);
        
        if(data == null)
        data = new GallerySaveData();
        
        if(data.models == null)
        data.models = new List<GalleryModelData>();
        
        return data;
    }

    private void SaveGallery(GallerySaveData gallery)
    {
        string json = JsonConvert.SerializeObject(gallery, JsonSettings);
        File.WriteAllText(gallerySavePath, json);
    }

    public List<GalleryModelData> GetAllGalleryModels()
    {
        return LoadGallery().models;
    }

    public void DeleteGalleryModel(string modelId)
    {
        GallerySaveData gallery = LoadGallery();
        GalleryModelData modelToRemove = gallery.models.Find(m => m.id == modelId);
        
        if(modelToRemove != null)
        {
            if(!string.IsNullOrEmpty(modelToRemove.thumbnailPath) && File.Exists(modelToRemove.thumbnailPath))
            {
                File.Delete(modelToRemove.thumbnailPath);
                Debug.Log($"Удалён файл миниатюры: {modelToRemove.thumbnailPath}");
            }

            gallery.models.Remove(modelToRemove);
            SaveGallery(gallery);
            Debug.Log($"Модель {modelId} удалена из галереи");
        }
        
        else
        Debug.LogWarning($"Модель с ID {modelId} не найдена");
    }
}

//[System.Serializable]
public class BlockSaveData
{
    public string blockDataId;
    public Vector3 position;
    public Vector3 rotation;
    public Color color;
    public List<BlockSaveData> children;
}

//[System.Serializable]
public class FreeModeSaveData
{
    public List<BlockSaveData> rootBlocks;
}

//[System.Serializable]
public class CareerSaveData
{
    public string levelId;
    public int currentStepPage;
    public List<int> completedSteps;
    public List<RequiredBlockSaveData> remainingBlocks;
    public List<BlockSaveData> rootBlocks;
}

//[System.Serializable]
public class RequiredBlockSaveData
{
    public string blockFullName;
    public Color color;
    public int remaining;
}

//[System.Serializable]
public class GalleryModelData
{
    public string id;
    public string name;
    public string type;
    public string levelId;
    public string creationDate;
    public List<BlockSaveData> blocks;
    public string thumbnailPath;
}

//[System.Serializable]
public class GallerySaveData
{
    public List<GalleryModelData> models = new List<GalleryModelData>();
}