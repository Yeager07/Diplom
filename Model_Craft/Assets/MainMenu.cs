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
    private int levelIndex = 0;
    private float speed = 20.0f;
    private Vector3 targetPosition = new Vector3(-196.0f, 0.0f, 0.0f);
    public float timer = 4.0f;
    public Button careerButton;
    public Button freeGameButton;
    public Button gulleryButton;
    public Button exitButton;
    public Button nextLevelButton;
    public Button previousLevelButton;
    public List<GameObject> levelPreview = new List<GameObject>();
    public GameObject levelList;
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
        SceneManager.LoadScene("02_TestScene");

        Cursor.lockState = CursorLockMode.Locked;
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

    public void ShowPreviousLevel()
    {
        if(levelIndex != 0)
        {
            nextLevelButton.interactable = true;
            targetPosition += new Vector3(1100.0f, 0.0f, 0.0f);
            
            //levelList.transform.localPosition = levelList.transform.localPosition + new Vector3(1100.0f, 0.0f, 0.0f);
            levelIndex--;
        }

        if(levelIndex == 0)
        previousLevelButton.interactable = false;
    }

    public void ShowNextLevel()
    {
        if(levelIndex != levelPreview.Count - 1)
        {
            previousLevelButton.interactable = true;
            targetPosition -= new Vector3(1100.0f, 0.0f, 0.0f);
            
            //levelList.transform.localPosition = levelList.transform.localPosition + new Vector3(-1100.0f, 0.0f, 0.0f);
            levelIndex++;
        }

        if(levelIndex == levelPreview.Count - 1)
        nextLevelButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(levelList.transform.localPosition != targetPosition)
        levelList.transform.localPosition = Vector3.MoveTowards(levelList.transform.localPosition, targetPosition, speed);
    }
}
