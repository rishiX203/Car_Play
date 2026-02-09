using UnityEngine;

public class SmoothCarCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;   // Car root

    [Header("Offset Settings")]
    public Vector3 offset = new Vector3(0, 6, -10);

    [Header("Smooth Settings")]
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (!target) return;

        // Desired position
        Vector3 desiredPos = target.position + offset;

        // Smooth follow
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );

        // Camera rotate aagakoodadhu
        transform.LookAt(target);
    }
}
