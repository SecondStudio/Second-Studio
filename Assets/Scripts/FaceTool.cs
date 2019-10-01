using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vertex = MeshEditor.Vertex;
using VertexGroup = MeshEditor.VertexGroup;
public class FaceTool : ToolBase
{
    public GameObject trail;
    protected Dictionary<VertexGroup, Vector3> VertexOffsets;
    FixedJoint joint;
    Rigidbody attachPoint;
    public SteamVR_TrackedObject trackedObj;
    protected GameObject[] trailList;
    public LineRenderer vis;
    protected Vector3 HitPoint;
    protected GameObject tri;
    List<GameObject> Faces;
    public List<MeshEditor.Face> HeldFaces;
    public FaceTool other;
    float InitialDistance;
    bool IsScaling = false, IsHolding = false;
    public Material OutlineMaterial;
    public SteamVR_Controller.Device device { get {return SteamVR_Controller.Input((int)trackedObj.index); } }

    protected override void OnEnable()
    { 
        base.OnEnable();
        //foreach (var e in FindObjectsOfType<MeshEditor>()) StartCoroutine(e.DrawFaces());
        trailList = GameObject.FindGameObjectsWithTag("Trail");
        foreach (GameObject gobject in trailList)
        {
            if (gobject.GetComponent<MeshEditor>() != null)
            {
                gobject.GetComponent<MeshEditor>().enabled = false;
                gobject.GetComponent<MeshEditor>().enabled = true;
            }
        }

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
    }


    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        Faces = new List<GameObject>();
    }


    
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (controller.gripButtonDown)
        {
            
            if (other && other.IsHolding) 
            {
                photonView.RPC("StartScaling", PhotonTargets.AllBufferedViaServer, transform.position);
            } else
            {
                photonView.RPC("Grab", PhotonTargets.AllBufferedViaServer, transform.position);
            }
            
        }
        else if (controller.gripButtonPressed && !other.IsScaling)
        {
            if (IsScaling)
            {
                photonView.RPC("SetScale", PhotonTargets.AllBufferedViaServer);
            } else
            {
                photonView.RPC("SetTransform", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles);
            }
            
        }
        else if (controller.gripButtonUp)
        {
            if (IsScaling)
            {
                photonView.RPC("StopScaling", PhotonTargets.AllBufferedViaServer);
            } else
            {
                photonView.RPC("LetGo", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles, device.velocity, device.angularVelocity);
            }
            
        }

    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Trail"))
        {
            trail = other.gameObject;
        }
    }

    [PunRPC]
    protected virtual void LetGo(Vector3 pos, Vector3 angles, Vector3 velocity, Vector3 angularVelocity)
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        VertexOffsets.Clear();
        FindObjectOfType<MeshIllustrator>().DrawSelection = true;
        IsHolding = false;
        
    }


    [PunRPC]
    protected virtual void Grab(Vector3 position)
    {
        if (VertexOffsets != null) VertexOffsets.Clear();
        else VertexOffsets = new Dictionary<VertexGroup, Vector3>();
        if (HeldFaces != null) HeldFaces.Clear();
        else HeldFaces = new List<MeshEditor.Face>();
        FindObjectOfType<MeshIllustrator>().DrawSelection = false ;
        IsHolding = true;

        foreach(var e in FindObjectOfType<SelectionTool>().Selection)
        {
            if(e is MeshEditor.Face)
            {
                HeldFaces.Add(e as MeshEditor.Face);
            }
        }
        foreach (var f in HeldFaces)
        {
            foreach(VertexGroup h in f.Groups)
            {
                if (VertexOffsets.ContainsKey(h)) continue;
                VertexOffsets.Add(h, h.WorldPosition - transform.position);
            }
            
        }

        
    }

    [PunRPC]
    protected virtual void SetTransform(Vector3 pos, Vector3 angles)
    {
        MeshEditor editor = null;
        int i = 0;
        var select = FindObjectOfType<SelectionTool>();
        select.Selection.Clear();
        foreach(var f in HeldFaces)
        {
            select.Selection.Add(f);
        }
        foreach (var vert in VertexOffsets)
        {
            vert.Key.WorldPosition = transform.TransformPoint(pos) + vert.Value;
            editor = vert.Key.Verts[0].Editor;
            editor.UpdateVertex(vert.Key);
            i++;
        }
        if (editor)
        {
            editor.UpdateMesh();
        }
        
    }

    protected void DrawFace(Transform trans, VertexGroup[] handles)
    {
        var points = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            points[i] = handles[i].WorldPosition;
        }
        if (tri) Destroy(tri);
        tri = new GameObject();
        var renderer = tri.AddComponent<MeshRenderer>();
        var filter = tri.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        mesh.vertices = points;
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2 };
        filter.mesh = mesh;
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.yellow;

    }

    [PunRPC]
    protected void StartScaling( Vector3 position)
    {
        InitialDistance = Vector3.Distance(controller.transform.position, controller.otherController.transform.position);
        IsScaling = true;
    }
    [PunRPC]
    protected void StopScaling()
    {
        HeldFaces.Clear();
        IsScaling = false;
        other.LetGo(other.transform.position, other.transform.rotation.eulerAngles, other.device.velocity, other.device.angularVelocity);
    }
    [PunRPC]
    protected void SetScale()
    {
        var select = FindObjectOfType<SelectionTool>();
        select.Selection.Clear();
        foreach (var f in HeldFaces)
        {
            
            f.Scale(Vector3.Distance(controller.transform.position, controller.otherController.transform.position) / InitialDistance);
            select.Selection.Add(f);
        }
    }

}



/*RaycastHit hit;
        var dir = (TargetObject.transform.position - transform.position).normalized;
        vis.positionCount = 2;
        vis.SetPosition(0, transform.position);
        vis.SetPosition(1, TargetObject.transform.position);
        TargetObject.GetComponent<MeshCollider>().convex = false;
        Physics.Raycast(new Ray(transform.position, dir), out hit , 1<<8);
        TargetObject.GetComponent<MeshCollider>().convex = true;
        Debug.LogWarning("Hit object: " + hit.transform + " at Triangle index: " + hit.triangleIndex);
        if (hit.triangleIndex == -1) return;
        var mesh = TargetObject.GetComponent<MeshFilter>().mesh;
        
        handlesHeld.Add(TargetObject.GetComponent<MeshEditor>().HandleForPosition[mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 0]]].gameObject, TargetObject.GetComponent<MeshEditor>().HandleForPosition[mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 0]]].transform.position - transform.position);
        handlesHeld.Add(TargetObject.GetComponent<MeshEditor>().HandleForPosition[mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 1]]].gameObject, TargetObject.GetComponent<MeshEditor>().HandleForPosition[mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 1]]].transform.position - transform.position);
        handlesHeld.Add(TargetObject.GetComponent<MeshEditor>().HandleForPosition[mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 2]]].gameObject, TargetObject.GetComponent<MeshEditor>().HandleForPosition[mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 2]]].transform.position - transform.position);*/
