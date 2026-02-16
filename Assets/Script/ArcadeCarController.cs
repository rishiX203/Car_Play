using UnityEngine;

public class ArcadeCarController : MonoBehaviour
{
    [Header("Car Movement")]
    public float moveSpeed = 15f;
    public float brakeSpeed = 5f;
    public float turnSpeed = 120f;
    public float maxSpeed = 20f;

    [Header("Wheel Meshes")]
    public Transform frontLeft;
    public Transform frontRight;
    public Transform rearLeft;
    public Transform rearRight;

    [Header("Wheel Settings")]
    public float wheelRadius = 0.35f;
    public float maxSteerAngle = 30f;
    public float steerSmooth = 8f;

    Rigidbody rb;
    float turnInput = 0f;
    bool isBraking = false;

    float steerAngle;
    float rotationAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        HandleTouchInput();
        HandleWheelVisuals();
    }

    void FixedUpdate()
    {
        MoveCar();
        TurnCar();
        LimitSpeed();
    }

    // ---------------- CAR MOVEMENT ----------------

    void MoveCar()
    {
        float currentSpeed = isBraking ? brakeSpeed : moveSpeed;

        Vector3 velocity = transform.forward * currentSpeed;

        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y,
            velocity.z
        );
    }

    void TurnCar()
    {
        if (turnInput != 0)
        {
            Quaternion turnRotation =
                Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);

            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    void LimitSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
        }
    }

    // ---------------- INPUT ----------------

    void HandleTouchInput()
    {
        turnInput = 0f;
        isBraking = false;

        if (Input.touchCount > 0)
        {
            if (Input.touchCount >= 2)
                isBraking = true;

            Touch touch = Input.GetTouch(0);

            if (touch.position.x < Screen.width / 2)
                turnInput = -1f;
            else
                turnInput = 1f;
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x < Screen.width / 2)
                turnInput = -1f;
            else
                turnInput = 1f;
        }

        if (Input.GetMouseButton(1))
        {
            isBraking = true;
        }
#endif
    }

    // ---------------- WHEEL VISUALS ----------------

    void HandleWheelVisuals()
    {
        // Forward speed based rotation
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        float rotationSpeed =
            (forwardSpeed / (2f * Mathf.PI * wheelRadius)) * 360f;

        rotationAngle += rotationSpeed * Time.deltaTime;

        float targetSteer = turnInput * maxSteerAngle;
        steerAngle = Mathf.Lerp(steerAngle, targetSteer, steerSmooth * Time.deltaTime);

        Quaternion spin = Quaternion.Euler(rotationAngle, 0f, 0f);

        // Front wheels (spin + steer)
        if (frontLeft != null)
            frontLeft.localRotation =
                Quaternion.Euler(0f, steerAngle, 0f) * spin;

        if (frontRight != null)
            frontRight.localRotation =
                Quaternion.Euler(0f, steerAngle, 0f) * spin;

        // Rear wheels (spin only)
        if (rearLeft != null)
            rearLeft.localRotation = spin;

        if (rearRight != null)
            rearRight.localRotation = spin;
    }
}
