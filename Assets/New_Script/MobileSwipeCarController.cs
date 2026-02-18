using UnityEngine;
using UnityEngine.InputSystem;

public class MobileSwipeCarController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Automatic forward speed on X-axis")]
    public float forwardSpeed = 25f;

    [Tooltip("How fast car switches lanes")]
    public float laneChangeSpeed = 12f;

    [Header("Lane Configuration")]
    [Tooltip("Distance between lanes on Z-axis")]
    public float laneWidth = 4f;

    [Header("Swipe Settings")]
    [Tooltip("Minimum swipe distance to register (in pixels)")]
    public float minSwipeDistance = 100f;

    [Tooltip("Maximum time for a valid swipe (in seconds)")]
    public float maxSwipeTime = 0.5f;

    [Header("Visual Feedback")]
    public bool showSwipeDebug = true;
    public GameObject leftArrowIndicator;
    public GameObject rightArrowIndicator;

    [Header("Audio (Optional)")]
    public AudioClip laneChangeSFX;

    // Lane system
    private int currentLane = 1; // 0=left, 1=center, 2=right
    private const int totalLanes = 3;
    private float targetZPosition = 0f;

    // Swipe detection
    private Vector2 swipeStartPos;
    private float swipeStartTime;
    private bool isSwiping = false;

    // Components
    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        InitializeComponents();
        VerifySetup();
        CalculateTargetLanePosition();

        if (showSwipeDebug)
        {
            Debug.Log("=== MOBILE SWIPE CONTROLLER READY ===");
            Debug.Log("Forward Speed: " + forwardSpeed);
            Debug.Log("Swipe Distance Threshold: " + minSwipeDistance);
        }
    }

    void InitializeComponents()
    {
        // Setup Rigidbody for smooth physics-based movement
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.mass = 1000f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Setup audio if available
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && laneChangeSFX != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Hide arrow indicators at start
        if (leftArrowIndicator != null) leftArrowIndicator.SetActive(false);
        if (rightArrowIndicator != null) rightArrowIndicator.SetActive(false);
    }

    void VerifySetup()
    {
        if (!CompareTag("Player"))
        {
            Debug.LogError("Car MUST be tagged as 'Player'!");
        }

        // Verify Input System is available
        if (Touchscreen.current == null)
        {
            Debug.LogWarning("No touchscreen detected. Testing in editor with mouse.");
        }
    }

    void Update()
    {
        HandleSwipeInput();

        // Keyboard fallback for testing in Unity Editor
#if UNITY_EDITOR
        HandleKeyboardInput();
#endif
    }

    void FixedUpdate()
    {
        MoveForwardAutomatically();
        SmoothLaneTransition();
    }

    void HandleSwipeInput()
    {
        // Check for touch or mouse input
        bool isPressed = false;
        Vector2 currentPosition = Vector2.zero;
        bool justReleased = false;

        // Mobile Touch Input (New Input System)
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            isPressed = touch.press.isPressed;

            if (isPressed)
            {
                currentPosition = touch.position.ReadValue();
            }

            justReleased = touch.press.wasReleasedThisFrame;
        }
        // Mouse fallback for Unity Editor testing
        else if (Mouse.current != null)
        {
            isPressed = Mouse.current.leftButton.isPressed;
            currentPosition = Mouse.current.position.ReadValue();
            justReleased = Mouse.current.leftButton.wasReleasedThisFrame;
        }

        // Swipe detection logic
        if (isPressed && !isSwiping)
        {
            // Start swipe
            StartSwipe(currentPosition);
        }
        else if (justReleased && isSwiping)
        {
            // End swipe
            EndSwipe(currentPosition);
        }
    }

    void StartSwipe(Vector2 position)
    {
        swipeStartPos = position;
        swipeStartTime = Time.time;
        isSwiping = true;

        if (showSwipeDebug)
        {
            Debug.Log("Swipe started at: " + position);
        }
    }

    void EndSwipe(Vector2 endPosition)
    {
        isSwiping = false;

        // Calculate swipe properties
        Vector2 swipeDelta = endPosition - swipeStartPos;
        float swipeDistance = swipeDelta.magnitude;
        float swipeTime = Time.time - swipeStartTime;

        if (showSwipeDebug)
        {
            Debug.Log("Swipe ended. Distance: " + swipeDistance + " Time: " + swipeTime);
        }

        // Validate swipe
        if (swipeDistance < minSwipeDistance)
        {
            if (showSwipeDebug)
            {
                Debug.Log("Swipe too short, ignored");
            }
            return;
        }

        if (swipeTime > maxSwipeTime)
        {
            if (showSwipeDebug)
            {
                Debug.Log("Swipe too slow, ignored");
            }
            return;
        }

        // Determine swipe direction (prioritize horizontal swipes)
        bool isHorizontalSwipe = Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y);

        if (isHorizontalSwipe)
        {
            if (swipeDelta.x > 0)
            {
                // Swipe RIGHT
                ChangeLane(1);
            }
            else
            {
                // Swipe LEFT
                ChangeLane(-1);
            }
        }
        else
        {
            if (showSwipeDebug)
            {
                Debug.Log("Vertical swipe detected, ignored");
            }
        }
    }

    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;

        // Validate lane bounds
        if (targetLane < 0 || targetLane >= totalLanes)
        {
            if (showSwipeDebug)
            {
                Debug.Log("Cannot move to lane " + targetLane + " (out of bounds)");
            }
            return;
        }

        // Update current lane
        currentLane = targetLane;
        CalculateTargetLanePosition();

        // Visual feedback
        ShowLaneChangeIndicator(direction);

        // Audio feedback
        PlayLaneChangeSFX();

        if (showSwipeDebug)
        {
            Debug.Log("Lane changed to: " + GetLaneName(currentLane));
        }
    }

    void CalculateTargetLanePosition()
    {
        // Center lane (1) = Z:0, Left lane (0) = Z:-laneWidth, Right lane (2) = Z:+laneWidth
        targetZPosition = (currentLane - 1) * laneWidth;
    }

    void MoveForwardAutomatically()
    {
        // Constant automatic forward movement on X-axis
        Vector3 forwardMovement = Vector3.right * forwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMovement);
    }

    void SmoothLaneTransition()
    {
        // Smoothly interpolate to target lane position
        float newZ = Mathf.Lerp(rb.position.z, targetZPosition, laneChangeSpeed * Time.fixedDeltaTime);
        Vector3 newPosition = new Vector3(rb.position.x, rb.position.y, newZ);
        rb.MovePosition(newPosition);
    }

    void ShowLaneChangeIndicator(int direction)
    {
        if (direction < 0 && leftArrowIndicator != null)
        {
            StartCoroutine(FlashIndicator(leftArrowIndicator));
        }
        else if (direction > 0 && rightArrowIndicator != null)
        {
            StartCoroutine(FlashIndicator(rightArrowIndicator));
        }
    }

    System.Collections.IEnumerator FlashIndicator(GameObject indicator)
    {
        indicator.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        indicator.SetActive(false);
    }

    void PlayLaneChangeSFX()
    {
        if (audioSource != null && laneChangeSFX != null)
        {
            audioSource.PlayOneShot(laneChangeSFX);
        }
    }

