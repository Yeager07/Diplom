using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private Player playerScript;
    public Button careerButton;
    public Button freeGameButton;
    public Button gulleryButton;
    public Button exitButton;
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
        playerScript.isBuildMode = true;
        playerScript.transform.Find("UI").gameObject.SetActive(true);
        playerScript.transform.Find("UI").transform.Find("Instruction").transform.Find("InstructionDownload").gameObject.SetActive(true);
        playerScript.transform.Find("UI").Find("BlocksIcon").gameObject.SetActive(true);
    }

    public void OpenCareerMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene("02_TestScene");
        playerScript.typeGame = "CareerMode";
        playerScript.transform.Find("UI").gameObject.SetActive(true);
        playerScript.transform.Find("UI").transform.Find("Instruction").transform.Find("InstructionDownload").gameObject.SetActive(false);
        
        Camera.main.GetComponent<MainScript>().PlacePlayerZero();
    }

    public void ChangeLanguageMode()
    {
        if(playerScript.language == "En")
        playerScript.language = "Ru";
        
        else
        playerScript.language = "En";

        ChangeLanguage();
    }
    
    private void ChangeLanguage()
    {
        if(playerScript.language == "En")
        {
            careerButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Career Mode";
            freeGameButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Free Game Mode";
            gulleryButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Gullery Mode";
            exitButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Exit";
        }
        
        else
        {
            careerButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Режим Карьеры";
            freeGameButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Свободная Сборка";
            gulleryButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Галерея";
            exitButton.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Выход";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
