using UnityEngine;

public class MobileCarController_FINAL1 : MonoBehaviour
{
    [Header("Speed")]
    public float maxSpeed = 20f;

    [Header("Steering")]
    public float steerSpeed = 5f;        // How fast car moves sideways
    public float roadHalfWidth = 2.5f;   // VERY IMPORTANT (adjust to road)

    [Header("Front Wheel Turn")]
    public float wheelTurnAngle = 30f;

    [Header("Wheel Meshes")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;

    Rigidbody rb;
    float currentSpeed;
    bool isHolding;
    Vector2 startPos;

    Vector3 startPosition;
    Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        HandleInput();
        RotateWheels();
        RespawnCheck();
    }

    void FixedUpdate()
    {
        MoveForward();
    }

    // ---------------- INPUT ----------------
    void HandleInput()
    {
#if UNITY_EDITOR
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
            ResetFrontWheels();
        }
#else
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
            else
            {
                isHolding = false;
                ResetFrontWheels();
            }
        }
        else
        {
            isHolding = false;
            ResetFrontWheels();
        }
#endif
    }

    // ---------------- FORWARD MOVE ----------------
    void MoveForward()
    {
        if (isHolding)
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.fixedDeltaTime * 2f);
        else
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.fixedDeltaTime * 3f);

        Vector3 forwardMove = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);
    }

    // ---------------- STEERING (LOCKED TO ROAD) ----------------
    void Steer(float input)
    {
        input = Mathf.Clamp(input, -1f, 1f);

        float targetX =
            rb.position.x + input * steerSpeed * Time.fixedDeltaTime;

        // 🔒 HARD LIMIT (THIS IS THE MAIN FIX)
        targetX = Mathf.Clamp(targetX, -roadHalfWidth, roadHalfWidth);

        Vector3 targetPos =
            new Vector3(targetX, rb.position.y, rb.position.z);

        rb.MovePosition(targetPos);

        // Front wheel visual turn
        float angle = input * wheelTurnAngle;
        wheelFL.localRotation = Quaternion.Euler(0, angle, 0);
        wheelFR.localRotation = Quaternion.Euler(0, angle, 0);
    }

    void ResetFrontWheels()
    {
        wheelFL.localRotation = Quaternion.identity;
        wheelFR.localRotation = Quaternion.identity;
    }

    // ---------------- WHEEL ROTATION ----------------
    void RotateWheels()
    {
        float rot = currentSpeed * 50f * Time.deltaTime;
        wheelFL.Rotate(Vector3.right, rot);
        wheelFR.Rotate(Vector3.right, rot);
        wheelRL.Rotate(Vector3.right, rot);
        wheelRR.Rotate(Vector3.right, rot);
    }

    // ---------------- RESPAWN ----------------
    void RespawnCheck()
    {
        if (transform.position.y < -5f)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }
}
