using UnityEngine;

/// <summary>
/// Handles smooth camera aiming with FOV control and reset functionality.
/// Supports gamepad and keyboard input through PlayerInputController.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerInputController inputController;
    [SerializeField] private Camera mainCamera;

    [Header("Aiming")]
    [SerializeField] private float aimSensitivity = 2f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float aimSmoothing = 0.15f;

    [Header("FOV")]
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float fovAdjustmentSpeed = 10f;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private float currentPitch = 0f;
    private float currentYaw = 0f;
    private Vector3 currentAimVelocity = Vector3.zero;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();

        if (inputController == null)
        {
            GameObject parent = transform.parent;
            inputController = parent != null ? parent.GetComponent<PlayerInputController>() : null;
        }

        // Store default camera state
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localRotation;
        mainCamera.fieldOfView = defaultFOV;

        if (mainCamera == null)
            Debug.LogError("CameraController requires a Camera component!");

        if (inputController == null)
            Debug.LogError("CameraController requires a PlayerInputController component in parent or self!");
    }

    private void Update()
    {
        if (inputController == null || mainCamera == null)
            return;

        UpdateAim();
        UpdateFOV();

        // Reset camera
        if (inputController.CameraInputData.resetAim)
        {
            ResetCamera();
        }
    }

    private void UpdateAim()
    {
        Vector2 aimDelta = inputController.CameraInputData.aimDelta * aimSensitivity;

        // Update pitch and yaw
        currentPitch -= aimDelta.y;
        currentYaw += aimDelta.x;

        // Clamp pitch to prevent flipping
        currentPitch = Mathf.Clamp(currentPitch, -maxPitch, maxPitch);

        // Apply smooth rotation
        Quaternion targetRotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, aimSmoothing);
    }

    private void UpdateFOV()
    {
        // Smooth FOV adjustment
        float targetFOV = defaultFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovAdjustmentSpeed);
    }

    public void ResetCamera()
    {
        currentPitch = 0f;
        currentYaw = 0f;
        transform.localPosition = defaultPosition;
        transform.localRotation = defaultRotation;
        mainCamera.fieldOfView = defaultFOV;
    }

    public void SetSensitivity(float sensitivity)
    {
        aimSensitivity = Mathf.Max(0.1f, sensitivity);
    }

    public float GetCurrentFOV()
    {
        return mainCamera.fieldOfView;
    }
}
