using UnityEngine;

public class MobileCarController : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float brakeSpeed = 5f;
    public float turnSpeed = 120f;
    public float maxSpeed = 20f;

    private Rigidbody rb;
    private float turnInput = 0f;
    private bool isBraking = false;

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
    }

    void FixedUpdate()
    {
        MoveCar();
        TurnCar();
        LimitSpeed();
    }

    void MoveCar()
    {
        float currentSpeed = isBraking ? brakeSpeed : moveSpeed;

        Vector3 forwardMove = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);
    }

    void TurnCar()
    {
        if (turnInput != 0)
        {
            Quaternion turnRotation = Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    void HandleTouchInput()
    {
        turnInput = 0f;
        isBraking = false;

        if (Input.touchCount > 0)
        {
            // 2 finger touch = Brake
            if (Input.touchCount >= 2)
            {
                isBraking = true;
            }

            Touch touch = Input.GetTouch(0);

            if (touch.position.x < Screen.width / 2)
                turnInput = -1f;
            else
                turnInput = 1f;
        }

#if UNITY_EDITOR
        // Mouse Left = Steering
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x < Screen.width / 2)
                turnInput = -1f;
            else
                turnInput = 1f;
        }

        // Mouse Right = Brake
        if (Input.GetMouseButton(1))
        {
            isBraking = true;
        }
#endif
    }

    void LimitSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
