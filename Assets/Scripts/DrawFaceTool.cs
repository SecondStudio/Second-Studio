using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawFaceTool : ToolBase {

    Vector3[] Points;
    bool IsPlacing;
    int CurrentPoint = 0;
    public GameObject FacePrefab;
    public GameObject drawPoint;

    public LineRenderer Line;
    protected override void Start()
    {
        base.Start();
        Line.positionCount = 3;
        Line.enabled = false;
        Points = new Vector3[3];
        trackerLetter = "D";
    }
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
        if (IsPlacing)
        {
            Line.SetPosition(CurrentPoint, GridSnapTool.Snap(drawPoint.transform.position));
        }

        if (controller.triggerButtonDown)
        {
            photonView.RPC("PlacePoint", PhotonTargets.AllBufferedViaServer);
        }
	}


    [PunRPC]
    void PlacePoint()
    {
        if (!IsPlacing)
        {
            IsPlacing = true;
            CurrentPoint = 0;
            Line.enabled = true;
        }
        Points[CurrentPoint] = GridSnapTool.Snap(drawPoint.transform.position);
        for(int i = CurrentPoint; i < 3; i++)
        {
            Line.SetPosition(i, GridSnapTool.Snap(drawPoint.transform.position));
        }
       
        CurrentPoint++;
        if(CurrentPoint == 3)
        {
            IsPlacing = false;
            FinishShape();
        }
    }

    void FinishShape()
    {
        var tri = Instantiate(FacePrefab , ObjectManager.instance.transform);
        var renderer = tri.GetComponent<MeshRenderer>();
        var filter = tri.GetComponent<MeshFilter>();
        var collider = tri.GetComponent<MeshCollider>();
        var editor = tri.GetComponent<MeshEditor>();
        collider.convex = false;
        var mesh = new Mesh();
        tri.transform.position = (Points[0] + Points[1] + Points[2]) / 3; //Center the mesh
        for (int i = 0; i < 3; i++)
        {
            Points[i] = tri.transform.InverseTransformPoint(Points[i]);
        }
        mesh.vertices = Points;
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        filter.sharedMesh = mesh;
        collider.sharedMesh = mesh;
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.white;
        ObjectManager.instance.AddObject(tri);

        if(Vector3.Dot(tri.transform.TransformDirection(mesh.normals[0]) , controller.Head.transform.forward) > 0)
        {
            mesh.triangles = new int[] { 2, 1, 0 };
            mesh.RecalculateNormals();
            filter.sharedMesh = mesh;
            collider.sharedMesh = mesh;
        }
        editor.StartGroupGeneration();
        collider.convex = false;
        Line.enabled = false;
    }
}
