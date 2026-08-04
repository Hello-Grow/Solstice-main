using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteToMeshConverter : EditorWindow
{
    [MenuItem("Tools/Convert Selected Sprites to 3D Meshes")]
    public static void ConvertSprites()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Please select GameObjects with SpriteRenderers in the Hierarchy first!");
            return;
        }

        string folderPath = "Assets/Generated3DSprites";
        if (!Directory.Exists(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Generated3DSprites");
        }

        int convertedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) continue;

            Sprite sprite = sr.sprite;
            Texture2D texture = sprite.texture;

            // 1. MATERIAL SETUP (Shared per texture)
            string matPath = $"{folderPath}/Mat_{texture.name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Shader Graphs/TwoSidedLit");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                mat = new Material(shader);
                mat.mainTexture = texture;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
                
                if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 1.0f);
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);
                mat.EnableKeyword("_ALPHATEST_ON");
                
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.enableInstancing = false; 
            mat.doubleSidedGI = true;
            EditorUtility.SetDirty(mat);

            // 2. CREATE UNIQUE MESH PER INSTANCE (Crucial for Lightmap UV Atlasing)
            Mesh mesh = new Mesh();
            mesh.name = $"Mesh_{sprite.name}_{obj.GetInstanceID()}";

            Vector2[] spriteVerts = sprite.vertices;
            Vector3[] verts3D = new Vector3[spriteVerts.Length];
            
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = 0; i < spriteVerts.Length; i++)
            {
                float x = spriteVerts[i].x;
                float y = spriteVerts[i].y;
                verts3D[i] = new Vector3(x, y, 0.001f); // Micro-depth to prevent zero-area dropouts

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            ushort[] spriteTris = sprite.triangles;
            int[] tris = new int[spriteTris.Length];
            for (int i = 0; i < spriteTris.Length; i++)
            {
                tris[i] = spriteTris[i];
            }

            // Normalize UV2s for clean 0..1 layout
            float width = maxX - minX;
            float height = maxY - minY;
            Vector2[] normalizedUV2 = new Vector2[spriteVerts.Length];

            for (int i = 0; i < spriteVerts.Length; i++)
            {
                float u = (width > 0.0001f) ? (spriteVerts[i].x - minX) / width : 0f;
                float v = (height > 0.0001f) ? (spriteVerts[i].y - minY) / height : 0f;
                normalizedUV2[i] = new Vector2(u, v);
            }

            mesh.vertices = verts3D;
            mesh.triangles = tris;
            mesh.uv = sprite.uv;        
            mesh.uv2 = normalizedUV2;   
            
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            string meshPath = $"{folderPath}/Mesh_{sprite.name}_{obj.GetInstanceID()}.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);
            EditorUtility.SetDirty(mesh);

            // 3. APPLY COMPONENTS
            Undo.RegisterCompleteObjectUndo(obj, "Convert Sprite to Mesh");
            Undo.DestroyObjectImmediate(sr);
            
            MeshFilter mf = Undo.AddComponent<MeshFilter>(obj);
            MeshRenderer mr = Undo.AddComponent<MeshRenderer>(obj);

            mf.sharedMesh = mesh;
            mr.sharedMaterial = mat;

            mr.receiveShadows = true;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
            mr.scaleInLightmap = 1.0f;
            mr.receiveGI = ReceiveGI.Lightmaps;
            
            GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.ContributeGI);

            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully converted {convertedCount} GameObjects with unique non-overlapping lightmap meshes!");
    }
}