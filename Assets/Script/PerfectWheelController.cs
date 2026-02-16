using UnityEngine;

public class PerfectWheelController : MonoBehaviour
{
    [Header("Wheel Meshes")]
    public Transform frontLeft;
    public Transform frontRight;
    public Transform rearLeft;
    public Transform rearRight;

    [Header("Car Rigidbody")]
    public Rigidbody carRb;

    [Header("Settings")]
    public float wheelRadius = 0.35f;      // Adjust if needed
    public float maxSteerAngle = 30f;
    public float steerSmooth = 8f;

    float steerAngle;
    float rotationAngle;

    void Update()
    {
        if (carRb == null) return;

        HandleWheelSpin();
        HandleSteering();
        ApplyVisuals();
    }

    void HandleWheelSpin()
    {
        // Forward speed only (correct direction based spin)
        float forwardSpeed = Vector3.Dot(carRb.linearVelocity, transform.forward);

        // Convert speed to rotation using wheel radius
        float rotationSpeed = (forwardSpeed / (2f * Mathf.PI * wheelRadius)) * 360f;

        rotationAngle += rotationSpeed * Time.deltaTime;
    }

    void HandleSteering()
    {
        float input = Input.GetAxis("Horizontal");
        float targetSteer = input * maxSteerAngle;

        steerAngle = Mathf.Lerp(steerAngle, targetSteer, steerSmooth * Time.deltaTime);
    }

    void ApplyVisuals()
    {
        // Spin around local X axis
        Quaternion spin = Quaternion.Euler(rotationAngle, 0f, 0f);

        // Front wheels (Spin + Steer)
        frontLeft.localRotation =
            Quaternion.Euler(0f, steerAngle, 0f) * spin;

        frontRight.localRotation =
            Quaternion.Euler(0f, steerAngle, 0f) * spin;

        // Rear wheels (Spin only)
        rearLeft.localRotation = spin;
        rearRight.localRotation = spin;
    }
}
