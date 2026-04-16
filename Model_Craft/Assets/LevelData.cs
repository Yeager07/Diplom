using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Lego/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public string instructionFileName;
    public List<Step> steps;
}

[System.Serializable]
public class Step
{
    public int pageNumber;
    public List<RequiredBlock> blocks;
}

[System.Serializable]
public class RequiredBlock
{
    public BlockData block;
    public Color color;
    public int count;
}