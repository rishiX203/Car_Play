using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AsphaltDriftController : MonoBehaviour
{
    [Header("Drift Settings")]
    public float minDriftSpeed = 15f;
    public float driftSidewaysForce = 8f;
    public float tractionControl = 4f;
    public float driftSteerBoost = 1.3f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        HandleDrift();
    }

    void HandleDrift()
    {
        float speed = rb.linearVelocity.magnitude;
        float input = Input.GetAxis("Horizontal");

        if (speed > minDriftSpeed && Mathf.Abs(input) > 0.6f)
        {
            // Calculate sideways velocity
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            // Reduce traction (make rear slide)
            localVelocity.x += input * driftSidewaysForce * Time.fixedDeltaTime;

            // Apply modified velocity
            rb.linearVelocity = transform.TransformDirection(localVelocity);

            // Extra steering feel boost
            rb.MoveRotation(rb.rotation *
                Quaternion.Euler(0f, input * driftSteerBoost * 50f * Time.fixedDeltaTime, 0f));
        }
        else
        {
            // Traction recovery
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, tractionControl * Time.fixedDeltaTime);
            rb.linearVelocity = transform.TransformDirection(localVelocity);
        }
    }
}
