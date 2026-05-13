using UnityEngine;
using UnityEngine.UI;

public class UIThemeApplier : MonoBehaviour
{
    private void OnEnable()
    {
        if(UISkinManager.Instance != null)
        UISkinManager.Instance.ApplyToGameObject(gameObject);
    }
}