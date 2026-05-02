using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class LongPressDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float longPressDuration = 1.0f;
    public UnityEvent onShortPress;
    public UnityEvent onLongPress;

    private float pressTime;
    private bool isPressed = false;
    private bool longPressTriggered = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        pressTime = Time.time;
        isPressed = true;
        longPressTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!isPressed) return;
        isPressed = false;

        if(!longPressTriggered)
        onShortPress?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }

    void Update()
    {
        if(isPressed && !longPressTriggered && Time.time - pressTime >= longPressDuration)
        {
            longPressTriggered = true;
            onLongPress?.Invoke();
        }
    }
}