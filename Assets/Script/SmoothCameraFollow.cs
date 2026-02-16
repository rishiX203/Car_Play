using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;          // Car Transform
    public Vector3 offset = new Vector3(0f, 5f, -8f);  // Camera position offset
    public float smoothSpeed = 5f;    // Smoothness value
    public float rotationSpeed = 5f;  // Rotation smoothness

    void LateUpdate()
    {
        if (target == null)
            return;

        // Desired position
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Smooth position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Smooth rotation
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }
}
