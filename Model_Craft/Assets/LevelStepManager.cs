using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelStepManager : MonoBehaviour
{
    public Transform spawnPoint;
    private PdfInstructionViewer pdfViewer;

    private LevelData currentLevel;
    private Dictionary<string, int> remainingForCurrentStep = new Dictionary<string, int>(); // blockName -> остаток
    private int currentStepPage = -1;
    private bool stepCompleted = true; // текущий шаг выполнен (все детали использованы)
    private bool isSpawning = false;

    void Start()
    {
        pdfViewer = GameObject.FindGameObjectWithTag("Player").transform.Find("PdfViewer").GetComponent<PdfInstructionViewer>();

        currentLevel = LevelLoader.SelectedLevel;
        
        if(currentLevel == null)
        {
            Debug.LogError("No level data!");
            return;
        }

        if(pdfViewer == null)
        pdfViewer = FindFirstObjectByType<PdfInstructionViewer>();

        if(pdfViewer != null)
        {
            pdfViewer.OnPageChanged += OnPageChanged;
            StartCoroutine(WaitForFirstPage());
        }
    }

    private IEnumerator WaitForFirstPage()
    {
        // Ждём, пока загрузится первая страница
        while(pdfViewer.pageTextures == null || pdfViewer.pageTextures.Length == 0)
        yield return null;
        
        int firstPage = pdfViewer.currentPageIndex + 1;
        OnPageChanged(firstPage);
    }

    private bool AreThereBlocksInPickupZone()
    {
        GameObject[] allBlocks = GameObject.FindGameObjectsWithTag("Selectable");
    
        foreach (GameObject block in allBlocks)
        {
            if(block.transform.position.x <= 800f) // зона подбора
            return true;
        }
        
        return false;
    }

    private void OnPageChanged(int pageNumber)
    {
        if(isSpawning)
        return;

        Step step = currentLevel.steps.Find(s => s.pageNumber == pageNumber);
        
        if(step == null)
        return;

        // Если уже на этом шаге и шаг не завершён, но в зоне подбора нет блоков – регенерируем (игрок удалил все)
        if(currentStepPage == pageNumber)
        {
            if (!stepCompleted && !AreThereBlocksInPickupZone())
            StartCoroutine(SpawnStepWithDelay(step, pageNumber));
        
            return;
        }

       // Переход на новый шаг
        if(AreThereBlocksInPickupZone())
        {
            Debug.Log("В зоне подбора есть блоки. Подберите их или перенесите в зону сборки, чтобы перейти к следующему шагу.");
            return;
        }

        if(!stepCompleted && currentStepPage != -1)
        {
            Debug.Log("Сначала используйте все детали текущего шага!");
            return;
        }

        StartCoroutine(SpawnStepWithDelay(step, pageNumber));
    }

    private IEnumerator SpawnStepWithDelay(Step step, int pageNumber)
    {
        isSpawning = true;
        yield return new WaitForSeconds(0.2f);
        SpawnStep(step);
        currentStepPage = pageNumber;
        stepCompleted = false;
        isSpawning = false;
    }

    private void SpawnStep(Step step)
    {
        remainingForCurrentStep.Clear();
        
        foreach(RequiredBlock req in step.blocks)
        {
            for(int i = 0; i < req.count; i++)
            SpawnBlock(req.block, req.color);
            
            remainingForCurrentStep[req.block.blockName] = req.count;
        }
    }

    private void SpawnBlock(BlockData block, Color color)
    {
        // Используем ваш метод спавна из MainScript
        Camera.main.GetComponent<MainScript>().SpawnBlock(spawnPoint.position, block.type + " " + block.blockName,
            Camera.main.GetComponent<MainScript>().blockPrefabs[block.type.ToString()], 
            Camera.main.GetComponent<MainScript>().standartMaterial);
        
        GameObject newBlock = Camera.main.GetComponent<MainScript>().newBlock;
        newBlock.tag = "Selectable"; // устанавливаем тег
        Renderer renderer = newBlock.GetComponent<Renderer>();
        
        if(renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }
    }

    private void SpawnBlockAtSpawnPoint(BlockData block, Color color)
    {
        Camera.main.GetComponent<MainScript>().SpawnBlock(spawnPoint.position, block.type + " " + block.blockName,
            Camera.main.GetComponent<MainScript>().blockPrefabs[block.type.ToString()],
            Camera.main.GetComponent<MainScript>().standartMaterial);
    
        GameObject newBlock = Camera.main.GetComponent<MainScript>().newBlock;
        newBlock.tag = "Selectable";
        Renderer renderer = newBlock.GetComponent<Renderer>();
    
        if(renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }
}

    // Вызывается, когда блок использован (кнопка Generate)
    public void OnBlockUsed(string blockName)
    {
        if(stepCompleted)
        return;
        
        if(remainingForCurrentStep.ContainsKey(blockName))
        {
            remainingForCurrentStep[blockName]--;
            
            if(remainingForCurrentStep[blockName] <= 0)
            remainingForCurrentStep.Remove(blockName);
        }
        
        if(remainingForCurrentStep.Count == 0)
        {
            stepCompleted = true;
            Debug.Log("Шаг выполнен! Можно переходить к следующей странице.");
        }
    }

    // Вызывается, когда блок удалён из инвентаря (без появления на сцене)
    public void OnBlockRemoved(string blockName, Color color, int count = 1)
    {
        if(currentStepPage == -1)
        return;
        
        Step step = currentLevel.steps.Find(s => s.pageNumber == currentStepPage);
        
        if(step == null)
        return;

        // Находим требуемый блок в шаге по имени и цвету
        RequiredBlock req = step.blocks.Find(b => b.block.blockName == blockName && b.color == color);

        if(req == null)
        return;

        for(int i = 0; i < count; i++)
        SpawnBlockAtSpawnPoint(req.block, req.color);
        
        // Увеличиваем оставшееся количество для этого блока
        if(remainingForCurrentStep.ContainsKey(blockName))
        remainingForCurrentStep[blockName] += count;
        
        else
        remainingForCurrentStep[blockName] = count;

        if(stepCompleted)
        stepCompleted = false;
}
}