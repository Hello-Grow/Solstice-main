using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EDTrigger : MonoBehaviour
{
    public enum TriggerSide { Front, Back, Left, Right }
    
    [Header("Trigger Settings")]
    [SerializeField] private TriggerSide activeSide = TriggerSide.Front;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool revertOnExit = true;
    
    [Header("Objects to Toggle")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;

    // Tracks if the trigger was successfully entered from the valid entrance
    private bool isActivated = false;

    // Ensure all attached box colliders are set to triggers in the editor
    private void Reset()
    {
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        foreach (BoxCollider col in colliders)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Check if player is entering from the selected side of the triggered box
        if (IsFromActiveSide(other.transform.position))
        {
            isActivated = true;
            ToggleObjects(objectsToEnable, true);
            ToggleObjects(objectsToDisable, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Only revert if revertOnExit is enabled, it was actually activated by entering correctly, and it's the player
        if (!revertOnExit || !isActivated || !other.CompareTag(playerTag)) return;

        // Check if player exits back out through the entrance side
        if (IsFromActiveSide(other.transform.position))
        {
            isActivated = false; // Reset the state
            
            // Revert states on exit
            ToggleObjects(objectsToEnable, false);
            ToggleObjects(objectsToDisable, true);
        }
        // If they exit from any other direction, IsFromActiveSide is false.
        // The objects remain toggled, and isActivated stays true.
    }

    /// Converts world pos to local pos relative to the CLOSEST box collider's center
    private bool IsFromActiveSide(Vector3 playerPosition)
    {
        BoxCollider closestBox = GetClosestBoxCollider(playerPosition);
        if (closestBox == null) return false;

        // Convert player position to local space, then offset by the specific box collider's center
        Vector3 localPos = transform.InverseTransformPoint(playerPosition) - closestBox.center;

        switch (activeSide)
        {
            case TriggerSide.Front: return localPos.z > 0;
            case TriggerSide.Back:  return localPos.z < 0;
            case TriggerSide.Right: return localPos.x > 0;
            case TriggerSide.Left:  return localPos.x < 0;
            default: return false;
        }
    }

    /// Finds whichever Box Collider is closest to the player so offset colliders work correctly
    private BoxCollider GetClosestBoxCollider(Vector3 position)
    {
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        BoxCollider closest = null;
        float minDistance = float.MaxValue;

        foreach (BoxCollider box in colliders)
        {
            if (!box.enabled) continue;

            // Get the world position of the box's center
            Vector3 worldCenter = transform.TransformPoint(box.center);
            float dist = Vector3.SqrMagnitude(position - worldCenter);

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = box;
            }
        }

        return closest;
    }

    /// Loop through and toggle object active states
    private void ToggleObjects(GameObject[] objects, bool state)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(state);
            }
        }
    }

    // Gizmos for visual debugging in the Scene view for ALL attached Box Colliders
    private void OnDrawGizmos()
    {
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        if (colliders.Length == 0) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        foreach (BoxCollider box in colliders)
        {
            if (!box.enabled) continue;

            // Draw Box Bounds
            Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(box.center, box.size);

            // Draw Directional Indicator
            Vector3 indicatorPos = box.center;
            Vector3 rayDir = Vector3.zero;

            switch (activeSide)
            {
                case TriggerSide.Front:
                    indicatorPos.z += box.size.z * 0.5f;
                    rayDir = Vector3.forward;
                    break;
                case TriggerSide.Back:
                    indicatorPos.z -= box.size.z * 0.5f;
                    rayDir = Vector3.back;
                    break;
                case TriggerSide.Right:
                    indicatorPos.x += box.size.x * 0.5f;
                    rayDir = Vector3.right;
                    break;
                case TriggerSide.Left:
                    indicatorPos.x -= box.size.x * 0.5f;
                    rayDir = Vector3.left;
                    break;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(indicatorPos, Mathf.Min(box.size.x, box.size.y, box.size.z) * 0.15f);
            Gizmos.DrawRay(indicatorPos, rayDir * 0.75f);
        }
    }
}
