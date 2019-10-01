//from https://github.com/karl-/pb_CSG

using UnityEngine;
using System.Collections;
using Parabox.CSG;
using System.Linq;

/// <summary>
/// is implementations for boolean operations that are handled from this outside class
/// </summary>
public class BooleanOperations : ToolBase
{
    GameObject first, second;
    GameObject composite;
    GameObject touched;
    Material firstMat;
    Color firstColor;
    Material secondMat;
    bool needReleased = false;

	enum BoolOp
	{
		Union,
		Subtract,
		Intersect
	};

    BoolOp boolType;

    protected override void Update()
    {
        base.Update();


        if(controller.triggerButtonPressed && !needReleased)
        {
            photonView.RPC("startDraw", PhotonTargets.AllBufferedViaServer);
        }

        if (controller.triggerButtonUp)
        {
            photonView.RPC("endDraw", PhotonTargets.AllBufferedViaServer);
        }
    }

    public void Union()
	{
        photonView.RPC("pickUnion", PhotonTargets.AllBufferedViaServer);
    }
    [PunRPC]
    void pickUnion()
    {
        boolType = BoolOp.Union ;
    }

    public void Subtraction()
    {
        photonView.RPC("pickSubtraction", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void pickSubtraction()
    {
        boolType = BoolOp.Subtract;
    }

    public void Intersection()
    {
        photonView.RPC("pickIntersection", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void pickIntersection()
    {
        boolType = BoolOp.Intersect;
    }

    [PunRPC]
    void startDraw()
    {
        if (first != null && first != touched && touched != null)
        {
            second = touched.gameObject;
            secondMat = second.GetComponent<MeshRenderer>().material;
            second.GetComponent<MeshRenderer>().material.color = Color.red;
            needReleased = true;
            Boolean(boolType);
        }
        else if (first == null && touched != null)
        {
            first = touched.gameObject;
            firstMat = first.GetComponent<MeshRenderer>().material;
            firstColor = firstMat.color;
            first.GetComponent<ObjectID>().ObjectColor = first.GetComponent<MeshRenderer>().material.color;
            first.GetComponent<MeshRenderer>().material.color = Color.blue;
        }
    }

    [PunRPC]
    void endDraw()
    {
        needReleased = false;
        if (first)
        {
            first.GetComponent<MeshRenderer>().material = firstMat;
            first = null;
        }
        if (second)
        {
            second.GetComponent<MeshRenderer>().material = secondMat;
            second = null;
        }
        firstMat = null;
        secondMat = null;
    }

    void Boolean(BoolOp operation)
	{
		Mesh m = new Mesh();

		switch(operation)
		{
			case BoolOp.Union:
				m = CSG.Union(first, second);
				break;

			case BoolOp.Subtract:
				m = CSG.Subtract(first, second);
				break;

			case BoolOp.Intersect:
				m = CSG.Intersect(first,second);
				break;
		}

		composite = new GameObject();
		composite.AddComponent<MeshFilter>().mesh = m;
		composite.AddComponent<MeshRenderer>().material = first.GetComponent<MeshRenderer>().material;

        GenerateBarycentric( composite );

	}

	void GenerateBarycentric(GameObject go)
	{
		Mesh m = go.GetComponent<MeshFilter>().mesh;

		if(m == null) return;

        int[] tris = m.triangles;
        int triangleCount = tris.Length;

        Vector3[] mesh_vertices = m.vertices;

        Vector3[] vertices = new Vector3[triangleCount];
        Vector3[] normals = m.normals;
        Vector2[] uv = m.uv;
        Color[] colors = Enumerable.Repeat(firstColor, triangleCount).ToArray();

        for (int i = 0; i < triangleCount; i++)
		{
			vertices[i] = mesh_vertices[tris[i]];
			tris[i] = i;
		}

		Mesh newMesh = new Mesh();

        newMesh.Clear();
        newMesh.vertices = vertices;
        newMesh.triangles = tris;
        newMesh.normals = normals;
        newMesh.colors = colors;
        newMesh.uv = uv;

        newMesh.colors = colors;

        Material tempMat = firstMat;
        go.GetComponent<MeshFilter>().mesh = newMesh;
        go.GetComponent<MeshRenderer>().material = tempMat;

        go.tag = "Trail";
        ObjectManager.instance.AddObject(go);

        go.GetComponent<ObjectID>().ObjectColor = firstColor;
        go.GetComponent<MeshRenderer>().material.color = go.GetComponent<ObjectID>().ObjectColor;

        GameObject.Destroy(first);
        GameObject.Destroy(second);
        first = null;
        second = null;
        touched = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == ("Trail"))
        {
            touched = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == ("Trail"))
        {
            touched = null;
        }
    }
}
