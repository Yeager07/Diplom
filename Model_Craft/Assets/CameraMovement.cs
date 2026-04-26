using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public Transform[] cameraPoints; // [0] центр, [1] стол, [2] шкаф, [3] настройки, [4] выход
    public float moveSpeed = 5f;
    public float rotationSpeed = 3f;

    private int currentTargetIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        if (cameraPoints.Length > 0)
        {
            transform.position = cameraPoints[0].position;
            transform.rotation = cameraPoints[0].rotation;
            currentTargetIndex = 0;
        }
    }

    public void MoveToPoint(int index)
    {
        if (index < 0 || index >= cameraPoints.Length) return;
        if (isMoving) StopAllCoroutines();
        StartCoroutine(SmoothMove(index));
    }

    IEnumerator SmoothMove(int targetIndex)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = cameraPoints[targetIndex].position;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = cameraPoints[targetIndex].rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t * rotationSpeed);
            yield return null;
        }
        transform.position = targetPos;
        transform.rotation = targetRot;
        isMoving = false;
        currentTargetIndex = targetIndex;

        // Уведомляем ZoneManager
        ZoneManager.Instance.OnCameraArrived(targetIndex);
    }
}