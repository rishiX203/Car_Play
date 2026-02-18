using UnityEngine;
using UnityEngine.InputSystem; // ADD THIS

public class SimpleEndlessCarController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float moveSpeed = 20f;
    public float laneChangeSpeed = 15f;

    [Header("Lane Settings")]
    public float laneWidth = 4f;
    public int totalLanes = 3;

    [Header("Debug")]
    public bool showDebug = true;

    private int currentLane = 1;
    private float targetZ = 0f;
    private bool isInitialized = false;

    void Start()
    {
        if (!CompareTag("Player"))
        {
            Debug.LogError("ERROR: Car must be tagged 'Player'!");
            return;
        }

        targetZ = transform.position.z;
        isInitialized = true;

        if (showDebug)
        {
            Debug.Log("=== CAR CONTROLLER STARTED ===");
            Debug.Log("Speed: " + moveSpeed);
            Debug.Log("Current Position: " + transform.position);
            Debug.Log("Current Lane: " + currentLane);
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        MoveForward();
        HandleInput();
        SmoothLaneTransition();

        if (showDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log("Car X position: " + transform.position.x.ToString("F1"));
        }
    }

    void MoveForward()
    {
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
    }

    void HandleInput()
    {
        // NEW INPUT SYSTEM - Keyboard
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                MoveLane(-1);
            }

            if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                MoveLane(1);
            }
        }

        // NEW INPUT SYSTEM - Touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
                if (touchPos.x < Screen.width * 0.5f)
                {
                    MoveLane(-1);
                }
                else
                {
                    MoveLane(1);
                }
            }
        }
    }

    void MoveLane(int direction)
    {
        currentLane += direction;
        currentLane = Mathf.Clamp(currentLane, 0, totalLanes - 1);

        int centerLane = totalLanes / 2;
        targetZ = (currentLane - centerLane) * laneWidth;

        if (showDebug)
        {
            Debug.Log("Lane: " + currentLane + " | Target Z: " + targetZ);
        }
    }

    void SmoothLaneTransition()
    {
        if (Mathf.Abs(transform.position.z - targetZ) > 0.01f)
        {
            float newZ = Mathf.Lerp(transform.position.z, targetZ, laneChangeSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
        }
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }

    public float GetDistance()
    {
        return transform.position.x;
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        if (showDebug)
        {
            Debug.Log("Speed changed to: " + moveSpeed);
        }
    }
}