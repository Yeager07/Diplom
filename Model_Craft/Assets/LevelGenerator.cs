using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelData data = LevelLoader.SelectedLevel;
        
        if(data == null)
        {
            Debug.LogError("Нет данных уровня!");
            return;
        }

        PdfInstructionViewer pdfViewer = FindFirstObjectByType<PdfInstructionViewer>();
        
        if(pdfViewer != null && pdfViewer.pdfFileName != data.instructionFileName)
        {
            StopCoroutine(pdfViewer.LoadAndSetup());
            pdfViewer.pdfFileName = data.instructionFileName;
            StartCoroutine(pdfViewer.LoadAndSetup());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
