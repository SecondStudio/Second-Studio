using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTool : Photon.MonoBehaviour {

    private void DisableAll()
    {
        GetComponentInChildren<FaceTool>().enabled = false;
        GetComponentInChildren<EdgeTool>().enabled = false;
        GetComponentInChildren<VertexTool>().enabled = false;
    }


    public void UseVertexTool()
    {
        photonView.RPC("UseVertex", PhotonTargets.AllBufferedViaServer);
    }

    public void UseFaceTool()
    {
        photonView.RPC("UseFace", PhotonTargets.AllBufferedViaServer);
    }
    public void UseEdgeTool()
    {
        photonView.RPC("UseEdge", PhotonTargets.AllBufferedViaServer);
    }
    [PunRPC]
    void UseFace()
    {
        DisableAll();
        GetComponentInChildren<FaceTool>().enabled = true;
        
    }

    [PunRPC]
    void UseEdge()
    {
        DisableAll();
        GetComponentInChildren<EdgeTool>().enabled = true;

    }

    [PunRPC]
    void UseVertex()
    {
        DisableAll();
        GetComponentInChildren<VertexTool>().enabled = true;

    }
}
