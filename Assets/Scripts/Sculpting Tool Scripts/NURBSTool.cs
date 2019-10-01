using UnityEngine;
using System.Collections;

/// <summary>
/// looks for hairs, whether it is created from the spline or the line
/// it is then dragged out to create a quad.
/// once the quad is made, can be dragged out again to create a volume
/// after that, using it again will just edit the volume
/// </summary>
public class NURBSTool: ToolBase
{
    //public ObjectType objectType = ObjectType.Hair;
    GameObject TargetObject , CollidingObject;
    private bool IsDragging , IsDraggingQuad;
    float timeLeft = 0.0f;
    float timeSizeLeft = 0.0f;
    private bool curveExtrusion = false;
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        trackerLetter = "X";
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!photonView.isMine)
            return;

        if (timeLeft > 0)
            timeLeft -= Time.deltaTime;

        if (timeSizeLeft > 0)
            timeSizeLeft -= Time.deltaTime;

        if (controller.appButtonDown)
        {
            curveExtrusion = !curveExtrusion;
        }

        else if (controller.triggerButtonDown)
        {
            if (CollidingObject != null) //Has a collision with a spline
            {
                TargetObject = CollidingObject;
                if (TargetObject.GetComponent<NURBSTrail>())
                {
                    if (TargetObject.GetComponent<NURBSTrail>().isQuad)
                    {
                        photonView.RPC("StartDraggingQuad", PhotonTargets.AllBufferedViaServer);
                    }
                    else
                    {
                        photonView.RPC("StartDragging", PhotonTargets.AllBufferedViaServer, curveExtrusion);
                    }
                }
            }
        }
        else if (controller.triggerButtonPressed && timeLeft <= 0)
        {
            if (IsDragging)
            {
                photonView.RPC("UpdateDrag", PhotonTargets.AllBufferedViaServer, curveExtrusion);

            }
            else if (IsDraggingQuad) {
                photonView.RPC("UpdateDragQuad", PhotonTargets.AllBufferedViaServer);
            }

        }
        else if (controller.triggerButtonUp)
        {
            if (IsDragging)
            {
                photonView.RPC("StopDragging", PhotonTargets.AllBufferedViaServer, curveExtrusion);
                
            } else if (IsDraggingQuad)
            {
                photonView.RPC("StopDraggingQuad", PhotonTargets.AllBufferedViaServer);
            }
        }
    }

    [PunRPC]
    void StartDragging(bool curved)
    {
        IsDragging = true;
        var refTrail = TargetObject.GetComponent<NURBSTrail>();
        refTrail.SetInitPos(controller.transform.position);
        refTrail.UpdateQuadDir(controller.transform.position, curved);
    }

    [PunRPC]
    void UpdateDrag(bool curved)
    {
        var refTrail = TargetObject.GetComponent<NURBSTrail>();
        refTrail.UpdateQuadDir(controller.transform.position, curved);
    }

    [PunRPC]
    void StopDragging(bool curved)
    {
        var refTrail = TargetObject.GetComponent<NURBSTrail>();
        refTrail.SetQuadDir(controller.transform.position, curved);
        var mesh = TargetObject.GetComponent<MeshFilter>().mesh;
        TargetObject.GetComponent<ObjectID>().OutlineRenderer.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        TargetObject.GetComponent<NURBSTrail>().isQuad = true;
        TargetObject = null;
        IsDragging = false;
    }

    [PunRPC]
    void StartDraggingQuad()
    {
        IsDraggingQuad = true;
        var refTrail = TargetObject.GetComponent<NURBSTrail>();
    }

    [PunRPC]
    void UpdateDragQuad()
    {
        var refTrail = TargetObject.GetComponent<NURBSTrail>();
        refTrail.UpdateVolumeDir(controller.transform.position);
    }

    [PunRPC]
    void StopDraggingQuad()
    {
        var refTrail = TargetObject.GetComponent<NURBSTrail>();
        refTrail.SetVolumeDir(controller.transform.position);
        var mesh = TargetObject.GetComponent<MeshFilter>().mesh;
        TargetObject.GetComponent<ObjectID>().OutlineRenderer.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        TargetObject = null;
        IsDraggingQuad = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.StartsWith("NURBS"))
        {//TODO: Hacky AF
            CollidingObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.StartsWith("NURBS"))
            CollidingObject = null;
    }

    public void ToggleCurve()
    {
        photonView.RPC("Toggle", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void Toggle()
    {
        curveExtrusion = !curveExtrusion;
    }
    #region CONSTRIANTS

    #endregion
}
