using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex = MeshEditor.Vertex;
using VertexGroup = MeshEditor.VertexGroup;
public class EdgeTool : ToolBase {

     
    public GameObject trail;
    Dictionary<VertexGroup, Vector3> VertexOffsets;
    FixedJoint joint;
    Rigidbody attachPoint;
    public SteamVR_TrackedObject trackedObj;
    GameObject[] trailList;
    public LineRenderer vis;
    Vector3 HitPoint;
    GameObject CurrentObject;
    VertexGroup[] HeldVerts;
    public EdgeTool other;
    List<MeshEditor.Edge> heldEdges;
    bool IsScaling , IsHolding;
    float InitialDistance;
    public Material OutlineMaterial;
    public SteamVR_Controller.Device device { get { return SteamVR_Controller.Input((int)trackedObj.index); } }

    protected override void OnEnable()
    {
        base.OnEnable();
        //foreach (var e in FindObjectsOfType<MeshEditor>()) StartCoroutine(e.DrawEdges());
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
        StopAllCoroutines();
        base.OnDisable();
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (controller.gripButtonDown && trail)
        {
            if (other && other.IsHolding)
            {
                photonView.RPC("StartScaling", PhotonTargets.AllBufferedViaServer, trail.GetComponent<ObjectID>().id, transform.position);
            }
            else
            {
                photonView.RPC("Grab", PhotonTargets.AllBufferedViaServer, transform.position);
            }

        }
        else if (controller.gripButtonPressed && !other.IsScaling)
        {
            if (IsScaling)
            {
                photonView.RPC("SetScale", PhotonTargets.AllBufferedViaServer);
            }
            else
            {
                photonView.RPC("SetTransform", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles);
            }

        }
        else if (controller.gripButtonUp)
        {
            if (IsScaling)
            {
                photonView.RPC("StopScaling", PhotonTargets.AllBufferedViaServer);
            }
            else
            {
                photonView.RPC("LetGo", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles, device.velocity, device.angularVelocity);
            }

        }
    }

    public void HandleToggle()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Trail"))
        {
            trail = other.gameObject;
        }
    }

    [PunRPC]
    void LetGo(Vector3 pos, Vector3 angles, Vector3 velocity, Vector3 angularVelocity)
    {
        heldEdges[0].Editor.UpdateFaces();
        IsHolding = false;
        VertexOffsets.Clear();
        FindObjectOfType<MeshIllustrator>().DrawSelection = true;
    }

    [PunRPC]
    void Grab(Vector3 position)
    {
        IsHolding = true;
        if (VertexOffsets != null) VertexOffsets.Clear();
        else VertexOffsets = new Dictionary<VertexGroup, Vector3>();
        if (heldEdges != null) heldEdges.Clear();
        else heldEdges = new List<MeshEditor.Edge>();
        FindObjectOfType<MeshIllustrator>().DrawSelection = true;
        foreach (var e in FindObjectOfType<SelectionTool>().Selection)
        {
            if(e is MeshEditor.Edge)
            {
                heldEdges.Add(e as MeshEditor.Edge);
            }
        }
        foreach(var e in heldEdges)
        {
            foreach (var h in e.Groups)
            {
                if (VertexOffsets.ContainsKey(h)) continue;
                VertexOffsets.Add(h, transform.InverseTransformPoint(h.WorldPosition) - transform.localPosition);
            }
        }
        

    }

    [PunRPC]
    void SetTransform(Vector3 pos, Vector3 angles)
    {
        MeshEditor editor = null;
        MeshEditor last = null;
        int i = 0;
        FindObjectOfType<SelectionTool>().Selection.Clear();
        foreach(var e in heldEdges)
        {
            FindObjectOfType<SelectionTool>().Selection.Add(e);
        }
        foreach (var vert in VertexOffsets)
        {
            vert.Key.WorldPosition = transform.TransformPoint(vert.Value + pos);
            editor = vert.Key.Verts[0].Editor;
            editor.UpdateVertex(vert.Key);
            i++;
            if(editor && editor != last)
            {
                editor.UpdateMesh();
                last = editor;
            }
        }

    }

    void DrawEdge (Transform trans, VertexGroup[] groups)
    {
        HeldVerts = groups;
        var points = new Vector3[2];
        for (int i = 0; i < 2; i++)
        {
            points[i] = groups[i].WorldPosition;
        }

        vis.SetPosition(0, points[0]);
        vis.SetPosition(1, points[1]);

    }

    void UpdateEdge()
    {
        var points = new Vector3[2];
        for (int i = 0; i < 2; i++)
        {
            points[i] = HeldVerts[i].WorldPosition;
        }

        vis.SetPosition(0, points[0]);
        vis.SetPosition(1, points[1]);
    }

    [PunRPC]
    protected void StartScaling(int id, Vector3 position)
    {
        InitialDistance = Vector3.Distance(controller.transform.position, controller.otherController.transform.position);
        if (VertexOffsets != null) VertexOffsets.Clear();
        else VertexOffsets = new Dictionary<VertexGroup, Vector3>();

        foreach (var e in FindObjectOfType<SelectionTool>().Selection)
        {
            if (e is MeshEditor.Edge)
            {
                heldEdges.Add(e as MeshEditor.Edge);
            }
        }
        foreach (var e in heldEdges)
        {
            foreach (var h in e.Groups)
            {
                if (VertexOffsets.ContainsKey(h)) continue;
                VertexOffsets.Add(h, transform.InverseTransformPoint(h.WorldPosition) - transform.localPosition);
            }
        }
        IsScaling = true;
        other.vis.enabled = false;
    }
    [PunRPC]
    protected void StopScaling()
    {
        heldEdges = null;
        IsScaling = false;
        other.LetGo(other.transform.position, other.transform.rotation.eulerAngles, other.device.velocity, other.device.angularVelocity);
    }
    [PunRPC]
    protected void SetScale()
    {
        foreach(var e in heldEdges)
        {
            e.Scale(Vector3.Distance(controller.transform.position, controller.otherController.transform.position) / InitialDistance);
        }
    }
}
