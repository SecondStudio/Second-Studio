using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// uses onePlaneBSP shader to show clipped objects
/// Able to see the interior of objects that are touching
/// does so by creating a clipping plane and applying clipping to the position of the plane
/// however, it may look weird when the plane is beyond an object and therefor not touching it, so the object remains clearly visible
/// despite the clipping plane going beyond it. this may need a fix
/// </summary>
public class ClippingTool : ToolBase
{
    public float ClippingDistance = 0f;
    public Text distanceText;
    public GameObject clippingPlane;
    bool planeOn;

    float originalClip;


    protected override void Start()
    {
        base.Start();
        //originalClip = mainCamera.GetComponent<Camera>().nearClipPlane;
        clippingPlane.transform.Translate(0, 0.35f, 0);
        trackerLetter = "K";
    }

    protected override void Update()
    {
        base.Update();
        if (controller.triggerButtonDown)
        {
            photonView.RPC("PlaneToggle", PhotonTargets.AllBufferedViaServer);
        }

        clippingPlane.transform.position = controller.transform.position + -controller.transform.up * ClippingDistance;
        clippingPlane.transform.rotation = controller.transform.rotation;


        distanceText.text = ClippingDistance.ToString("#.##");

        if (!isHeld)
            planeOn = false;

        if (planeOn)
        {
            clippingPlane.SetActive(true);
        }
        else
        {
            clippingPlane.SetActive(false);
        }
    }

    protected override void OnDisable()
    {
        planeOn = false;
        clippingPlane.GetComponent<ClippingScript>().Disable();
        clippingPlane.SetActive(false);
        clippingPlane.GetComponent<ClippingScript>().enabled = false;
    }

    public void Lengthen()
    {
        photonView.RPC("LengthUp", PhotonTargets.AllBufferedViaServer);
    }

    public void Shorten()
    {
        photonView.RPC("LengthDown", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void LengthUp()
    {
        ClippingDistance += .2f;
    }

    [PunRPC]
    void LengthDown()
    {
        if(ClippingDistance > 0)
            ClippingDistance -= .2f;
    }

    [PunRPC]
    void PlaneToggle()
    {
        planeOn = !planeOn;
        if (!planeOn) clippingPlane.GetComponent<ClippingScript>().enabled = false;
    }
}
