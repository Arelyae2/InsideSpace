using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro; // Required for TextMeshPro UI

[RequireComponent(typeof(Rigidbody))]
public class SpacecraftController : MonoBehaviour
{
    [Header("Thrust Settings")]
    public float verticalThrustMultiplier = 50f;
    public float horizontalThrustMultiplier = 30f;
    public float rotationThrustMultiplier = 15f;

    [Header("Stabilisation Settings")]
    public float brakeStrength = 10f;
    public float levelSpeed = 5f;

    [Header("Cinemachine Integration")]
    public CinemachineCamera orbitalCamera;
    public float cameraSensitivity = 100f;

    [Header("UI Debug Settings")]
    [Tooltip("Drag a TextMeshPro - Text (GUI) object here")]
    public TextMeshProUGUI speedDebugText;

    [Header("Input Action References")]
    public InputActionReference upThrustAction;
    public InputActionReference downThrustAction;
    public InputActionReference horizontalThrustAction;
    public InputActionReference rotateLeftAction;
    public InputActionReference rotateRightAction;
    public InputActionReference stabilizeAction;
    public InputActionReference cameraStickAction;
    public InputActionReference dockAction;

    private Rigidbody rb;
    private CinemachineOrbitalFollow orbitalFollow;
    private float cameraYawOffset = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (orbitalCamera != null)
        {
            orbitalFollow = orbitalCamera.GetComponent<CinemachineOrbitalFollow>();
        }
    }

    private void OnEnable()
    {
        if (dockAction != null) dockAction.action.performed += OnDockAttempt;
    }

    private void OnDisable()
    {
        if (dockAction != null) dockAction.action.performed -= OnDockAttempt;
    }

    private void FixedUpdate()
    {
        HandleThrust();
        HandleRotation();
        HandleStabilisation();
    }

    private void Update()
    {
        UpdateUIDebug();
        HandleCameraSync();
    }

    private void HandleThrust()
    {
        float upRaw = upThrustAction.action.ReadValue<float>();
        float downRaw = downThrustAction.action.ReadValue<float>();
        float netVertical = upRaw - downRaw;

        rb.AddRelativeForce(Vector3.up * netVertical * verticalThrustMultiplier, ForceMode.Force);

        Vector2 horizontalInput = horizontalThrustAction.action.ReadValue<Vector2>();
        Vector3 horizontalForce = new Vector3(horizontalInput.x, 0f, horizontalInput.y);
        rb.AddRelativeForce(horizontalForce * horizontalThrustMultiplier, ForceMode.Force);
    }

    private void HandleRotation()
    {
        float leftRot = rotateLeftAction.action.ReadValue<float>();
        float rightRot = rotateRightAction.action.ReadValue<float>();
        float netRotation = rightRot - leftRot;

        rb.AddRelativeTorque(Vector3.up * netRotation * rotationThrustMultiplier, ForceMode.Force);
    }

    private void HandleStabilisation()
    {
        if (stabilizeAction.action.IsPressed())
        {
            rb.AddForce(-rb.linearVelocity * brakeStrength, ForceMode.Acceleration);
            rb.AddTorque(-rb.angularVelocity * brakeStrength, ForceMode.Acceleration);

            Vector3 currentForward = transform.forward;
            currentForward.y = 0;
            if (currentForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetLevelRotation = Quaternion.LookRotation(currentForward, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetLevelRotation, Time.fixedDeltaTime * levelSpeed));
            }
        }
    }

    private void HandleCameraSync()
    {
        if (orbitalFollow == null) return;

        Vector2 camInput = cameraStickAction.action.ReadValue<Vector2>();
        cameraYawOffset += camInput.x * cameraSensitivity * Time.fixedDeltaTime;

        orbitalFollow.HorizontalAxis.Value = transform.eulerAngles.y + cameraYawOffset;
        orbitalFollow.VerticalAxis.Value += camInput.y * (cameraSensitivity * 0.5f) * Time.fixedDeltaTime;
    }

    private void UpdateUIDebug()
    {
        if (speedDebugText == null) return;

        // Calculate speeds
        float linearSpeed = rb.linearVelocity.magnitude;
        float verticalSpeed = rb.linearVelocity.y; // World Y speed
        float angularSpeed = rb.angularVelocity.magnitude;

        // Construct the debug string
        // We use rich text tags (<b>) for readability
        string debugInfo = $"<b>SPEED TELEMETRY</b>\n" +
                           $"Linear Velocity: {linearSpeed:F2} m/s\n" +
                           $"Vertical Delta: {verticalSpeed:F2} m/s\n" +
                           $"Angular Spin: {angularSpeed:F2} rad/s\n" +
                           $"Stabilizer: {(stabilizeAction.action.IsPressed() ? "<color=green>ON</color>" : "<color=red>OFF</color>")}";

        speedDebugText.text = debugInfo;
    }

    private void OnDockAttempt(InputAction.CallbackContext context)
    {
        Debug.Log("[System] Docking Attempt initiated.");
    }
}