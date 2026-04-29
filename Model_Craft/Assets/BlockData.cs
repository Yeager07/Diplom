using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Lego/Block Data")]
public class BlockData : ScriptableObject
{
    public string blockName;
    public Sprite icon;
    public GameObject prefab;
    public BlockType type;
    public string blockID;
}

public enum BlockType
{
    Brick,      // Обычные кубики
    Plate,      // Пластины
    Tile,       // Гладкие пластины
    Slice,      // Срезанные
    Special,    // Специальные блоки
    Arch,       //Арки
    Panel,      //Панели
    Cylinders,  //Цилиндры и конусы
    RoundPlate  //Круглые пластины
}
