using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainScript : MonoBehaviour
{

    private Player playerScript;
    private Vector3 zeroPos = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 buildPos = new Vector3(1000.0f, 1000.0f, 1000.0f);
    public Dictionary<string, BlockData[]> blockPrefabs = new Dictionary<string, BlockData[]>();
    public List<LevelData> levelDatas= new List<LevelData>();
    public GameObject newBlock;
    public Material rendgenMaterial;
    public Material standartMaterial;
    public Material outlineMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindAllBlockPrefab();
        
        rendgenMaterial = Resources.Load<Material>("Materials/BlockRendgen");
        standartMaterial = Resources.Load<Material>("Materials/BlockStandart");
        outlineMaterial = Resources.Load<Material>("Materials/MaterialOutline");

        foreach(LevelData levelData in Resources.LoadAll<LevelData>("Level"))
        levelDatas.Add(levelData);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if(player != null)
        playerScript = player.GetComponent<Player>();
    }

    void FindAllBlockPrefab()
    {
        blockPrefabs["Brick"] = Resources.LoadAll<BlockData>("Models/Blocks/Brick");
        blockPrefabs["Plate"] = Resources.LoadAll<BlockData>("Models/Blocks/Plate");
        blockPrefabs["Tile"] = Resources.LoadAll<BlockData>("Models/Blocks/Tile");
        blockPrefabs["Slice"] = Resources.LoadAll<BlockData>("Models/Blocks/Slice");
        blockPrefabs["Special"] = Resources.LoadAll<BlockData>("Models/Blocks/Special");
        blockPrefabs["Arch"] = Resources.LoadAll<BlockData>("Models/Blocks/Arch");
        blockPrefabs["Panel"] = Resources.LoadAll<BlockData>("Models/Blocks/Panel");
        blockPrefabs["Cylinders"] = Resources.LoadAll<BlockData>("Models/Blocks/Cylinders");
        blockPrefabs["RoundPlate"] = Resources.LoadAll<BlockData>("Models/Blocks/RoundPlate");
    }

    public void PlacePlayerZero()
    {
        playerScript.rotateDirection = zeroPos;
        playerScript.transform.rotation = Quaternion.Euler(zeroPos);
        playerScript.transform.position = zeroPos;
        playerScript.targetPosition = zeroPos;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void FindAllChild(Transform parentTransform, List<Transform> children)
    {
        foreach(Transform child in parentTransform)
        {
            if(child != null)
            FindAllChild(child, children);

            children.Add(child);
        }

        children.RemoveAll(child => child.CompareTag("Point"));
    }

    public void SpawnBlock(Vector3 spawnPoint, string prefabName, BlockData[] prefabs, Material blockMaterial, Vector3 rotation)
    {
        if(prefabs.Length == 0)
        {
            Debug.Log($"Пустой массив префабов");
            return;
        }

        foreach(BlockData prefab in prefabs)
        {
            if(prefab.name == prefabName)
            {
                newBlock = Instantiate(prefab.prefab, spawnPoint, Quaternion.Euler(rotation));
                newBlock.GetComponent<MeshRenderer>().material = blockMaterial;
                newBlock.name = prefabName;
                newBlock.GetComponent<Block>().blockData = prefab;
                newBlock.GetComponent<Block>().isInPickupZone = (newBlock.transform.position.x <= 800f);
                playerScript.movedObject = newBlock;
                return;
            }
        }
    }

    public void MakeObjectGravity(GameObject currentObject)
    {
        if(SceneManager.GetActiveScene().name == "02_TestScene" && !playerScript.isBuildMode && currentObject.transform.position.x < 800.0f)
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<Rigidbody>().useGravity = true;
            currentObject.GetComponent<Collider>().isTrigger = false;
        }

        else
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<Rigidbody>().useGravity = false;
            currentObject.GetComponent<Collider>().isTrigger = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Transform colorChoosePanel = playerScript.transform.Find("UI").transform.Find("AdvancesColorPickerPanelPrefab(Clone)");
        
        if(Input.GetKeyUp(KeyCode.B) && playerScript.typeGame == "CareerMode" &&
        !playerScript.transform.Find("UI").Find("PauseMenu").gameObject.activeInHierarchy)
        {   
            if(colorChoosePanel == null || !colorChoosePanel.gameObject.activeInHierarchy)
            {
                if(!playerScript.isBuildMode)
                {
                    Cursor.lockState = CursorLockMode.None;
                    playerScript.currentDistance = playerScript.minDistance;
                    playerScript.isBuildMode = true;

                    playerScript.rotateDirection = zeroPos;
                    playerScript.transform.rotation = Quaternion.Euler(zeroPos);
                    playerScript.targetPosition = buildPos;
                }
                
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    playerScript.colorListPanel.gameObject.SetActive(false);
                    playerScript.isBuildMode = false;

                    PlacePlayerZero();
                }
            }

            else
            return;
        }
    }
}
