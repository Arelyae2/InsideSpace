using UnityEngine;

/// <summary>
/// Manages precision docking mechanics with position validation and hold-time requirements.
/// Visualizes docking zone with Gizmos for debugging.
/// </summary>
public class DockingSystem : MonoBehaviour
{
    [SerializeField] private PlayerInputController inputController;
    [SerializeField] private Rigidbody spacecraftRigidbody;
    [SerializeField] private SpacecraftController spacecraftController;

    [Header("Docking Zone")]
    [SerializeField] private Vector3 dockingBoxSize = new Vector3(5f, 5f, 5f);
    [SerializeField] private Transform dockingTarget;  // Optional: specific docking point

    [Header("Docking Requirements")]
    [SerializeField] private float requiredDockingTime = 3f;
    [SerializeField] private float maxAllowedVelocity = 0.5f;
    [SerializeField] private float positionCheckFrequency = 0.1f;

    [Header("Debugging")]
    [SerializeField] private bool visualizeGizmos = true;
    [SerializeField] private Color dockingZoneColor = new Color(0, 1, 0, 0.3f);

    private float dockingProgress = 0f;
    private float timeSinceValidPosition = 0f;
    private bool isDocked = false;
    private bool wasValidLastFrame = false;

    private void Start()
    {
        if (spacecraftRigidbody == null)
        {
            GameObject parent = transform.parent ?? gameObject;
            spacecraftRigidbody = parent.GetComponent<Rigidbody>();
        }

        if (spacecraftController == null)
        {
            GameObject parent = transform.parent ?? gameObject;
            spacecraftController = parent.GetComponent<SpacecraftController>();
        }

        if (inputController == null)
        {
            GameObject parent = transform.parent ?? gameObject;
            inputController = parent.GetComponent<PlayerInputController>();
        }
    }

    private void Update()
    {
        if (isDocked || spacecraftRigidbody == null)
            return;

        // Check if in valid docking position
        bool isValidPosition = IsInDockingZone();
        bool isVelocityValid = spacecraftRigidbody.velocity.magnitude <= maxAllowedVelocity;

        if (isValidPosition && isVelocityValid)
        {
            if (!wasValidLastFrame)
            {
                timeSinceValidPosition = 0f;
            }

            timeSinceValidPosition += Time.deltaTime;
            dockingProgress = Mathf.Clamp01(timeSinceValidPosition / requiredDockingTime);

            // Check if docking time complete
            if (timeSinceValidPosition >= requiredDockingTime)
            {
                CompleteDocking();
            }

            wasValidLastFrame = true;
        }
        else
        {
            // Reset progress if position becomes invalid
            if (wasValidLastFrame)
            {
                timeSinceValidPosition = 0f;
                dockingProgress = 0f;
            }
            wasValidLastFrame = false;
        }

        // Check for manual docking attempt
        if (inputController != null && inputController.UIInputData.attemptDocking)
        {
            if (isValidPosition && isVelocityValid)
            {
                timeSinceValidPosition = requiredDockingTime; // Skip wait time
            }
        }
    }

    private bool IsInDockingZone()
    {
        Vector3 dockingCenter = dockingTarget != null ? dockingTarget.position : transform.position;
        Vector3 relativePosition = spacecraftRigidbody.position - dockingCenter;

        // Check if within docking box
        if (Mathf.Abs(relativePosition.x) <= dockingBoxSize.x / 2 &&
            Mathf.Abs(relativePosition.y) <= dockingBoxSize.y / 2 &&
            Mathf.Abs(relativePosition.z) <= dockingBoxSize.z / 2)
        {
            return true;
        }

        return false;
    }

    private void CompleteDocking()
    {
        isDocked = true;
        dockingProgress = 1f;

        // Lock spacecraft in place
        if (spacecraftRigidbody != null)
        {
            spacecraftRigidbody.velocity = Vector3.zero;
            spacecraftRigidbody.angularVelocity = Vector3.zero;
            spacecraftRigidbody.isKinematic = true;
        }

        Debug.Log("Docking complete!");
        OnDockingComplete();
    }

    protected virtual void OnDockingComplete()
    {
        // Override this method in subclasses for custom behavior
    }

    public void ResetDocking()
    {
        isDocked = false;
        dockingProgress = 0f;
        timeSinceValidPosition = 0f;
        wasValidLastFrame = false;

        if (spacecraftRigidbody != null)
        {
            spacecraftRigidbody.isKinematic = false;
        }
    }

    public float GetDockingProgress()
    {
        return dockingProgress;
    }

    public bool IsDocked()
    {
        return isDocked;
    }

    public bool IsInDockingZone(Vector3 position)
    {
        Vector3 dockingCenter = dockingTarget != null ? dockingTarget.position : transform.position;
        Vector3 relativePosition = position - dockingCenter;

        return Mathf.Abs(relativePosition.x) <= dockingBoxSize.x / 2 &&
               Mathf.Abs(relativePosition.y) <= dockingBoxSize.y / 2 &&
               Mathf.Abs(relativePosition.z) <= dockingBoxSize.z / 2;
    }

    private void OnDrawGizmos()
    {
        if (!visualizeGizmos)
            return;

        Vector3 dockingCenter = dockingTarget != null ? dockingTarget.position : transform.position;

        // Draw docking zone
        Gizmos.color = isDocked ? Color.green : dockingZoneColor;
        Gizmos.DrawCube(dockingCenter, dockingBoxSize);

        // Draw wireframe
        Gizmos.color = isDocked ? Color.green : new Color(0, 1, 0, 1);
        DrawWireframeCube(dockingCenter, dockingBoxSize);
    }

    private void DrawWireframeCube(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size / 2;

        Vector3[] corners = new Vector3[8]
        {
            center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
            center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
            center + new Vector3(halfSize.x, halfSize.y, -halfSize.z),
            center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
            center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
            center + new Vector3(halfSize.x, -halfSize.y, halfSize.z),
            center + new Vector3(halfSize.x, halfSize.y, halfSize.z),
            center + new Vector3(-halfSize.x, halfSize.y, halfSize.z),
        };

        // Draw edges
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);

        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[5], corners[6]);
        Gizmos.DrawLine(corners[6], corners[7]);
        Gizmos.DrawLine(corners[7], corners[4]);

        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[7]);
    }
}
