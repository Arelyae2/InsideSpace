# Spacecraft Controller System Documentation

## Overview
The Spacecraft Controller System implements a precision flight control system inspired by Outer Wilds. It features multi-axis thrust control, smooth camera aiming, and precision docking mechanics.

## Components

### PlayerInputController
Reads all player input from gamepad and keyboard.

**Features:**
- Gamepad support (XInput) with keyboard fallback
- Three input categories: Thrust, Camera, UI
- Normalized input values (0-1)
- Helper methods for thrust power and state checking

**Input Mapping:**

| Action | Gamepad | Keyboard |
|--------|---------|----------|
| Up/Down Thrust | Right Stick Y | W/S |
| Left/Right Thrust | Left Stick X | A/D |
| Forward/Back | Left Stick Y | ↑/↓ |
| Left Rotation | LT | Q |
| Right Rotation | RT | E |
| Camera Aim | Right Stick | Arrow Keys |
| Reset Aim | Y Button | R |
| Pause Menu | Start | ESC |
| Attempt Docking | B Button | E |
| Stabilisation | A Button | SPACE |

### SpacecraftController
Applies physics-based forces and torque to the spacecraft.

**Features:**
- Multi-axis thrust system (vertical, horizontal, forward)
- Rotational control around Y-axis
- Stabilisation system for velocity damping
- Velocity limiting
- Vertical thrust multiplier (1.5x by default)

**Configuration:**
- `thrustForce`: Base thrust force magnitude
- `verticalThrustMultiplier`: Multiplier for vertical thrust (more powerful)
- `rotationTorque`: Rotation torque magnitude
- `stabilisationDrag`: Velocity damping factor (0.95 = 95% retained per frame)

### CameraController
Handles smooth camera aiming with FOV control and reset functionality.

**Features:**
- Smooth aim with sensitivity adjustment
- FOV adjustment and reset
- Pitch/yaw clamping to prevent camera flipping
- Automatic reset to default position/FOV
- Configurable aiming smoothness

**Configuration:**
- `aimSensitivity`: Mouse/stick sensitivity
- `maxPitch`: Maximum pitch angle (±80°)
- `aimSmoothing`: Smoothing factor for aim transitions
- `defaultFOV`: Starting field of view (60°)
- `fovAdjustmentSpeed`: Speed of FOV transitions

### DockingSystem
Manages precision docking mechanics.

**Features:**
- Docking box collision validation
- Requires player to hold position and low velocity
- Progress tracking (0-1)
- Visual Gizmo debugging
- Configurable docking time and max velocity

**Configuration:**
- `dockingBoxSize`: Dimensions of valid docking zone
- `requiredDockingTime`: Time to hold position (3 seconds default)
- `maxAllowedVelocity`: Max velocity to dock (0.5 m/s default)
- `visualizeGizmos`: Toggle Gizmo visualization

## Setup Guide

### 1. Create Spacecraft GameObject
```
Spacecraft (GameObject)
├── Mesh (Model)
├── Rigidbody (with constraints)
├── PlayerInputController (Script)
├── SpacecraftController (Script)
├── Camera (Child Object)
│   └── CameraController (Script)
└── DockingSystem (Script)
```

### 2. Configure Rigidbody
- Set Mass: 1000 (or desired mass)
- Gravity: OFF
- Constraints: Freeze Rotation X, Y (allow Z rotation only)
- Use Gravity: OFF
- Collision Detection: Continuous or Continuous Dynamic

### 3. Attach Scripts
1. Add **PlayerInputController** to the root Spacecraft GameObject
2. Add **SpacecraftController** to the root, assign the Rigidbody
3. Create Camera child object, add **CameraController**
4. Add **DockingSystem** to the root or a docking trigger area

### 4. Configure Controllers
Adjust parameters in the Inspector:
- Thrust Force: 100-500 depending on ship mass
- Stabilisation Drag: 0.85-0.95 (higher = more damping)
- Camera Sensitivity: 1-3 (personal preference)
- Docking Time: 2-5 seconds

## Usage

### In Your Code
```csharp
// Get thrust power for UI display
int power = spacecraftController.GetThrustPower(); // 0-255

// Check if stabilisation is active
bool isStable = spacecraftController.IsStabilisationActive();

// Get docking progress
float progress = dockingSystem.GetDockingProgress(); // 0-1

// Check docking status
bool docked = dockingSystem.IsDocked();
```

### Custom Docking Behavior
```csharp
public class CustomDocking : DockingSystem
{
    protected override void OnDockingComplete()
    {
        Debug.Log("Custom docking sequence!");
        // Your custom code here
    }
}
```

## Physics Tuning

### For Slow, Precise Control
- Reduce `thrustForce` (50-100)
- Increase `stabilisationDrag` (0.98+)
- Reduce `rotationTorque` (10-20)

### For Fast, Responsive Control
- Increase `thrustForce` (200-500)
- Decrease `stabilisationDrag` (0.85-0.90)
- Increase `rotationTorque` (50-100)

### For Arcade-Style Flying
- High `thrustForce` (300+)
- Low `stabilisationDrag` (0.80)
- Medium `rotationTorque` (50)
- High velocity limit (`maxVelocity`: 100+)

## Troubleshooting

**Spacecraft won't move:**
- Check Rigidbody is set to Dynamic (not Static)
- Verify gravity is OFF
- Ensure constraints don't freeze all axes
- Check input is being detected

**Camera behaves erratically:**
- Reduce `aimSensitivity`
- Increase `aimSmoothing`
- Check for conflicting input systems

**Docking won't complete:**
- Verify Rigidbody velocity is below threshold
- Increase `requiredDockingTime`
- Increase `maxAllowedVelocity` threshold
- Check docking box size isn't too small

## Performance Notes
- All scripts use efficient frame updates
- Gizmos only draw in editor (disabled in builds)
- Physics calculations optimized for stability
- Input system uses Unity's new input system (fallback to old system)

## Future Enhancements
- Thruster particle effects
- Sound effects for thrust
- Autopilot/dock assistance systems
- Advanced flight computer UI
- Fuel management system
- Damage and repair mechanics
