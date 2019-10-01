//from http://answers.unity3d.com/questions/944228/creating-a-smooth-round-flat-circle.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
//this circle is divdied up into pizza slices of triangles
public class CirclePlane : MonoBehaviour
{
    public int numOfPoints;

    public void Start()
    {
        float angleStep = 360.0f / (float)numOfPoints;
        List<Vector3> vertexList = new List<Vector3>();
        List<int> triangleList = new List<int>();
        Quaternion quaternion = Quaternion.Euler(0.0f, 0.0f, angleStep);
        // Make first triangle.
        vertexList.Add(new Vector3(0.0f, 0.0f, 0.0f));  // 1. Circle center.
        vertexList.Add(new Vector3(0.0f, 0.5f, 0.0f));  // 2. First vertex on circle outline (radius = 0.5f)
        vertexList.Add(quaternion * vertexList[1]);     // 3. First vertex on circle outline rotated by angle)
                                                        // Add triangle indices.
        triangleList.Add(0);
        triangleList.Add(1);
        triangleList.Add(2);
        for (int i = 0; i < numOfPoints - 1; i++)
        {
            triangleList.Add(0);                      // Index of circle center.
            triangleList.Add(vertexList.Count - 1);
            triangleList.Add(vertexList.Count);
            vertexList.Add(quaternion * vertexList[vertexList.Count - 1]);
        }

        GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh.vertices = vertexList.ToArray();
        GetComponent<MeshFilter>().sharedMesh.triangles = triangleList.ToArray();

        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;

        //transform.Rotate(new Vector3(0,180,0));
    }
}
