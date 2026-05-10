using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all player input from gamepad and keyboard.
/// Organizes input into three categories: Thrust, Camera, and UI.
/// </summary>
public class PlayerInputController : MonoBehaviour
{
    [System.Serializable]
    public struct ThrustInput
    {
        public float verticalThrust;      // Up/Down (W/S or Right Stick Y)
        public float horizontalThrust;    // Left/Right (A/D or Left Stick X)
        public float forwardThrust;       // Forward/Back (Up/Down or Left Stick Y)
        public float leftRotation;        // Rotate Left (Q or LT)
        public float rightRotation;       // Rotate Right (E or RT)
    }

    [System.Serializable]
    public struct CameraInput
    {
        public Vector2 aimDelta;          // Camera aim (Arrow Keys or Right Stick)
        public bool resetAim;             // Reset camera (R or Y Button)
    }

    [System.Serializable]
    public struct UIInput
    {
        public bool pauseMenu;            // Pause (ESC or Start)
        public bool attemptDocking;       // Dock (E or B Button)
        public bool stabilisation;        // Stabilise (SPACE or A Button)
        public Vector2 uiNavigation;      // UI navigation
    }

    public ThrustInput ThrustInputData { get; private set; }
    public CameraInput CameraInputData { get; private set; }
    public UIInput UIInputData { get; private set; }

    private InputActionMap gameplayActions;
    private InputActionMap uiActions;

    private void Start()
    {
        if (Gamepad.current == null)
        {
            Debug.LogWarning("No gamepad detected. Using keyboard input.");
        }
    }

    private void Update()
    {
        ReadThrustInput();
        ReadCameraInput();
        ReadUIInput();
    }

    private void ReadThrustInput()
    {
        ThrustInput thrust = new ThrustInput();

        // Gamepad input (Right Stick Y)
        if (Gamepad.current != null)
        {
            thrust.verticalThrust = Gamepad.current.rightStick.ReadValue().y;
            thrust.horizontalThrust = Gamepad.current.leftStick.ReadValue().x;
            thrust.forwardThrust = -Gamepad.current.leftStick.ReadValue().y;
            thrust.leftRotation = Gamepad.current.leftTrigger.ReadValue();
            thrust.rightRotation = Gamepad.current.rightTrigger.ReadValue();
        }

        // Keyboard fallback
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) thrust.verticalThrust = Mathf.Max(thrust.verticalThrust, 1f);
            if (Keyboard.current.sKey.isPressed) thrust.verticalThrust = Mathf.Min(thrust.verticalThrust, -1f);
            if (Keyboard.current.aKey.isPressed) thrust.horizontalThrust = Mathf.Min(thrust.horizontalThrust, -1f);
            if (Keyboard.current.dKey.isPressed) thrust.horizontalThrust = Mathf.Max(thrust.horizontalThrust, 1f);
            if (Keyboard.current.upArrowKey.isPressed) thrust.forwardThrust = Mathf.Max(thrust.forwardThrust, 1f);
            if (Keyboard.current.downArrowKey.isPressed) thrust.forwardThrust = Mathf.Min(thrust.forwardThrust, -1f);
            if (Keyboard.current.qKey.isPressed) thrust.leftRotation = 1f;
            if (Keyboard.current.eKey.isPressed) thrust.rightRotation = 1f;
        }

        ThrustInputData = thrust;
    }

    private void ReadCameraInput()
    {
        CameraInput camera = new CameraInput();

        // Gamepad input (Right Stick)
        if (Gamepad.current != null)
        {
            camera.aimDelta = Gamepad.current.rightStick.ReadValue();
            camera.resetAim = Gamepad.current.yButton.wasPressedThisFrame;
        }

        // Keyboard fallback
        if (Keyboard.current != null)
        {
            Vector2 keyboardAim = Vector2.zero;
            if (Keyboard.current.upArrowKey.isPressed) keyboardAim.y += 1f;
            if (Keyboard.current.downArrowKey.isPressed) keyboardAim.y -= 1f;
            if (Keyboard.current.leftArrowKey.isPressed) keyboardAim.x -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) keyboardAim.x += 1f;
            camera.aimDelta = keyboardAim;
            camera.resetAim = Keyboard.current.rKey.wasPressedThisFrame;
        }

        CameraInputData = camera;
    }

    private void ReadUIInput()
    {
        UIInput ui = new UIInput();

        if (Gamepad.current != null)
        {
            ui.pauseMenu = Gamepad.current.startButton.wasPressedThisFrame;
            ui.attemptDocking = Gamepad.current.bButton.wasPressedThisFrame;
            ui.stabilisation = Gamepad.current.aButton.wasPressedThisFrame;
        }

        if (Keyboard.current != null)
        {
            ui.pauseMenu = Keyboard.current.escapeKey.wasPressedThisFrame;
            ui.attemptDocking = Keyboard.current.eKey.wasPressedThisFrame;
            ui.stabilisation = Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        UIInputData = ui;
    }

    /// <summary>
    /// Returns thrust power as 0-255 for UI display.
    /// </summary>
    public int GetThrustPower()
    {
        float power = new Vector3(
            ThrustInputData.horizontalThrust,
            ThrustInputData.verticalThrust,
            ThrustInputData.forwardThrust
        ).magnitude;
        return Mathf.RoundToInt(power * 255f);
    }

    public bool IsThrustActive()
    {
        return ThrustInputData.verticalThrust != 0 ||
               ThrustInputData.horizontalThrust != 0 ||
               ThrustInputData.forwardThrust != 0;
    }

    public bool IsRotating()
    {
        return ThrustInputData.leftRotation > 0 || ThrustInputData.rightRotation > 0;
    }
}