#if UNITY_EDITOR
    void HandleKeyboardInput()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            ChangeLane(-1);
        }

        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ChangeLane(1);
        }
    }
#endif

    string GetLaneName(int lane)
    {
        switch (lane)
        {
            case 0: return "LEFT";
            case 1: return "CENTER";
            case 2: return "RIGHT";
            default: return "UNKNOWN";
        }
    }

    // Public methods for external access
    public int GetCurrentLane()
    {
        return currentLane;
    }

    public float GetSpeed()
    {
        return forwardSpeed;
    }

    public void SetSpeed(float newSpeed)
    {
        forwardSpeed = Mathf.Max(0, newSpeed);
    }

    public float GetDistanceTraveled()
    {
        return transform.position.x;
    }

    // Visual debug in Scene view
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw current lane
        Gizmos.color = UnityEngine.Color.green;
        Vector3 lanePos = new Vector3(transform.position.x, transform.position.y, targetZPosition);
        Gizmos.DrawLine(lanePos + Vector3.up * 5, lanePos - Vector3.up * 5);

        // Draw all lanes
        Gizmos.color = UnityEngine.Color.yellow;
        for (int i = 0; i < totalLanes; i++)
        {
            float z = (i - 1) * laneWidth;
            Vector3 start = new Vector3(transform.position.x - 10, 0, z);
            Vector3 end = new Vector3(transform.position.x + 50, 0, z);
            Gizmos.DrawLine(start, end);
        }
    }
}