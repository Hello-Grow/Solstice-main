using System.Collections;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [Header("Z")]
    public float targetZ = -5f;          // STRICTLY Absolute world Z when locked
    public float unlockedZOffset = -5f;  // STRICTLY Distance from player when unlocked
    public bool unlockZ = false; 
    
    [Header("Bounds")]
    public bool constrainToBounds = true;
    public float boundsInset = 0f;

    [Header("Entrypoint")]
    public bool Left = true;
    public bool Right = true;

    [Header("Wall Fading")]
    public Renderer[] frontWalls;        // Assign multiple Mesh/Sprite Renderers in the Inspector
    public float fadeDuration = 0.5f;

    public float MinX { get; private set; }
    public float MaxX { get; private set; }

    private CameraController camController;
    private Coroutine fadeCoroutine;

    void Start()
    {
        camController = Camera.main.GetComponent<CameraController>(); 
        if (constrainToBounds) GetBounds(); 
    }

    private void GetBounds() 
    {
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null)
        {
            MinX = col2D.bounds.min.x + boundsInset;
            MaxX = col2D.bounds.max.x - boundsInset;
        }
        else
        {
            Collider col3D = GetComponent<Collider>();
            if (col3D != null)
            {
                MinX = col3D.bounds.min.x + boundsInset;
                MaxX = col3D.bounds.max.x - boundsInset;
            }
        }

        if (MinX > MaxX)
        {
            float center = (MinX + MaxX) / 2f;
            MinX = center;
            MaxX = center;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            camController.RegisterZone(this);
            StartFade(0f); // 0f = Fully transparent
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            camController.UnregisterZone(this);
            StartFade(1f); // 1f = Fully opaque
        }
    }

    private void StartFade(float targetAlpha)
    {
        if (frontWalls == null || frontWalls.Length == 0) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        int count = frontWalls.Length;
        float[] startAlphas = new float[count];

        // Read the starting "_Alpha" directly from your custom Shader Graph
        for (int i = 0; i < count; i++)
        {
            if (frontWalls[i] != null)
            {
                startAlphas[i] = frontWalls[i].material.HasProperty("_Alpha") 
                    ? frontWalls[i].material.GetFloat("_Alpha") 
                    : frontWalls[i].material.color.a;
            }
        }

        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / fadeDuration);

            for (int i = 0; i < count; i++)
            {
                if (frontWalls[i] == null) continue;

                float newAlpha = Mathf.Lerp(startAlphas[i], targetAlpha, t);

                // Apply directly to your Shader Graph's _Alpha Blackboard property
                if (frontWalls[i].material.HasProperty("_Alpha"))
                {
                    frontWalls[i].material.SetFloat("_Alpha", newAlpha);
                }
                else
                {
                    // Fallback just in case a standard Unity material is used
                    Color col = frontWalls[i].material.color;
                    frontWalls[i].material.color = new Color(col.r, col.g, col.b, newAlpha);
                }
            }

            yield return null;
        }

        // Snap to exact target alpha at the end of the fade
        for (int i = 0; i < count; i++)
        {
            if (frontWalls[i] == null) continue;

            if (frontWalls[i].material.HasProperty("_Alpha"))
            {
                frontWalls[i].material.SetFloat("_Alpha", targetAlpha);
            }
            else
            {
                Color col = frontWalls[i].material.color;
                frontWalls[i].material.color = new Color(col.r, col.g, col.b, targetAlpha);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!constrainToBounds) return;

        Bounds bounds = new Bounds();
        bool hasBounds = false;

        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null) { bounds = col2D.bounds; hasBounds = true; }
        else
        {
            Collider col3D = GetComponent<Collider>();
            if (col3D != null) { bounds = col3D.bounds; hasBounds = true; }
        }

        if (hasBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            float gizmoMinX = bounds.min.x + boundsInset;
            float gizmoMaxX = bounds.max.x - boundsInset;

            if (gizmoMinX > gizmoMaxX)
            {
                float center = (gizmoMinX + gizmoMaxX) / 2f;
                gizmoMinX = center;
                gizmoMaxX = center;
            }

            Gizmos.color = Color.red;
            Vector3 insetSize = new Vector3(gizmoMaxX - gizmoMinX, bounds.size.y, bounds.size.z);
            Vector3 insetCenter = new Vector3((gizmoMinX + gizmoMaxX) / 2f, bounds.center.y, bounds.center.z);
            Gizmos.DrawWireCube(insetCenter, insetSize);

            Gizmos.color = new Color(0.6f, 0.2f, 0.8f, 0.6f); 
            float planeThickness = 0.01f; 

            if (!Left) 
            {
                Vector3 leftPlaneCenter = new Vector3(gizmoMinX, bounds.center.y, bounds.center.z);
                Vector3 leftPlaneSize = new Vector3(planeThickness, bounds.size.y, bounds.size.z);
                Gizmos.DrawCube(leftPlaneCenter, leftPlaneSize); 
            }

            if (!Right) 
            {
                Vector3 rightPlaneCenter = new Vector3(gizmoMaxX, bounds.center.y, bounds.center.z);
                Vector3 rightPlaneSize = new Vector3(planeThickness, bounds.size.y, bounds.size.z);
                Gizmos.DrawCube(rightPlaneCenter, rightPlaneSize);
            }
        }
    }
}