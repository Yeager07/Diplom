using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public int zoneIndex; // 1-стол, 2-шкаф, 3-настройки, 4-выход

    private CameraMovement camMove;

    void Start()
    {
        camMove = Camera.main.GetComponent<CameraMovement>();
    }

    void OnMouseDown()
    {
        if (camMove != null)
            camMove.MoveToPoint(zoneIndex);
    }
}