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
    private Vector3 scenePos = new Vector3(-2.5f, 1.65f, -9.3f);
    public GameObject[] blockPrefabs;
    public GameObject newBlock;
    public Material rendgenMaterial;
    public Material standartMaterial;
    public Material outlineMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blockPrefabs = Resources.LoadAll<GameObject>("Models/Prefab");
        rendgenMaterial = Resources.Load<Material>("Materials/BlockRendgen");
        standartMaterial = Resources.Load<Material>("Materials/BlockStandart");
        outlineMaterial = Resources.Load<Material>("Materials/MaterialOutline");

        Cursor.lockState = CursorLockMode.Locked;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        playerScript = player.GetComponent<Player>();
    }

    void LoadScene(string sceneName, bool isBuildMode, Vector3 pos, Vector3 rotate)
    {
        playerScript.rotateDirection = new Vector3(0.0f, 0.0f, 0.0f);
        SceneManager.LoadScene(sceneName);
        playerScript.isBuildMode = !isBuildMode;
        playerScript.transform.position = pos;
        playerScript.transform.rotation = Quaternion.Euler(rotate);
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

    public void SpawnBlock(Vector3 spawnPoint, string prefabName, Material blockMaterial)
    {
        foreach(GameObject prefab in blockPrefabs)
        {
            if(prefab.name == prefabName)
            {
                newBlock = Instantiate(prefab, spawnPoint, prefab.transform.rotation);
                newBlock.GetComponent<MeshRenderer>().material = blockMaterial;
                newBlock.name = prefabName;
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.B))
        {
            if(!playerScript.isBuildMode)
            {
                Cursor.lockState = CursorLockMode.None;
                playerScript.distance = playerScript.minDistance;
                LoadScene("BuildScene", playerScript.isBuildMode, zeroPos, zeroPos);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                LoadScene("TestScene", playerScript.isBuildMode, scenePos, zeroPos);
            }
        }
    }
}
