using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelStepManager : MonoBehaviour
{
    public Transform spawnPoint;
    private PdfInstructionViewer pdfViewer;

    private LevelData currentLevel;
    private Dictionary<string, int> remainingForCurrentStep = new Dictionary<string, int>(); // blockName -> остаток
    private Dictionary<string, int> totalForCurrentStep = new Dictionary<string, int>();     // исходное количество в шаге
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
        
        foreach(GameObject block in allBlocks)
        {
            if(block.transform.position.x <= 800f)
            {
                Debug.Log($"Block in pickup zone: {block.name} at {block.transform.position}");
                return true;
            }
        }
        
        Debug.Log("No blocks in pickup zone");
        return false;
    }

    private void OnPageChanged(int pageNumber)
    {
        Debug.Log($"OnPageChanged: page={pageNumber}, currentStepPage={currentStepPage}, stepCompleted={stepCompleted}, remainingCount={remainingForCurrentStep.Count}");

        if(isSpawning)
        return;

        Step step = currentLevel.steps.Find(s => s.pageNumber == pageNumber);
        
        if(step == null)
        return;

        // Если мы уже на этой странице (текущий шаг)
        if(currentStepPage == pageNumber)
        {
            // Если шаг не завершён, но в зоне подбора нет блоков – значит, игрок удалил все блоки (например, через Delete)
            // Восстанавливаем только недостающие блоки (разницу между total и remaining)
            if(!stepCompleted && !AreThereBlocksInPickupZone())
            {
                foreach(var req in step.blocks)
                {
                    string fullName = req.block.type + " " + req.block.blockName;
                    int remaining = remainingForCurrentStep.ContainsKey(fullName) ? remainingForCurrentStep[req.block.type + " " + req.block.blockName] : 0;
                    int total = totalForCurrentStep[fullName];
                    int missing = total - remaining;
                    
                    for(int i = 0; i < missing; i++)
                    SpawnBlockAtSpawnPoint(req.block, req.color);
                    // После спавна оставшееся количество увеличивается на missing (так как эти блоки снова на столе)
                    remainingForCurrentStep[fullName] = total;
                }
            }
            return;
        }

        // Переход на новый шаг (другую страницу)
        // Проверяем, есть ли блоки в зоне подбора
        if(AreThereBlocksInPickupZone())
        {
            Debug.Log("В зоне подбора есть блоки. Подберите их или перенесите в зону сборки, чтобы перейти к следующему шагу.");
            return;
        }

        // Если предыдущий шаг не завершён – не разрешаем переход
        if(!stepCompleted && currentStepPage != -1)
        {
            Debug.Log("Сначала используйте все детали текущего шага!");
            return;
        }

        // Генерируем новый шаг
        StartCoroutine(SpawnStepWithDelay(step, pageNumber));
    }

    private IEnumerator SpawnStepWithDelay(Step step, int pageNumber)
    {
        isSpawning = true;
        yield return new WaitForSeconds(0.2f);
        SpawnStep(step);
        currentStepPage = pageNumber;
        stepCompleted = false;
        Debug.Log($"SpawnStepWithDelay: currentStepPage set to {currentStepPage}, stepCompleted=false");
        isSpawning = false;
    }

    private void SpawnStep(Step step)
    {
        remainingForCurrentStep.Clear();
        totalForCurrentStep.Clear();
        
        foreach(RequiredBlock req in step.blocks)
        {
            string fullName = req.block.type + " " + req.block.blockName;
            
            for(int i = 0; i < req.count; i++)
            SpawnBlockAtSpawnPoint(req.block, req.color);

            remainingForCurrentStep[fullName] = req.count;
            totalForCurrentStep[fullName] = req.count;
        }
    }

    private void SpawnBlockAtSpawnPoint(BlockData block, Color color)
    {
        Debug.Log($"Spawning {block.type} {block.blockName} at {spawnPoint.position}");

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

        Debug.Log("Block instantiated at " + spawnPoint.position);
    }

    // Вызывается, когда блок использован (кнопка Generate)
    public void OnBlockUsed(string blockName)
    {
        Debug.Log($"OnBlockUsed called for {blockName}, stepCompleted={stepCompleted}, remaining before: {string.Join(",", remainingForCurrentStep.Select(kv=>kv.Key+":"+kv.Value))}");

        if(stepCompleted)
        return;
        
        if(remainingForCurrentStep.ContainsKey(blockName))
        {
            remainingForCurrentStep[blockName]--;
            
            if(remainingForCurrentStep[blockName] <= 0)
            remainingForCurrentStep.Remove(blockName);
        }

        Debug.Log($"remaining after: {string.Join(",", remainingForCurrentStep.Select(kv=>kv.Key+":"+kv.Value))}");
        
        if(remainingForCurrentStep.Count == 0)
        {
            stepCompleted = true;
            Debug.Log("Шаг выполнен! Можно переходить к следующей странице.");
        }
    }

    public bool IsBlockColorNeeded(string blockName, Color color)
    {
        Debug.Log($"IsBlockColorNeeded: {blockName}, color={color}, currentStepPage={currentStepPage}");
        
        if(currentStepPage == -1)
        return false;
        
        Step step = currentLevel.steps.Find(s => s.pageNumber == currentStepPage);
        
        if(step == null)
        return false;
        
        foreach(var req in step.blocks)
        {
            string fullName = req.block.type + " " + req.block.blockName;
            
            if(fullName == blockName)
            return true;
        }
        return false;
    }

    // Вызывается, когда блок удалён из инвентаря (без появления на сцене)
    public void OnBlockRemoved(string blockName, Color color, int count = 1)
    {
        Debug.Log($"OnBlockRemoved called: {blockName}, color={color}, count={count}");

        if(currentStepPage == -1)
        return;
        
        Step step = currentLevel.steps.Find(s => s.pageNumber == currentStepPage);
        
        if(step == null)
        {
            Debug.Log("Step not found!");
            return;
        }

        RequiredBlock req = step.blocks.Find(b => (b.block.type + " " + b.block.blockName) == blockName && b.color == color);
        
        if(req == null)
        {
            Debug.Log("RequiredBlock not found!");
            return;
        }

        Debug.Log("Spawning block at spawn point...");
        
        for(int i = 0; i < count; i++)
        SpawnBlockAtSpawnPoint(req.block, req.color);

        /*if(remainingForCurrentStep.ContainsKey(blockName))
        remainingForCurrentStep[blockName] += count;
        
        else
        remainingForCurrentStep[blockName] = count;*/

        if(stepCompleted)
        stepCompleted = false;
    }
}