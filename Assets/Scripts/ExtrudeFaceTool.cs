using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtrudeFaceTool : FaceTool {

    protected override void Start()
    {
        base.Start();
        isHeld = false;
        trackerLetter = "E";
    }
    protected override void Update()
    {

        if (controller.triggerButtonDown)
        {
                photonView.RPC("Grab", PhotonTargets.AllBufferedViaServer, transform.position);
        }
        else if (controller.triggerButtonPressed)
        {
                photonView.RPC("SetTransform", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles);
        }
        else if (controller.triggerButtonUp)
        {
            photonView.RPC("LetGo", PhotonTargets.AllBufferedViaServer, transform.position, transform.rotation.eulerAngles, device.velocity, device.angularVelocity);
        }

    }

    [PunRPC]
    protected override void Grab(Vector3 position)
    {
        if (VertexOffsets != null) VertexOffsets.Clear();
        else VertexOffsets = new Dictionary<MeshEditor.VertexGroup, Vector3>();
        if (HeldFaces != null) HeldFaces.Clear();
        else HeldFaces = new List<MeshEditor.Face>();

        foreach(var e in FindObjectOfType<SelectionTool>().Selection)
        {
            if(e is MeshEditor.Face)
            {
                foreach(var g in e.Editor.ExtrudeFace(e as MeshEditor.Face))
                {
                    if (VertexOffsets.ContainsKey(g)) return;
                    VertexOffsets.Add(g, g.WorldPosition - transform.position);
                }
            }
        }
    }

    [PunRPC]
    protected override void SetTransform(Vector3 pos, Vector3 angles)
    {
        base.SetTransform(pos, angles);
    }

    [PunRPC]
    protected override void LetGo(Vector3 pos, Vector3 angles, Vector3 velocity, Vector3 angularVelocity)
    {
        MeshEditor editor = null;
        foreach(var v in VertexOffsets)
        {
            editor = v.Key.Editor;
        }
        editor.ConvertToSharedVerts(true);
        base.LetGo(pos, angles, velocity, angularVelocity);
    }
}
