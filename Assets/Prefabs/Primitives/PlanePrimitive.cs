//from http://answers.unity3d.com/questions/944228/creating-a-smooth-round-flat-circle.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
//simply two triangles put together
public class PlanePrimitive : MonoBehaviour
{

    public void Start()
    {
        List<Vector3> vertexList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        vertexList.Add(new Vector3(-.5f, .5f, 0));
        vertexList.Add(new Vector3(.5f, .5f, 0));
        vertexList.Add(new Vector3(.5f, -.5f, 0));
        vertexList.Add(new Vector3(-.5f, -.5f, 0));


        triangleList.Add(0);
        triangleList.Add(1);
        triangleList.Add(3);

        triangleList.Add(1);
        triangleList.Add(2);
        triangleList.Add(3);

        GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh.vertices = vertexList.ToArray();
        GetComponent<MeshFilter>().sharedMesh.triangles = triangleList.ToArray();

        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }
}
