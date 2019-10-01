//from http://answers.unity3d.com/questions/944228/creating-a-smooth-round-flat-circle.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
//this is created by having many trapezoids combined going around the circle.
//these trapezoids are divided into 3 triangles

public class CircleTube : MonoBehaviour
{
    public int numOfPoints;

    public void Start()
    {
        float angleStep = 360.0f / (float)numOfPoints;
        List<Vector3> vertexList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        // Make first triangle. 
        vertexList.Add(new Vector3(0.0f, 0.48f, 0.0f));   
        vertexList.Add(new Vector3(0.0f, 0.5f, 0.0f));


        for (int i = 0; i < numOfPoints; i++)
        {
            vertexList.Add(Quaternion.Euler(0f, 0f, (i+.5f) * angleStep) * vertexList[1]);
            vertexList.Add(Quaternion.Euler(0f, 0f, (i+1) * angleStep) * vertexList[0]);
            vertexList.Add(Quaternion.Euler(0f, 0f, (i+1) * angleStep) * vertexList[1]);

            triangleList.Add(0 + 3 * i);
            triangleList.Add(1 + 3 * i);
            triangleList.Add(2 + 3 * i);

            triangleList.Add(3 + 3 * i);
            triangleList.Add(2 + 3 * i);
            triangleList.Add(4 + 3 * i);

            triangleList.Add(0 + 3 * i);
            triangleList.Add(2 + 3 * i);
            triangleList.Add(3 + 3 * i);
        }



        GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh.vertices = vertexList.ToArray();
        GetComponent<MeshFilter>().sharedMesh.triangles = triangleList.ToArray();

        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }
}
