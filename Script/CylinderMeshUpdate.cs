using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderMeshUpdate : ProceduralCylinderMesh
{
    [Header("Cylinder Mesh Modification")]
    public int cylinderBiteIndex = 2;
    public int cylinderBiteAmount = 2;

    private List<int> triangleBiteList = new List<int>();

    private new void Awake()
    {
        base.Awake();
    }

    private new void Start()
    {
        base.Start();
    }

    public override void GetCylinderTop()
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
            if (i < cylinderBiteIndex || i > cylinderBiteIndex + cylinderBiteAmount)
            {
                triangleTopList.Add(triangleTopCenterIndices);
                triangleTopList.Add(triangleVertexIndices + 1);
                triangleTopList.Add(triangleVertexIndices + 2);
            }

            triangleVertexIndices++;
        }

        triangleList.InsertRange(triangleList.Count, triangleTopList);
    }

    public override void GetCylinderBottom()
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
            if (i < cylinderBiteIndex || i > cylinderBiteIndex + cylinderBiteAmount)
            {
                triangleBottomList.Add(triangleBottomCenterIndices);
                triangleBottomList.Add(triangleVertexIndices + 2);
                triangleBottomList.Add(triangleVertexIndices + 1);
            }

            triangleVertexIndices++;
        }

        triangleList.InsertRange(triangleList.Count, triangleBottomList);
    }

    public override void GetCylinderSide()
    {
        for (int i = 0, incrementIndices = 0; i < vertexTopList.Count - 1; i++, incrementIndices++)
        {
            if (i < cylinderBiteIndex || i > cylinderBiteIndex + cylinderBiteAmount)
            {
                triangleSideList.Add((triangleBottomCenterIndices + incrementIndices) + 1);
                triangleSideList.Add((triangleTopCenterIndices + incrementIndices) + 2);
                triangleSideList.Add((triangleTopCenterIndices + incrementIndices) + 1);
            }

            triangleVertexIndices++;

            if (i < cylinderBiteIndex || i > cylinderBiteIndex + cylinderBiteAmount)
            {
                triangleSideList.Add((triangleBottomCenterIndices + incrementIndices) + 2);
                triangleSideList.Add((triangleTopCenterIndices + incrementIndices) + 2);
                triangleSideList.Add((triangleBottomCenterIndices + incrementIndices) + 1);
            }

            triangleVertexIndices++;

            if (i == cylinderBiteIndex)
            {
                triangleBiteList.Add((triangleBottomCenterIndices + incrementIndices) + 1);
                triangleBiteList.Add(triangleBottomCenterIndices);
                triangleBiteList.Add((triangleTopCenterIndices + incrementIndices) + 1);

                triangleBiteList.Add((triangleTopCenterIndices + incrementIndices) + 1);
                triangleBiteList.Add(triangleBottomCenterIndices);
                triangleBiteList.Add(triangleTopCenterIndices);
            }
            else if (i == (cylinderBiteIndex + 1) + cylinderBiteAmount)
            {
                // We added + 1 to cylinderBiteIndex to make sure the triangle is drawn to the correct index position
                triangleBiteList.Add((triangleBottomCenterIndices + incrementIndices) + 1);
                triangleBiteList.Add((triangleTopCenterIndices + incrementIndices) + 1);
                triangleBiteList.Add(triangleBottomCenterIndices);

                triangleBiteList.Add(triangleBottomCenterIndices);
                triangleBiteList.Add((triangleTopCenterIndices + incrementIndices) + 1);
                triangleBiteList.Add(triangleTopCenterIndices);
            }
        }

        triangleList.InsertRange(triangleList.Count, triangleSideList);
        triangleList.InsertRange(triangleList.Count, triangleBiteList);
    }
}
