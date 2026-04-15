using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Lego/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public string instructionFileName;
    public List<RequiredBlock> requiredBlocks;
}

[System.Serializable]
public class RequiredBlock
{
    public BlockData block;
    public Color color;
    public int count;
}