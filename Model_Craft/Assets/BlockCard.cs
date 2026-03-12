using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BlockCard : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;

    private BlockData blockData;
    private System.Action<BlockData> onClickCallback;

    void Awake()
    {
        // Подписываемся на клик кнопки (если компонент Button есть)
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnCardClick);
        }
    }

    public void Setup(BlockData data, System.Action<BlockData> onClick)
    {
        blockData = data;
        onClickCallback = onClick;
        Debug.Log($"BlockCard.Setup: {data.blockName}, callback is { (onClick == null ? "NULL" : "OK") }");
        // Заполняем UI
        if(iconImage != null)
        iconImage.sprite = data.icon;
        
        if(nameText != null)
        nameText.text = data.blockName;
    }

    public void OnMouseEnter()
    {
        gameObject.GetComponent<Image>().material = Camera.main.GetComponent<MainScript>().outlineMaterial;
    }

    public void OnMouseExit()
    {
        gameObject.GetComponent<Image>().material = null;
    }

    // Вызывается кнопкой
    public void OnCardClick()
    {
        Debug.Log($"BlockCard.OnCardClick: {blockData?.blockName}, callback is { (onClickCallback == null ? "NULL" : "OK") }");
        onClickCallback?.Invoke(blockData);
    }
}
