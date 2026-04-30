using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CareerModeLoader : MonoBehaviour
{
    void Start()
    {
        if(ZoneManager.PendingCareerSave != null)
        {
            LevelStepManager.IsLoadingSave = true;
            StartCoroutine(RestoreCareerSaveCoroutine(ZoneManager.PendingCareerSave));
            ZoneManager.PendingCareerSave = null;
        }
    }

    private IEnumerator RestoreCareerSaveCoroutine(CareerSaveData data)
    {
        yield return null;

        ClearAllBlocks();
        SaveManager.Instance.SpawnFromSaveData(data.rootBlocks, null);
        
        LevelStepManager stepManager = FindFirstObjectByType<LevelStepManager>();
        
        if(stepManager != null)
        {
            ClearBlocksInSpawnZone(stepManager.spawnPoint);
            stepManager.SetCurrentLevel(LevelLoader.SelectedLevel);
            stepManager.RestoreState(data.currentStepPage, data.completedSteps, data.remainingBlocks);
            stepManager.SpawnMissingBlocksForCurrentStep();
        }

        yield return StartCoroutine(DelayedGoToPageAndRelease(data.currentStepPage - 1, stepManager));
        LevelStepManager.IsLoadingSave = false;
    }

    private IEnumerator DelayedGoToPageAndRelease(int pageIndex, LevelStepManager stepManager)
    {
        yield return null;
     
        PdfInstructionViewer pdf = FindFirstObjectByType<PdfInstructionViewer>();
     
        if(pdf != null)
        pdf.GoToPage(pageIndex);

        yield return null;
    }

    private void ClearBlocksInSpawnZone(Transform spawnPoint)
    {
        if(spawnPoint == null)
        {
            Debug.LogWarning("spawnPoint is null, cannot clear blocks in spawn zone.");
            return;
        }
        
        float radius = 50.0f;
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Selectable");
        Debug.Log($"ClearBlocksInSpawnZone: found {blocks.Length} blocks, spawnPoint at {spawnPoint.position}");

        foreach(GameObject block in blocks)
        {
            float dist = Vector3.Distance(block.transform.position, spawnPoint.position);
            
            if(dist <= radius)
            {
                Debug.Log($"Destroying {block.name} at distance {dist}");
                Destroy(block);
            }
        }
    }

    private void ClearAllBlocks()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Selectable");
        
        foreach(GameObject b in blocks)
        {
            if(b.CompareTag("SpawnPoint"))
            continue;

            Destroy(b);
        }
        
        Block.connections.Clear();
    }
}
