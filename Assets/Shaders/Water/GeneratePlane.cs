using UnityEngine;

public class GeneratePlane : MonoBehaviour
{
    private Mesh mesh;
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;
    [SerializeField] private Transform mainLight;
    private Transform playerTransform;
    private GameManager gameManager;
    private Material material;

    private void Awake()
    {
        playerTransform = FindFirstObjectByType<PlayerController>().transform;
        gameManager = FindFirstObjectByType<GameManager>();
        Generate();
        material = GetComponent<MeshRenderer>().material;
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3((float)x-xSize/2,0.0f, (float)y-ySize/2);
            }
        }
        mesh.vertices = vertices;

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        //transform.position = new Vector3(-xSize/2, -10.0f,-ySize/2);
    }
    private void Update()
    {
        material.SetVector("_LightDir", mainLight.forward);
        material.SetFloat("_SunsetLerp", gameManager.GetSunsetLerp());
        transform.position = new Vector3(playerTransform.position.x,transform.position.y,transform.position.z);
    }
}
