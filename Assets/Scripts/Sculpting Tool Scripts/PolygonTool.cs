using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// follows the controller in a plane and builds a flat mesh out of it
/// </summary>
public class PolygonTool : ToolBase
{
    public GameObject polygonPrefab;
    List<Vector3> vertexList;
    List<int[]> triangleList;
    Vector3 startPos;
    Vector3 currentPos;
    float startingX;
    float startingY;
    float startingZ;
    GameObject thisPoly;
    public LineRenderer line;

    bool constrainX = true;
    bool constrainY = false;
    bool constrainZ = false;

    protected override void Start()
    {
        base.Start();
        trackerLetter = "O";
    }

    protected override void Update()
    {
        base.Update();

        if (controller.triggerButtonDown)
        {
            photonView.RPC("startDrawing", PhotonTargets.AllBufferedViaServer);
        }
        else if (controller.triggerButtonPressed)
        {
            photonView.RPC("keepDrawing", PhotonTargets.AllBufferedViaServer);
        }        
        if(controller.triggerButtonUp)
        {
            photonView.RPC("endDrawing", PhotonTargets.AllBufferedViaServer);
        }
    }

    [PunRPC]
    public void startDrawing()
    {
        vertexList = new List<Vector3>();
        triangleList = new List<int[]>();
        startPos = controller.transform.position;
        currentPos = startPos;
        startingX = startPos.x;
        startingY = startPos.y;
        startingZ = startPos.z;
    }

    [PunRPC]
    public void keepDrawing()
    {
        if (constrainX && Vector3.Distance(getX(controller.transform.position), getX(currentPos)) > .1f)
        {
            vertexList.Add(getX(currentPos));
            currentPos = getX(controller.transform.position);
            line.positionCount = vertexList.Count;
            line.SetPositions(vertexList.ToArray());
        }
        if (constrainY && Vector3.Distance(getY(controller.transform.position), getY(currentPos)) > .1f)
        {
            vertexList.Add(getY(currentPos));
            currentPos = getY(controller.transform.position);
            line.positionCount = vertexList.Count;
            line.SetPositions(vertexList.ToArray());
        }
        if (constrainZ && Vector3.Distance(getZ(controller.transform.position), getZ(currentPos)) > .1f)
        {
            vertexList.Add(getZ(currentPos));
            currentPos = getZ(controller.transform.position);
            line.positionCount = vertexList.Count;
            line.SetPositions(vertexList.ToArray());
        }
    }

    [PunRPC]
    public void endDrawing()
    {
        currentPos = startPos;
        //vertexList.Add(getX(currentPos));

        List<int> vertsLeft = new List<int>();
        for (int v = 0; v < vertexList.Count; v++)
        {
            vertsLeft.Add(v);
        }

        int i = 0;
        while (vertsLeft.Count > 3) // while there are more than 3 points left
        {
            triangleList.Add(new int[] { vertsLeft[i % vertsLeft.Count], vertsLeft[(i + 1) % vertsLeft.Count], vertsLeft[(i + 2) % vertsLeft.Count] });
            vertsLeft.Remove(vertsLeft[(i + 1) % vertsLeft.Count]);
            i = i + 2 % vertsLeft.Count;
        }
        i = 0;
        triangleList.Add(new int[] { vertsLeft[i % vertsLeft.Count], vertsLeft[(i + 1) % vertsLeft.Count], vertsLeft[(i + 2) % vertsLeft.Count] });

        List<int> tris = new List<int>();
        foreach (var t in triangleList)
        {
            foreach (var p in t)
            {
                tris.Add(p);
            }
            thisPoly = Instantiate(polygonPrefab, startPos, Quaternion.identity);
        }
        for (int j = 0; j < vertexList.Count; j++)
        {
            vertexList[j] = thisPoly.transform.InverseTransformPoint(vertexList[j]);
        }

        thisPoly.GetComponent<MeshFilter>().mesh.vertices = vertexList.ToArray();
        thisPoly.GetComponent<MeshFilter>().mesh.triangles = tris.ToArray();
        thisPoly.GetComponent<MeshCollider>().sharedMesh = thisPoly.GetComponent<MeshFilter>().mesh;
        var renderer = thisPoly.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.white;
        ObjectManager.instance.AddObject(thisPoly);
        var mesh = thisPoly.GetComponent<MeshFilter>().mesh;
        mesh.RecalculateNormals();
        if (Vector3.Dot(thisPoly.transform.TransformDirection(mesh.normals[0]), controller.Head.transform.forward) > 0)
        {
            var ts = mesh.triangles;
            for (int j = 0; j < ts.Length; j += 3)
            {
                int temp = ts[j + 2];
                ts[j + 2] = ts[j];
                ts[j] = temp;
            }
            mesh.triangles = ts;
            mesh.RecalculateNormals();
            thisPoly.GetComponent<MeshFilter>().mesh = mesh;
            thisPoly.GetComponent<MeshCollider>().sharedMesh = thisPoly.GetComponent<MeshFilter>().mesh = mesh;
        }
        thisPoly.GetComponent<MeshEditor>().StartGroupGeneration();
        vertexList.Clear();
        triangleList.Clear();
        line.positionCount = 0;
    }


    Vector3 getX(Vector3 inVec)
    {
        Vector3 returnVec = inVec;
        returnVec.x = startingX;
        return returnVec;
    }

    Vector3 getY(Vector3 inVec)
    {
        Vector3 returnVec = inVec;
        returnVec.y = startingY;
        return returnVec;
    }

    Vector3 getZ(Vector3 inVec)
    {
        Vector3 returnVec = inVec;
        returnVec.z = startingZ;
        return returnVec;
    }

    public void ConstraintX()
    {
        photonView.RPC("constraintX", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void constraintX()
    {
        constrainX = true;
        constrainY = false;
        constrainZ = false;
    }

    public void ConstraintY()
    {
        photonView.RPC("constraintY", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void constraintY()
    {
        constrainX = false;
        constrainY = true;
        constrainZ = false;
    }

    public void ConstraintZ()
    {
        photonView.RPC("constraintZ", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void constraintZ()
    {
        constrainX = false;
        constrainY = false;
        constrainZ = true;
    }
}
