using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ProceduralCylinderMesh : MonoBehaviour
{
    [Header("Cylinder Profile")]
    public float cylinderRadius = 2f;
    public float cylinderHeight = -1f;
    public float cylinderStep = 7.5f;

    protected List<Vector3> vertexList = new List<Vector3>();

    protected List<Vector3> vertexTopList = new List<Vector3>();
    protected List<Vector3> vertexBottomList = new List<Vector3>();

    public int triangleTopCenterIndices { get; private set; }
    public int triangleBottomCenterIndices { get; private set; }
    public int triangleVertexIndices { get; set; }

    protected List<int> triangleList = new List<int>();

    protected List<int> triangleTopList = new List<int>();

    protected List<int> triangleSideList = new List<int>();

    protected List<int> triangleBottomList = new List<int>();

    private List<Vector3> normalList = new List<Vector3>();
    private List<Vector2> uvList = new List<Vector2>();
    private List<Vector4> tangentList = new List<Vector4>();

    private MeshFilter meshFilter = null;
    private MeshCollider meshCollider = null;

    protected void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        CreateCylinder();
    }

    protected void Start()
    {
        AddCollider();
    }

    private void CreateCylinder()
    {
        Mesh mesh = new Mesh
        {
            name = "Procedural Cylinder",
        };

        GetCylinderTop();
        GetCylinderBottom();
        GetCylinderSide();
        GetCylinderNormal();
        GetCylinderUV();
        GetCylinderTangents();

        meshFilter.mesh = SaveDataMesh(mesh);
    }

    protected void GetTopCenterVertex(Vector3 vertex)
    {
        vertexList.Insert(vertexList.Count, vertex);
        triangleTopCenterIndices = triangleList.Count > 0 ? triangleList[triangleList.Count - 1] + 1 : 0;
        triangleVertexIndices = triangleTopCenterIndices;
    }

    protected void GetBottomCenterVertex(Vector3 vertex)
    {
        vertexList.Insert(vertexList.Count, vertex);
        triangleBottomCenterIndices = triangleList.Count > 0 ? triangleList[triangleList.Count - 1] + 1 : 0;
        triangleVertexIndices = triangleBottomCenterIndices;
    }

    public virtual void GetCylinderTop()
    {
        GetTopCenterVertex(Vector3.zero);

        for (float angle = 0; (System.Math.Truncate(angle * 1000) / 1000) <= Mathf.PI * 2; angle += GetCylinderStep())
        {
            Vector3 vertex = new Vector3(GetParametricEquation(angle).x, 0, GetParametricEquation(angle).y);

            vertexTopList.Add(vertex);
        }

        vertexList.InsertRange(vertexList.Count, vertexTopList);

        // The (vertexTopList.Count - 1) is outer layer vertex of a cylinder excluding the last top vertex
        for (int i = 0; i < vertexTopList.Count - 1; i++)
        {
            triangleTopList.Add(triangleTopCenterIndices);
            triangleTopList.Add(triangleVertexIndices + 1);
            triangleTopList.Add(triangleVertexIndices + 2);

            triangleVertexIndices++;
        }

        triangleList.InsertRange(triangleList.Count, triangleTopList);
    }

    public virtual void GetCylinderBottom()
    {
        GetBottomCenterVertex(Vector3.down);

        for (int i = 0; i < vertexTopList.Count; i++)
        {
            Vector3 vertex = new Vector3(vertexTopList[i].x, cylinderHeight, vertexTopList[i].z);

            vertexBottomList.Add(vertex);
        }

        vertexList.InsertRange(vertexList.Count, vertexBottomList);

        for (int i = 0; i < vertexBottomList.Count - 1; i++)
        {
            triangleBottomList.Add(triangleBottomCenterIndices);
            triangleBottomList.Add(triangleVertexIndices + 2);
            triangleBottomList.Add(triangleVertexIndices + 1);

            triangleVertexIndices++;
        }

        triangleList.InsertRange(triangleList.Count, triangleBottomList);
    }

    public virtual void GetCylinderSide()
    {
        for (int i = 0, incrementIndices = 0; i < vertexTopList.Count - 1; i++, incrementIndices++)
        {
            triangleSideList.Add((triangleBottomCenterIndices + incrementIndices) + 1);
            triangleSideList.Add((triangleTopCenterIndices + incrementIndices) + 2);
            triangleSideList.Add((triangleTopCenterIndices + incrementIndices) + 1);

            triangleVertexIndices++;

            triangleSideList.Add((triangleBottomCenterIndices + incrementIndices) + 2);
            triangleSideList.Add((triangleTopCenterIndices + incrementIndices) + 2);
            triangleSideList.Add((triangleBottomCenterIndices + incrementIndices) + 1);

            triangleVertexIndices++;
        }

        triangleList.InsertRange(triangleList.Count, triangleSideList);
    }

    private void GetCylinderNormal()
    {
        for (int i = 0; i < vertexList.Count; i++)
        {
            Vector3 normal = vertexList[i] - transform.position;
            normalList.Add(normal.normalized);
        }
    }

    private void GetCylinderUV()
    {
        for (int i = 0; i < vertexList.Count; i++)
        {
            Vector3 uv = new Vector3(vertexList[i].x, vertexList[i].z);
            uvList.Add(uv);
        }
    }

    private void GetCylinderTangents()
    {
        for (int i = 0; i < vertexList.Count; i++)
        {
            tangentList.Add(new Vector4(1f, 0f, 0f, -1f));
        }
    }

    private void AddCollider()
    {
        if (meshCollider != null)
        {
            meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;
            meshCollider.convex = false;
            meshCollider.sharedMesh = meshFilter.mesh;
            meshCollider.enabled = true;
        }
    }

    private Mesh SaveDataMesh(Mesh mesh)
    {
        mesh.vertices = vertexList.ToArray();
        mesh.triangles = triangleList.ToArray();
        mesh.normals = normalList.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.tangents = tangentList.ToArray();

        return mesh;
    }

    protected float GetCylinderStep()
    {
        return (Mathf.PI / cylinderStep);
    }

    protected Vector2 GetParametricEquation(float angle)
    {
        return new Vector2(cylinderRadius * Mathf.Cos((Mathf.PI * 2) - angle),
                           cylinderRadius * Mathf.Sin((Mathf.PI * 2) - angle));
    }
}
