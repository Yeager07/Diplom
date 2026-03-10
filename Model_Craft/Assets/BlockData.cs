using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Lego/Block Data")]
public class BlockData : ScriptableObject
{
    public string blockName;        // Название блока
    public Sprite icon;             // Иконка для карточки
    public GameObject prefab;       // Префаб самой детали (если нужно)
    public BlockType type;          // Тип блока (например, "Кубики", "Пластины", "Спецдетали")
}

// Enum для типов блоков (можно расширять)
public enum BlockType
{
    Brick,      // Обычные кубики
    Plate,      // Пластины
    Tile,      // Гладкие пластины
    Slice,    // Срезанные
    Special,    // Специальные блоки
    Arch,    //Арки
    Panel,    //Панели
    Cylinders,    //Цилиндры и конусы
    RoundPlate    //Круглые пластины
}
