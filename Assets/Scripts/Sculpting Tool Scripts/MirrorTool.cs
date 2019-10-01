using UnityEngine;
using System.Collections;

/// <summary>
/// simply turns the mirror on and off
/// most of the mirror functionally is under the tools that use them
/// this means spline and line too for now
/// the Mirror tool just mimics these drawings on the other side when they are first created and does nothing else
/// a good idea is to bring mirror functionality away from the drawing tools and onto this so that this can be independent
/// that way, the mirroring can be updated to more functions such as mirroring a primitive spawning
/// </summary>
public class MirrorTool : ToolBase
{
    public MirrorScript mirrorScript;
    public GameObject theMirror;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();
        mirrorScript.gameObject.SetActive(false);
        trackerLetter = "M";
    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();

        if (!photonView.isMine)
            return;

        if(controller.triggerButtonDown)
        {
            photonView.RPC("FlipOnOff", PhotonTargets.AllBufferedViaServer);
        }

    }

    [PunRPC]
    public void FlipOnOff()
    {
        mirrorScript.gameObject.SetActive(!mirrorScript.gameObject.activeSelf);

        mirrorScript.theMirror = theMirror;

        if (mirrorScript.gameObject.activeSelf)
        {
            theMirror.transform.position = controller.transform.position;
            theMirror.transform.rotation = controller.transform.rotation;
            theMirror.transform.Rotate(90,90,0);
        }
    }
}
