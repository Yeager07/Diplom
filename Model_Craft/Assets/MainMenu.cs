using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    private Player playerScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        playerScript = player.GetComponent<Player>();
    }

    public void OpenFreeMode()
    {
        SceneManager.LoadScene("04_FreeMode");
        playerScript.typeGame = "FreeMode";
        playerScript.transform.Find("UI").gameObject.SetActive(true);
        playerScript.isBuildMode = true;
        playerScript.transform.Find("UI").Find("BlocksIcon").gameObject.SetActive(true);;
    }

    public void OpenCareerMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene("02_TestScene");
        playerScript.typeGame = "CareerMode";
        playerScript.transform.Find("UI").gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
