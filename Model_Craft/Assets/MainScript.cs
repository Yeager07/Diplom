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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.B))
        {
            if(!playerScript.isBuildMode)
            {
                Cursor.lockState = CursorLockMode.None;
                LoadScene("BuildScene", playerScript.isBuildMode, zeroPos, zeroPos);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                LoadScene("TestScene", playerScript.isBuildMode, scenePos, zeroPos);
                //playerScript.target = GameObject.FindGameObjectWithTag("Target");
            }
        }
    }
}
