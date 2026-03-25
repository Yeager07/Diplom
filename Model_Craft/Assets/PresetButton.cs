using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using UnityEngine.UI;

public class PresetButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float longPressDuration = 1.0f; // порог долгого нажатия в секундах
    private bool isPressed = false;
    private float pressStartTime;
    private Color myColor;
    public Image progressIndicator;
    // Колбэки
    private Action<Color> onClick;
    private Action<Color> onLongPress;
    private Coroutine longPressCoroutine;

    public void Initialize(Color color, Action<Color> clickCallback, Action<Color> longPressCallback)
    {
        myColor = color;
        onClick = clickCallback;
        onLongPress = longPressCallback;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        pressStartTime = Time.time;
        longPressCoroutine = StartCoroutine(LongPressProgress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed)
        return;

        // Останавливаем корутину прогресса
        if (longPressCoroutine != null)
        StopCoroutine(longPressCoroutine);

        float pressDuration = Time.time - pressStartTime;

        // Короткое нажатие — выбираем цвет
        if (pressDuration < longPressDuration)
        onClick?.Invoke(myColor);

        // Долгое нажатие — удаляем пресет
        else
        onLongPress?.Invoke(myColor);

        isPressed = false;

        // Сбрасываем индикатор, если он есть
        if (progressIndicator != null)
        progressIndicator.fillAmount = 1.0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPressed)
        {
            if (longPressCoroutine != null)
            StopCoroutine(longPressCoroutine);
            
            isPressed = false;

            // Сбрасываем индикатор
            if (progressIndicator != null)
            progressIndicator.fillAmount = 0.0f;
        }

        //Invoke(myColor);
    }

    private IEnumerator LongPressProgress()
    {
        float elapsed = 0.0f;

        while (elapsed < longPressDuration)
        {
            elapsed += Time.deltaTime;
            // Обновляем индикатор прогресса (если назначен)
            if (progressIndicator != null)
            progressIndicator.fillAmount = elapsed / longPressDuration;
            
            yield return null;
        }

        if (elapsed == longPressDuration)
        progressIndicator.fillAmount = 0.0f;
    }
}