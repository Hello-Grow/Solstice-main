using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("Default Settings")]
    public Transform player; 
    public float smoothSpeed = 0.125f; 
    public float xOffset = 0f;
    public float yOffset = 0f; 

    [Header("Z")]
    public float zMoveSpeed = 3f;
    private float targetZ;
    private float unlockedZOffset; 
    private bool isZUnlocked = true; 
    
    // Captured initial offset at Start to use when outside all zones
    private float defaultZOffset; 

    private bool useBounds = false;
    private float minX;
    private float maxX;
    
    private bool Left;
    private bool Right;

    private List<CameraZone> activeZones = new List<CameraZone>();

    void Start()
    {
        if (player != null)
        {
            defaultZOffset = transform.position.z - player.position.z;
        }
        targetZ = transform.position.z;
        unlockedZOffset = defaultZOffset;
    }

    void LateUpdate()
    {
        if (player == null) return;
        
        // NO DOUBLE PURPOSE:
        // When unlocked, add the specific offset. When locked, use absolute targetZ.
        float actualTargetZ = isZUnlocked ? (player.position.z + unlockedZOffset) : targetZ;
        
        float currentZ = Mathf.Lerp(transform.position.z, actualTargetZ, Time.deltaTime * zMoveSpeed);

        float targetX = player.position.x + xOffset;
        float targetY = player.position.y + yOffset;

        if (useBounds)
        {
            float currentMinX = Left ? Mathf.Min(minX, transform.position.x) : float.MinValue;
            float currentMaxX = Right ? Mathf.Max(maxX, transform.position.x) : float.MaxValue;

            targetX = Mathf.Clamp(targetX, currentMinX, currentMaxX);
        }

        float smoothedX = Mathf.Lerp(transform.position.x, targetX, smoothSpeed);
        float smoothedY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed); 
        
        transform.position = new Vector3(smoothedX, smoothedY, currentZ);
    }

    public void RegisterZone(CameraZone zone)
    {
        if (activeZones.Contains(zone))
        {
            activeZones.Remove(zone);
        }
        
        activeZones.Add(zone);
        UpdateCurrentZone();
    }

    public void UnregisterZone(CameraZone zone)
    {
        if (activeZones.Contains(zone))
        {
            activeZones.Remove(zone);
        }
        UpdateCurrentZone();
    }

    private void UpdateCurrentZone()
    {
        activeZones.RemoveAll(z => z == null);

        if (activeZones.Count > 0)
        {
            CameraZone currentZone = activeZones[activeZones.Count - 1]; 
            
            targetZ = currentZone.targetZ;
            unlockedZOffset = currentZone.unlockedZOffset; // Reading the new separated value
            useBounds = currentZone.constrainToBounds;
            minX = currentZone.MinX;
            maxX = currentZone.MaxX;
            Left = currentZone.Left;
            Right = currentZone.Right;
            isZUnlocked = currentZone.unlockZ;
        }
        else
        {
            ResetToDefault();
        }
    }

    private void ResetToDefault()
    {
        useBounds = false;
        isZUnlocked = true; 
        unlockedZOffset = defaultZOffset; // Return to standard distance when leaving all zones
    }
}
