using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FakeReactiveShadows : MonoBehaviour
{
    [System.Serializable]
    public struct ShadowLightSource
    {
        public Light light;
        public float maxDistance;
    }

    [Header("Lighting Setup")]
    public ShadowLightSource[] shadowLights; 
    public float height = 2f; 
    public float intMult = 1f; 

    [Header("Shadow Properties")]
    public LayerMask ground = 1; // Ground layer for the shadow
    public LayerMask obstacleMask; // Layers that block light
    public float width = 1f; 
    [Range(2, 20)] public int segs = 8; 
    public Color color = Color.black; 
    [Tooltip("How fast the shadow fades in and out when blocked by an obstacle.")]
    public float occlusionFadeSpeed = 5f; // NEW: Controls fade speed

    private Mesh msh;
    private Vector3[] vts;
    private Color[] cls;
    private Vector2[] uvs;
    private int[] tris;

    // NEW: Tracks the current visibility/fade alpha for each light source
    private float[] lightFades;

    private int cchdCnt = -1; 

    void Start()
    {
        msh = new Mesh();
        msh.name = "DynamicMultiShadowMesh";
        GetComponent<MeshFilter>().mesh = msh;
    }

    void LateUpdate()
    {
        if (shadowLights == null || shadowLights.Length == 0)
        {
            msh.Clear();
            return;
        }

        if (shadowLights.Length != cchdCnt)
        {
            AllocArr(); 
        }

        GenMsh(); 
    }

    private void AllocArr()
    {
        cchdCnt = shadowLights.Length;
        int vCnt = shadowLights.Length * (segs + 1) * 2;
        int tCnt = shadowLights.Length * segs * 6;

        vts = new Vector3[vCnt];
        cls = new Color[vCnt];
        uvs = new Vector2[vCnt];
        tris = new int[tCnt];

        // NEW: Allocate and initialize the fade array so shadows start visible
        lightFades = new float[cchdCnt];
        for (int i = 0; i < cchdCnt; i++)
        {
            lightFades[i] = 1f;
        }
    }

    private void GenMsh()
    {
        Vector3 ftPos = transform.position;
        Vector3 topPos = ftPos + Vector3.up * height;
        // Calculate the center of the character for line-of-sight checks
        Vector3 centerPos = ftPos + Vector3.up * (height * 0.5f);

        int vIdx = 0;
        int tIdx = 0;

        for (int i = 0; i < shadowLights.Length; i++)
        {
            Light currLt = shadowLights[i].light;
            float currMaxDist = shadowLights[i].maxDistance;
            
            bool isOccluded = false;

            if (currLt != null && currLt.isActiveAndEnabled)
            {
                Vector3 lPos = currLt.transform.position;
                Vector3 dirToLight = lPos - centerPos;
                
                if (Physics.Raycast(centerPos, dirToLight.normalized, out RaycastHit hit, dirToLight.magnitude, obstacleMask))
                {
                    isOccluded = true;
                    Debug.DrawLine(centerPos, hit.point, Color.red); 
                }
                else
                {
                    Debug.DrawLine(centerPos, lPos, Color.green); 
                }
            }

            // NEW: Smoothly interpolate the fade value (0 = blocked/inactive, 1 = clear line of sight)
            float targetFade = (currLt != null && currLt.isActiveAndEnabled && !isOccluded) ? 1f : 0f;
            lightFades[i] = Mathf.MoveTowards(lightFades[i], targetFade, Time.deltaTime * occlusionFadeSpeed);

            // NEW: Only skip mesh generation if the shadow has completely faded out
            if (lightFades[i] <= 0.001f)
            {
                for (int j = 0; j < segs * 6; j++) tris[tIdx++] = 0;
                for (int j = 0; j < (segs + 1) * 2; j++) { vts[vIdx] = Vector3.zero; cls[vIdx] = Color.clear; uvs[vIdx++] = Vector2.zero; }
                continue;
            }

            Vector3 lightPos = currLt.transform.position;
            Vector3 lToT = topPos - lightPos;

            Vector3 fltLPos = new Vector3(lightPos.x, ftPos.y, lightPos.z);
            float fltDist = Vector3.Distance(fltLPos, ftPos);

            float dRat = Mathf.Clamp01(fltDist / currMaxDist);
            float dFd = Mathf.SmoothStep(1f, 0f, dRat); 
            float clmpInt = Mathf.Clamp01(currLt.intensity * intMult);
            
            // NEW: Multiply the final alpha by our smooth fade weight
            float fnlA = clmpInt * dFd * color.a * lightFades[i];
            
            Color curCol = new Color(color.r, color.g, color.b, fnlA);

            Vector3 shdEnd;
            if (lToT.y >= 0)
            {
                shdEnd = ftPos + new Vector3(lToT.x, 0, lToT.z).normalized * currMaxDist;
            }
            else
            {
                float t = (ftPos.y - lightPos.y) / lToT.y;
                shdEnd = lightPos + lToT * t;

                Vector3 shdVec = shdEnd - ftPos;
                if (shdVec.magnitude > currMaxDist)
                {
                    shdEnd = ftPos + shdVec.normalized * currMaxDist;
                }
            }

            Vector3 shdDir = (shdEnd - ftPos).normalized;
            Vector3 crsDir = Vector3.Cross(shdDir, Vector3.up).normalized;

            for (int k = 0; k <= segs; k++)
            {
                float t = (float)k / segs;
                Vector3 pntOnL = Vector3.Lerp(ftPos, shdEnd, t);

                if (k > 0)
                {
                    Vector3 rStrt = pntOnL + Vector3.up * 2f;
                    if (Physics.Raycast(rStrt, Vector3.down, out RaycastHit hit, 5f, ground))
                    {
                        pntOnL = hit.point + Vector3.up * 0.02f;
                    }
                }
                else
                {
                    pntOnL = ftPos + Vector3.up * 0.02f;
                }

                vts[vIdx] = transform.InverseTransformPoint(pntOnL + crsDir * (width / 2f));
                vts[vIdx + 1] = transform.InverseTransformPoint(pntOnL - crsDir * (width / 2f));

                Color vCol = curCol;
                vCol.a *= Mathf.SmoothStep(1f, 0f, t); 
                cls[vIdx] = vCol;
                cls[vIdx + 1] = vCol;

                uvs[vIdx] = new Vector2(1f, 1f - t);
                uvs[vIdx + 1] = new Vector2(0f, 1f - t);

                if (k < segs)
                {
                    tris[tIdx++] = vIdx;
                    tris[tIdx++] = vIdx + 2;
                    tris[tIdx++] = vIdx + 1;

                    tris[tIdx++] = vIdx + 1;
                    tris[tIdx++] = vIdx + 2;
                    tris[tIdx++] = vIdx + 3;
                }

                vIdx += 2;
            }
        }

        msh.Clear();
        msh.vertices = vts;
        msh.colors = cls;
        msh.uv = uvs;
        msh.triangles = tris;
        msh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (shadowLights == null) return;
        Vector3 ftPos = transform.position;
        Vector3 topPos = ftPos + Vector3.up * height;
        Vector3 centerPos = ftPos + Vector3.up * (height * 0.5f);

        foreach (ShadowLightSource setup in shadowLights)
        {
            Light l = setup.light;
            float currentMaxDist = setup.maxDistance;

            if (l == null || !l.isActiveAndEnabled) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(centerPos, l.transform.position);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(l.transform.position, currentMaxDist);
            
            if (Application.isPlaying)
            {
                Vector3 lToT = topPos - l.transform.position;
                Vector3 shdEnd = ftPos;
                if (lToT.y < 0)
                {
                    float t = (ftPos.y - l.transform.position.y) / lToT.y;
                    shdEnd = l.transform.position + lToT * t;
                    Vector3 shdVec = shdEnd - ftPos;
                    if (shdVec.magnitude > currentMaxDist) shdEnd = ftPos + shdVec.normalized * currentMaxDist;
                }
                else
                {
                    shdEnd = ftPos + new Vector3(lToT.x, 0, lToT.z).normalized * currentMaxDist;
                }

                Gizmos.color = Color.red;
                Gizmos.DrawLine(topPos, shdEnd);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(ftPos, shdEnd);
            }
        }
    }
}
