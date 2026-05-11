using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SpacecraftController : MonoBehaviour
{
    [Header("Flight Dynamics")]
    public float forwardThrust = 100f;
    public float brakingThrust = 800f;
    public float autoTurnSpeed = 20f;

    [Header("Camera System")]
    public Transform cameraTransform;
    public float cameraSensitivity = 2.0f;
    public float followDistance = 12f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference cameraAction;
    public InputActionReference rollLeftAction;
    public InputActionReference rollRightAction;
    public InputActionReference brakeAction;

    private Rigidbody rb;
    private float pitch;
    private float yaw;
    private Vector3 currentMoveTargetDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = 0f;

        if (cameraTransform != null)
        {
            Vector3 angles = cameraTransform.eulerAngles;
            pitch = angles.x;
            yaw = angles.y;
        }
    }

    private void OnEnable()
    {
        if (cameraAction?.action != null) cameraAction.action.Enable();
        if (moveAction?.action != null) moveAction.action.Enable();
        if (rollLeftAction?.action != null) rollLeftAction.action.Enable();
        if (rollRightAction?.action != null) rollRightAction.action.Enable();
        if (brakeAction?.action != null) brakeAction.action.Enable();
    }

    private void OnDisable()
    {
        if (cameraAction?.action != null) cameraAction.action.Disable();
        if (moveAction?.action != null) moveAction.action.Disable();
        if (rollLeftAction?.action != null) rollLeftAction.action.Disable();
        if (rollRightAction?.action != null) rollRightAction.action.Disable();
        if (brakeAction?.action != null) brakeAction.action.Disable();
    }

    private void FixedUpdate()
    {
        bool isManual = rollLeftAction.action.IsPressed() || rollRightAction.action.IsPressed();

        HandleMovement(isManual);
        HandleEmergencyBrakes();
        HandleCameraLogic(isManual);
    }

    private void LateUpdate()
    {
        // Camera update in LateUpdate to avoid jitter
        bool isManual = rollLeftAction.action.IsPressed() || rollRightAction.action.IsPressed();

    }

    private void HandleCameraLogic(bool isManual)
    {
        if (cameraTransform == null) return;

        Vector2 lookInput = cameraAction.action.ReadValue<Vector2>();

        yaw += lookInput.x * cameraSensitivity;
        pitch -= lookInput.y * cameraSensitivity;

        yaw = NormalizeAngle(yaw);
        pitch = NormalizeAngle(pitch);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);

        // Use rb.position instead of transform.position for smoother follow with physics
        cameraTransform.rotation = targetRotation;
        cameraTransform.position = rb.position - (targetRotation * Vector3.forward * followDistance);
    }

    private void HandleMovement(bool isManual)
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 0.01f && cameraTransform != null)
        {
            Vector3 camFwd = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            currentMoveTargetDir = (camFwd * moveInput.y + camRight * moveInput.x).normalized;

            if (!isManual)
            {
                Quaternion targetRot = Quaternion.LookRotation(currentMoveTargetDir, cameraTransform.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * autoTurnSpeed));
            }

            float velocityAlignment = rb.linearVelocity.magnitude > 0.01f
                ? Vector3.Dot(rb.linearVelocity.normalized, currentMoveTargetDir)
                : 1f;

            float force = (velocityAlignment < 0.6f && rb.linearVelocity.magnitude > 1f)
                ? brakingThrust
                : forwardThrust;

            rb.AddForce(currentMoveTargetDir * force, ForceMode.Force);
        }
    }

    private void HandleEmergencyBrakes()
    {
        if (brakeAction.action.IsPressed())
        {
            rb.AddForce(-rb.linearVelocity * 50f, ForceMode.Acceleration);
            rb.AddTorque(-rb.angularVelocity * 50f, ForceMode.Acceleration);
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}