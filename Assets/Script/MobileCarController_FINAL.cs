using UnityEngine;

public class MobileCarController_FINAL : MonoBehaviour
{
    [Header("Speed")]
    public float maxSpeed = 20f;
    public float acceleration = 30f;
    public float steerStrength = 5f;

    [Header("Wheel Meshes")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;

    Rigidbody rb;
    float currentSpeed;
    bool isHolding;
    Vector2 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleInput();
        RotateWheels();
    }

    void FixedUpdate()
    {
        MoveForward();
    }

    void HandleInput()
    {
#if UNITY_EDITOR
        // MOUSE (Editor test)
        if (Input.GetMouseButtonDown(0))
        {
            isHolding = true;
            startPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            float deltaX = (Input.mousePosition.x - startPos.x) / Screen.width;
            Steer(deltaX);
        }
        else
        {
            isHolding = false;
        }
#else
        // TOUCH (Mobile)
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                isHolding = true;
                startPos = t.position;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                float deltaX = (t.position.x - startPos.x) / Screen.width;
                Steer(deltaX);
            }
            else if (t.phase == TouchPhase.Ended)
            {
                isHolding = false;
            }
        }
        else
        {
            isHolding = false;
        }
#endif
    }

    void MoveForward()
    {
        if (isHolding)
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.fixedDeltaTime * 2f);
        else
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.fixedDeltaTime * 3f);

        Vector3 move = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void Steer(float amount)
    {
        Vector3 sideMove = transform.right * amount * steerStrength;
        rb.MovePosition(rb.position + sideMove);
    }

    void RotateWheels()
    {
        float rot = currentSpeed * 50f * Time.deltaTime;
        wheelFL.Rotate(Vector3.right, rot);
        wheelFR.Rotate(Vector3.right, rot);
        wheelRL.Rotate(Vector3.right, rot);
        wheelRR.Rotate(Vector3.right, rot);
    }
}
