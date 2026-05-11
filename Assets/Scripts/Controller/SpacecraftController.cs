using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro;

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
    public TextMeshProUGUI speedDebugText;

    [Header("Gizmo Settings")]
    public bool drawMovementGizmos = true;
    public float gizmoScale = 2f;

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
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        if (orbitalCamera != null)
            orbitalFollow = orbitalCamera.GetComponent<CinemachineOrbitalFollow>();
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
        rb.AddRelativeForce(Vector3.up * (upRaw - downRaw) * verticalThrustMultiplier, ForceMode.Force);

        Vector2 horizontalInput = horizontalThrustAction.action.ReadValue<Vector2>();
        rb.AddRelativeForce(new Vector3(horizontalInput.x, 0f, horizontalInput.y) * horizontalThrustMultiplier, ForceMode.Force);
    }

    private void HandleRotation()
    {
        float leftRot = rotateLeftAction.action.ReadValue<float>();
        float rightRot = rotateRightAction.action.ReadValue<float>();
        rb.AddRelativeTorque(Vector3.up * (rightRot - leftRot) * rotationThrustMultiplier, ForceMode.Force);
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

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        Vector3 localAng = transform.InverseTransformDirection(rb.angularVelocity);

        speedDebugText.text = $"<b>[ LINEAR VELOCITY ]</b>\n" +
                             $"X: {localVel.x,6:F2} | Y: {localVel.y,6:F2} | Z: {localVel.z,6:F2}\n" +
                             $"<b>[ ROTATIONAL VELOCITY ]</b>\n" +
                             $"X: {localAng.x,6:F2} | Y: {localAng.y,6:F2} | Z: {localAng.z,6:F2}";
    }

    private void OnDrawGizmos()
    {
        if (!drawMovementGizmos || rb == null) return;

        Vector3 pos = transform.position;

        // --- Draw Linear Velocity Vector (Yellow) ---
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(pos, rb.linearVelocity * gizmoScale);
        Gizmos.DrawSphere(pos + rb.linearVelocity * gizmoScale, 0.1f);

        // --- Draw Local Axis Indicators (Helpful for orientation) ---
        // Red = Right (X), Green = Up (Y), Blue = Forward (Z)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos, transform.right * gizmoScale * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(pos, transform.up * gizmoScale * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(pos, transform.forward * gizmoScale * 0.5f);

        // --- Draw Angular Velocity (Cyan) ---
        // The ray points along the axis of rotation
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(pos, rb.angularVelocity * gizmoScale);
    }

    private void OnDockAttempt(InputAction.CallbackContext context) => Debug.Log("Docking Attempt...");
}