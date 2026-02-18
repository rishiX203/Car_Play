using UnityEngine;

public class EndlessRunnerCarController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the car moves forward (X-axis)")]
    public float forwardSpeed = 25f;

    [Tooltip("How fast car switches between lanes")]
    public float laneChangeSpeed = 8f;

    [Header("Lane System")]
    [Tooltip("Width between each lane on Z-axis")]
    public float laneWidth = 4f;

    [Tooltip("Total number of lanes (3 = left, center, right)")]
    public int totalLanes = 3;

    [Header("Input Settings")]
    [Tooltip("Use swipe controls for mobile")]
    public bool useMobileInput = true;

    [Tooltip("Minimum swipe distance to register")]
    public float minSwipeDistance = 50f;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Private variables
    private int currentLane = 1; // Start in center lane (0 is left, 1 is center, 2 is right)
    private float targetZPosition = 0f;
    private Rigidbody rb;

    // Mobile input tracking
    private Vector2 touchStartPos;
    private bool isSwiping = false;

    void Start()
    {
        // Get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning("Rigidbody was missing - added automatically");
        }

        // Configure Rigidbody
        rb.useGravity = true;
        rb.mass = 1000;
        rb.linearDamping = 0;
        rb.angularDamping = 0.05f;

        // Freeze rotations to prevent flipping
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezeRotationZ;

        // Calculate starting lane position
        CalculateTargetPosition();

        // Verify car is tagged
        if (!gameObject.CompareTag("Player"))
        {
            Debug.LogError("Car MUST be tagged as 'Player'! Please tag this GameObject.");
        }

        if (showDebugInfo)
        {
            Debug.Log("Car Controller initialized. Starting lane: " + currentLane + ", Speed: " + forwardSpeed);
        }
    }

    void Update()
    {
        HandleInput();

        if (showDebugInfo)
        {
            DebugDisplay();
        }
    }

    void FixedUpdate()
    {
        MoveForward();
        SmoothLaneChange();
    }

    void HandleInput()
    {
        // Keyboard and PC Input
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
        }

        // Mobile Touch and Swipe Input
        if (useMobileInput && Application.isMobilePlatform)
        {
            HandleMobileInput();
        }
        // Fallback: Simple touch for testing in Editor
        else if (useMobileInput && Input.touchCount > 0)
        {
            HandleMobileInput();
        }
    }

    void HandleMobileInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Ended:
                    if (isSwiping)
                    {
                        Vector2 swipeDelta = touch.position - touchStartPos;

                        // Detect swipe direction
                        if (swipeDelta.magnitude > minSwipeDistance)
                        {
                            // Horizontal swipe (left or right lane change)
                            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                            {
                                if (swipeDelta.x > 0)
                                {
                                    ChangeLane(1); // Swipe right
                                }
                                else
                                {
                                    ChangeLane(-1); // Swipe left
                                }
                            }
                        }
                    }
                    isSwiping = false;
                    break;

                case TouchPhase.Canceled:
                    isSwiping = false;
                    break;
            }
        }
    }

    void ChangeLane(int direction)
    {
        // Update lane index
        currentLane += direction;

        // Clamp to valid lane range
        currentLane = Mathf.Clamp(currentLane, 0, totalLanes - 1);

        // Calculate new target position
        CalculateTargetPosition();

        if (showDebugInfo)
        {
            Debug.Log("Lane changed to: " + currentLane + " (Target Z: " + targetZPosition + ")");
        }
    }

    void CalculateTargetPosition()
    {
        // Calculate Z position based on lane
        // Center lane (1) = Z:0, Left lane (0) = Z:-laneWidth, Right lane (2) = Z:+laneWidth
        int centerLane = totalLanes / 2;
        targetZPosition = (currentLane - centerLane) * laneWidth;
    }

    void MoveForward()
    {
        // Constant forward movement on X-axis
        Vector3 forwardMovement = Vector3.right * forwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMovement);
    }

    void SmoothLaneChange()
    {
        // Smoothly interpolate to target Z position
        float newZ = Mathf.Lerp(transform.position.z, targetZPosition, laneChangeSpeed * Time.fixedDeltaTime);

        // Create new position maintaining X and Y
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, newZ);
        rb.MovePosition(newPosition);
    }

    void DebugDisplay()
    {
        // Display current state in Scene view
        Debug.DrawRay(transform.position, Vector3.right * 5f, Color.green); // Forward direction
        Debug.DrawRay(transform.position, Vector3.up * 2f, Color.blue); // Up direction

        // Draw target lane position
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, targetZPosition);
        Debug.DrawLine(transform.position, targetPos, Color.yellow);
    }

    // Public method to increase speed (for difficulty progression)
    public void IncreaseSpeed(float amount)
    {
        forwardSpeed += amount;
        if (showDebugInfo)
        {
            Debug.Log("Speed increased to: " + forwardSpeed);
        }
    }

    // Public method to get current speed
    public float GetCurrentSpeed()
    {
        return forwardSpeed;
    }

    // Public method to get distance traveled
    public float GetDistanceTraveled()
    {
        return transform.position.x;
    }
}