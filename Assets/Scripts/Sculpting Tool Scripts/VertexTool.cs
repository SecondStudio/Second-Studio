using UnityEngine;
using System.Collections.Generic;

using VertexGroup = MeshEditor.VertexGroup;
public class VertexTool : ToolBase
{
    public GameObject obj;
    List<VertexGroup> HeldVerts;
    FixedJoint joint;
    Rigidbody attachPoint;
    public SteamVR_TrackedObject trackedObj;
    GameObject[] trailList;
    Transform prevParent;
    GameObject currentObject;
    public Material OutlineMaterial;
    public VertexTool other;
    Dictionary<VertexGroup, Vector3> Offsets;

    protected override void OnEnable()
    {
        base.OnEnable();

        trailList = GameObject.FindGameObjectsWithTag("Trail");
        foreach (GameObject gobject in trailList)
        {
            if(gobject.GetComponent<MeshEditor>() != null)
            {
                gobject.GetComponent<MeshEditor>().enabled = false;
                gobject.GetComponent<MeshEditor>().enabled = true;
            }
            
        }
        foreach (var t in GameObject.FindGameObjectsWithTag("Trail"))
        {
            if (t.GetComponent<ObjectID>() == null) continue;
            t.GetComponent<ObjectID>().OutlineRenderer.material = OutlineMaterial;
            t.GetComponent<ObjectID>().OutlineRenderer.enabled = true;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        var device = SteamVR_Controller.Input((int)trackedObj.index);

        if (controller.gripButtonDown)
        {
            photonView.RPC("Grab", PhotonTargets.AllBufferedViaServer, transform.position);
        }
        else if (controller.gripButtonPressed && HeldVerts != null)
        {
            photonView.RPC("SetTransform", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles);
        }
        else if (controller.gripButtonUp && HeldVerts != null)
        {
            photonView.RPC("LetGo", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles, device.velocity, device.angularVelocity);
        }
        if (obj != null)
        { 
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Trail"))
        {
            obj = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != obj) return;
        obj = null;
    }

    [PunRPC]
    void LetGo(Vector3 pos, Vector3 angles, Vector3 velocity, Vector3 angularVelocity)
    {
        HeldVerts[0].Editor.UpdateFaces();
        HeldVerts.Clear();
        FindObjectOfType<MeshIllustrator>().DrawSelection = true;
    }

    [PunRPC]
    void Grab(Vector3 position)
    {
        Offsets = new Dictionary<VertexGroup, Vector3>();
        HeldVerts = new List<VertexGroup>();
        FindObjectOfType<MeshIllustrator>().DrawSelection = false;
        foreach (var v in FindObjectOfType<SelectionTool>().Selection)
        {
            if(v is VertexGroup)
            {
                HeldVerts.Add(v as VertexGroup);

                Offsets.Add(v as VertexGroup, (v as VertexGroup).WorldPosition - transform.position);
            }
        }
    }

    [PunRPC]
    void SetTransform(Vector3 pos, Vector3 angles)
    {
        foreach(var vert in HeldVerts)
        {
            vert.WorldPosition = pos + Offsets[vert];
            vert.Verts[0].Editor.UpdateVertex(vert);
            vert.Verts[0].Editor.UpdateMesh();
        }

    }
}
