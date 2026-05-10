using UnityEngine;

/// <summary>
/// Applies physics-based forces and torque to the spacecraft.
/// Handles multi-axis thrust, rotation, and stabilisation systems.
/// </summary>
public class SpacecraftController : MonoBehaviour
{
    [SerializeField] private PlayerInputController inputController;
    [SerializeField] private Rigidbody spacecraftRigidbody;

    [Header("Thrust Configuration")]
    [SerializeField] private float thrustForce = 250f;
    [SerializeField] private float verticalThrustMultiplier = 1.5f;
    [SerializeField] private float rotationTorque = 50f;
    [SerializeField] private float maxVelocity = 50f;

    [Header("Stabilisation")]
    [SerializeField] private float stabilisationDrag = 0.95f; // 0.95 = 95% velocity retained
    [SerializeField] private bool stabilisationActive = false;

    private bool isStabilisationEnabled = false;

    private void Start()
    {
        if (spacecraftRigidbody == null)
            spacecraftRigidbody = GetComponent<Rigidbody>();

        if (inputController == null)
            inputController = GetComponent<PlayerInputController>();

        if (spacecraftRigidbody == null)
            Debug.LogError("SpacecraftController requires a Rigidbody component!");

        if (inputController == null)
            Debug.LogError("SpacecraftController requires a PlayerInputController component!");
    }

    private void FixedUpdate()
    {
        if (spacecraftRigidbody == null || inputController == null)
            return;

        ApplyThrust();
        ApplyRotation();
        ApplyStabilisation();
        LimitVelocity();

        // Toggle stabilisation
        if (inputController.UIInputData.stabilisation)
        {
            isStabilisationEnabled = !isStabilisationEnabled;
        }
    }

    private void ApplyThrust()
    {
        PlayerInputController.ThrustInput thrust = inputController.ThrustInputData;

        // Vertical thrust (Up/Down) - more powerful
        Vector3 verticalForce = Vector3.up * thrust.verticalThrust * thrustForce * verticalThrustMultiplier;

        // Horizontal thrust (Left/Right)
        Vector3 horizontalForce = Vector3.right * thrust.horizontalThrust * thrustForce;

        // Forward thrust (Forward/Back)
        Vector3 forwardForce = Vector3.forward * thrust.forwardThrust * thrustForce;

        // Apply all forces
        spacecraftRigidbody.AddForce(verticalForce, ForceMode.Force);
        spacecraftRigidbody.AddForce(horizontalForce, ForceMode.Force);
        spacecraftRigidbody.AddForce(forwardForce, ForceMode.Force);
    }

    private void ApplyRotation()
    {
        PlayerInputController.ThrustInput thrust = inputController.ThrustInputData;

        // Calculate rotation around Y-axis (left/right)
        float rotationInput = thrust.rightRotation - thrust.leftRotation;
        Vector3 rotationTorqueForce = Vector3.up * rotationInput * rotationTorque;

        spacecraftRigidbody.AddTorque(rotationTorqueForce, ForceMode.Force);
    }

    private void ApplyStabilisation()
    {
        if (!isStabilisationEnabled)
            return;

        // Apply drag to velocity
        spacecraftRigidbody.velocity *= stabilisationDrag;
        spacecraftRigidbody.angularVelocity *= stabilisationDrag;
    }

    private void LimitVelocity()
    {
        if (spacecraftRigidbody.velocity.magnitude > maxVelocity)
        {
            spacecraftRigidbody.velocity = spacecraftRigidbody.velocity.normalized * maxVelocity;
        }
    }

    public int GetThrustPower()
    {
        return inputController.GetThrustPower();
    }

    public bool IsStabilisationActive()
    {
        return isStabilisationEnabled;
    }

    public float GetCurrentVelocity()
    {
        return spacecraftRigidbody.velocity.magnitude;
    }

    public void SetStabilisationActive(bool active)
    {
        isStabilisationEnabled = active;
    }
}
