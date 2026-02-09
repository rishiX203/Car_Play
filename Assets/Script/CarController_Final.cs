using UnityEngine;

public class CarController_Final : MonoBehaviour
{
    // WHEEL COLLIDERS (Wheels/Colliders)
    [SerializeField] WheelCollider Wheel_FL;
    [SerializeField] WheelCollider Wheel_FR;
    [SerializeField] WheelCollider Wheel_RL;
    [SerializeField] WheelCollider Wheel_RR;

    // WHEEL MESHES (Wheels/Meshes)
    [SerializeField] Transform Mesh_FL;
    [SerializeField] Transform Mesh_FR;
    [SerializeField] Transform Mesh_RL;
    [SerializeField] Transform Mesh_RR;

    [SerializeField] float motorPower = 2000f;
    [SerializeField] float steeringPower = 25f;

    void FixedUpdate()
    {
        // PC TEST : SPACE = move
        float motor = Input.GetKey(KeyCode.Space) ? motorPower : 0f;
        float steer = Input.GetAxis("Horizontal") * steeringPower;

        // Motor – all 4 wheels
        Wheel_FL.motorTorque = motor;
        Wheel_FR.motorTorque = motor;
        Wheel_RL.motorTorque = motor;
        Wheel_RR.motorTorque = motor;

        // Steering – front wheels only
        Wheel_FL.steerAngle = steer;
        Wheel_FR.steerAngle = steer;

        // Update wheel visuals
        UpdateWheel(Wheel_FL, Mesh_FL);
        UpdateWheel(Wheel_FR, Mesh_FR);
        UpdateWheel(Wheel_RL, Mesh_RL);
        UpdateWheel(Wheel_RR, Mesh_RR);
    }

    void UpdateWheel(WheelCollider col, Transform mesh)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}
