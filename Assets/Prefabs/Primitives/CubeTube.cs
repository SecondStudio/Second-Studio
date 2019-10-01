//from http://answers.unity3d.com/questions/944228/creating-a-smooth-round-flat-circle.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
//a trapezoid per side that are divided into three triangles
public class CubeTube : MonoBehaviour
{

    public void Start()
    {
        List<Vector3> vertexList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        vertexList.Add(new Vector3(-.5f, -.5f, 0));
        vertexList.Add(new Vector3(-.48f, -.48f, 0));
        vertexList.Add(new Vector3(0, -.5f, 0));
        vertexList.Add(new Vector3(.48f, -.48f, 0));
        vertexList.Add(new Vector3(.5f,-.5f, 0));
        vertexList.Add(new Vector3(.5f, 0, 0));
        vertexList.Add(new Vector3(.48f, .48f, 0));
        vertexList.Add(new Vector3(.5f, .5f, 0));
        vertexList.Add(new Vector3(0f, .5f, 0));
        vertexList.Add(new Vector3(-.48f, .48f, 0));
        vertexList.Add(new Vector3(-.5f, .5f, 0));
        vertexList.Add(new Vector3(-.5f, 0, 0));


        triangleList.Add(0);
        triangleList.Add(1);
        triangleList.Add(2);

        triangleList.Add(1);
        triangleList.Add(3);
        triangleList.Add(2);

        triangleList.Add(2);
        triangleList.Add(3);
        triangleList.Add(4);

        triangleList.Add(4);
        triangleList.Add(3);
        triangleList.Add(5);

        triangleList.Add(3);
        triangleList.Add(6);
        triangleList.Add(5);

        triangleList.Add(7);
        triangleList.Add(5);
        triangleList.Add(6);

        triangleList.Add(6);
        triangleList.Add(8);
        triangleList.Add(7);

        triangleList.Add(6);
        triangleList.Add(9);
        triangleList.Add(8);

        triangleList.Add(8);
        triangleList.Add(9);
        triangleList.Add(10);

        triangleList.Add(9);
        triangleList.Add(11);
        triangleList.Add(10);

        triangleList.Add(11);
        triangleList.Add(9);
        triangleList.Add(1);

        triangleList.Add(11);
        triangleList.Add(1);
        triangleList.Add(0);



        GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh.vertices = vertexList.ToArray();
        GetComponent<MeshFilter>().sharedMesh.triangles = triangleList.ToArray();

        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }
}
