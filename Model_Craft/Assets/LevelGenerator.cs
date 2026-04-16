using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform spawnPoint;
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

        /*foreach(RequiredBlock req in data.requiredBlocks)
        {
            for(int i = 0; i < req.count; i++)
            {
                //GameObject block = Instantiate(req.block.prefab, spawnPoint.position, Quaternion.identity);
                Camera.main.GetComponent<MainScript>().SpawnBlock(spawnPoint.position, req.block.type + " " + req.block.blockName,
                Camera.main.GetComponent<MainScript>().blockPrefabs[req.block.type.ToString()], Camera.main.GetComponent<MainScript>().standartMaterial);
                
                GameObject block = Camera.main.GetComponent<MainScript>().newBlock;
                
                Renderer renderer = block.GetComponent<Renderer>();
                
                if(renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = req.color;
                    renderer.material = mat;
                }
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
