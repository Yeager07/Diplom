using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelStepManager : MonoBehaviour
{
    public Transform spawnPoint;
    private PdfInstructionViewer pdfViewer;

    private LevelData currentLevel;
    private Dictionary<string, int> remainingForCurrentStep = new Dictionary<string, int>();
    private int currentStepPage = -1;
    private bool stepCompleted = true;
    private bool isSpawning = false;

    private HashSet<int> completedSteps = new HashSet<int>();
    private bool isRestoring = false;

    public static bool IsLoadingSave = false;

    void Start()
    {
        if(IsLoadingSave)
        {
            Debug.Log("LevelStepManager: Loading save, skipping initial page setup.");
            currentLevel = LevelLoader.SelectedLevel;
            return;
        }

        Initialize(LevelLoader.SelectedLevel);
    }

    void OnDestroy()
    {
        if(pdfViewer != null)
        pdfViewer.OnPageChanged -= OnPageChanged;
    }

    public void SetCurrentLevel(LevelData level)
    {
        currentLevel = level;

        if(pdfViewer == null)
        pdfViewer = FindFirstObjectByType<PdfInstructionViewer>();
    
        if(pdfViewer != null && !IsLoadingSave)
        {
            pdfViewer.OnPageChanged -= OnPageChanged;
            pdfViewer.OnPageChanged += OnPageChanged;
        }
    }

    public void Initialize(LevelData level)
    {
        currentLevel = level;
        pdfViewer = GameObject.FindGameObjectWithTag("Player").transform.Find("PdfViewer").GetComponent<PdfInstructionViewer>();
     
        if(pdfViewer == null)
        pdfViewer = FindFirstObjectByType<PdfInstructionViewer>();
     
        if(pdfViewer != null)
        {
            if(!IsLoadingSave)
            pdfViewer.ResetToFirstPage();
            
            
            if(!IsLoadingSave)
            {
                pdfViewer.OnPageChanged += OnPageChanged;
                StartCoroutine(WaitForFirstPage());
            }
        }
    }

    private IEnumerator WaitForFirstPage()
    {
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

        if(isSpawning || isRestoring)
        {
            Debug.Log("  blocked by isSpawning or isRestoring");
            return;
        }

        Step step = currentLevel.steps.Find(s => s.pageNumber == pageNumber);
        
        if(step == null)
        {
            Debug.Log("  step is null");
            return;
        }

        if(completedSteps.Contains(pageNumber))
        {
            Debug.Log("  step already completed, skipping");
            currentStepPage = pageNumber;
            stepCompleted = true;
            return;
        }

        if(currentStepPage == pageNumber)            
        {
            Debug.Log("  already on this step, returning");
            return;
        }
        
        Debug.Log("  checking conditions for moving to new step...");
        
        if(AreThereBlocksInPickupZone())
        {
            Debug.Log("  blocks in pickup zone, abort");
            return;
        }

        if(!stepCompleted && currentStepPage != -1)
        {
            Debug.Log("  previous step not completed, abort");
            return;
        }

        if(step.blocks.Count == 0)
        {
            Debug.Log("  step has no blocks, marking complete");
            currentStepPage = pageNumber;
            stepCompleted = true;
            completedSteps.Add(pageNumber);
            return;
        }

        Debug.Log("  starting SpawnStepWithDelay");
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
        Debug.Log("  spawnStep assign");

        remainingForCurrentStep.Clear();
        
        foreach(RequiredBlock req in step.blocks)
        {
            string fullName = req.block.type + " " + req.block.blockName;
            
            for(int i = 0; i < req.count; i++)
            SpawnBlockAtSpawnPoint(req.block, req.color);

            remainingForCurrentStep[fullName] = req.count;
        }
    }

    private void SpawnBlockAtSpawnPoint(BlockData block, Color color)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint is null!");
            return;
        }

        Debug.Log($"Spawning {block.type} {block.blockName} at {spawnPoint.position}");

        Camera.main.GetComponent<MainScript>().SpawnBlock(spawnPoint.position, block.type + " " + block.blockName,
            Camera.main.GetComponent<MainScript>().blockPrefabs[block.type.ToString()],
            Camera.main.GetComponent<MainScript>().standartMaterial, new Vector3(0.0f, 0.0f, 0.0f));

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

        if(remainingForCurrentStep.Count == 0)
        {
            stepCompleted = true;
            Debug.Log("Шаг выполнен! Можно переходить к следующей странице.");
            completedSteps.Add(currentStepPage);
            CheckAllStepsCompleted();
        }
    }

    private void CheckAllStepsCompleted()
    {
        if(completedSteps.Count == currentLevel.steps.Count)
        {
            List<BlockSaveData> rootBlocks = SaveManager.Instance.CollectRootBlocks();
            SaveManager.Instance.SaveCareerModelToGallery(currentLevel.levelName, rootBlocks);
            Debug.Log($"Модель для уровня {currentLevel.levelName} сохранена в галерею!");
        }
        else
        {
            Debug.Log("Not all steps completed yet.");
        }
    }

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

        if(remainingForCurrentStep.ContainsKey(blockName))
        remainingForCurrentStep[blockName] += count;

        else
        remainingForCurrentStep[blockName] = count;

        if(stepCompleted)
        stepCompleted = false;
    }

    public void SpawnMissingBlocksForCurrentStep()
    {
        if(currentLevel == null)
        {
            Debug.LogWarning("currentLevel not initialized yet, skipping spawn.");
            return;
        }

        if(currentStepPage == -1)
        return;
        
        Step step = currentLevel.steps.Find(s => s.pageNumber == currentStepPage);
        
        if(step == null)
        return;
        
        Debug.Log($"Spawning missing blocks for step {currentStepPage}");

        foreach(RequiredBlock req in step.blocks)
        {
            string fullName = req.block.type + " " + req.block.blockName;
            int remaining = remainingForCurrentStep.ContainsKey(fullName) ? remainingForCurrentStep[fullName] : 0;
            
            Debug.Log($"  {fullName}: remaining (from dict) = {remaining}, total required = {req.count}");

            for(int i = 0; i < remaining; i++)
            SpawnBlockAtSpawnPoint(req.block, req.color);
        }
    }

    public List<RequiredBlockSaveData> GetRemainingForStep()
    {
        List<RequiredBlockSaveData> result = new List<RequiredBlockSaveData>();

        if(currentStepPage == -1)
        return result;
        
        Step step = currentLevel.steps.Find(s => s.pageNumber == currentStepPage);
        
        if(step == null)
        return result;

        foreach(RequiredBlock req in step.blocks)
        {
            string fullName = req.block.type + " " + req.block.blockName;
            int remaining = remainingForCurrentStep.ContainsKey(fullName) ? remainingForCurrentStep[fullName] : 0;
            
            if(remaining > 0)
            {
                result.Add(new RequiredBlockSaveData
                { blockFullName = fullName,
                color = req.color,
                remaining = remaining});
            }
        }
        
        return result;
    }

    public List<int> GetCompletedSteps() => new List<int>(completedSteps);

    public void RestoreState(int stepPage, List<int> completed, List<RequiredBlockSaveData> remaining)
    {
        isRestoring = true;
        currentStepPage = stepPage;

        if(pdfViewer != null)
        pdfViewer.GoToPage(stepPage - 1);

        completedSteps = new HashSet<int>(completed);
        remainingForCurrentStep.Clear();
     
        foreach(var r in remaining)
        remainingForCurrentStep[r.blockFullName] = r.remaining;
        
        stepCompleted = (remainingForCurrentStep.Count == 0);

        if(pdfViewer == null)
        pdfViewer = FindFirstObjectByType<PdfInstructionViewer>();
        
        if(pdfViewer != null)
        {
            pdfViewer.OnPageChanged -= OnPageChanged;
            pdfViewer.OnPageChanged += OnPageChanged;
        }

        isRestoring = false;
    }

    public void SetRestoring(bool restoring)
    {
        isRestoring = restoring;
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
}