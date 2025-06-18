using System.Collections;
using UnityEngine;

public class WaterSurface : MonoBehaviour
{
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public int resolution = 64;
    public float width = 10f;
    public float springConstant = 0.02f;
    public float damping = 0.04f;
    public float spread = 0.05f;

    private float[] positions, velocities, accelerations;
    private LineRenderer line;

    void Start()
    {
        positions = new float[resolution];
        velocities = new float[resolution];
        accelerations = new float[resolution];

        line = GetComponent<LineRenderer>();
        line.positionCount = resolution;
        line.useWorldSpace = false;

        // Mesh初期化
        mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;

        // マテリアル設定（透過青など）
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = new Color(0.2f, 0.5f, 1f, 0.5f); // 水っぽい色
    }


    void Update()
    {
        // ばね物理シミュレーション
        for (int i = 0; i < resolution; i++)
        {
            float force = -springConstant * positions[i] - velocities[i] * damping;
            accelerations[i] = force;
        }

        for (int i = 0; i < resolution; i++)
        {
            velocities[i] += accelerations[i];
            positions[i] += velocities[i];
        }

        // 横に波を伝播
        float[] leftDeltas = new float[resolution];
        float[] rightDeltas = new float[resolution];

        for (int j = 0; j < 8; j++) // 伝播を複数回
        {
            for (int i = 0; i < resolution; i++)
            {
                if (i > 0)
                    leftDeltas[i] = spread * (positions[i] - positions[i - 1]);
                if (i < resolution - 1)
                    rightDeltas[i] = spread * (positions[i] - positions[i + 1]);
            }

            for (int i = 0; i < resolution; i++)
            {
                if (i > 0)
                    velocities[i - 1] += leftDeltas[i];
                if (i < resolution - 1)
                    velocities[i + 1] += rightDeltas[i];
            }
        }

        // LineRendererに更新
        for (int i = 0; i < resolution; i++)
        {
            float x = i * (width / (resolution - 1));
            line.SetPosition(i, new Vector3(x, positions[i], 0f));
        }
        UpdateMesh();

    }
    void UpdateMesh()
    {
        Vector3[] vertices = new Vector3[resolution * 2];
        int[] triangles = new int[(resolution - 1) * 6];

        for (int i = 0; i < resolution; i++)
        {
            float x = i * (width / (resolution - 1));
            float y = positions[i];
            vertices[i] = new Vector3(x, y, 0);        // 上辺（波）
            vertices[i + resolution] = new Vector3(x, -5f, 0); // 下辺（固定値で水底）
        }

        int t = 0;
        for (int i = 0; i < resolution - 1; i++)
        {
            int topLeft = i;
            int topRight = i + 1;
            int bottomLeft = i + resolution;
            int bottomRight = i + 1 + resolution;

            // 左上→左下→右上
            triangles[t++] = topLeft;
            triangles[t++] = bottomLeft;
            triangles[t++] = topRight;

            // 右上→左下→右下
            triangles[t++] = topRight;
            triangles[t++] = bottomLeft;
            triangles[t++] = bottomRight;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void Splash(Vector3 worldPos, float force)
    {
        float x = transform.InverseTransformPoint(worldPos).x;
        int index = Mathf.Clamp(Mathf.RoundToInt((x / width) * (resolution - 1)), 0, resolution - 1);
        velocities[index] = force;
        Debug.Log($"Splash at index {index} with force {force} at position {worldPos}");
    }
    public Vector3 ClosestSurfacePoint(Vector3 worldPos)
    {
        float x = transform.InverseTransformPoint(worldPos).x;
        int index = Mathf.Clamp(Mathf.RoundToInt((x / width) * (resolution - 1)), 0, resolution - 1);

        float surfaceY = positions[index]; // 波のY値
        Vector3 localPos = new Vector3(x, surfaceY, 0f);
        return transform.TransformPoint(localPos);
    }


}
