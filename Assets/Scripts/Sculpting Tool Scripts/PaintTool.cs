using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//much of the functionality is actully under colorpicker
public class PaintTool : ToolBase
{

    Color ActiveColor;
    bool Painting = false;
    public void SetColor(Color color) { ActiveColor = color; ColorIndicator.GetComponent<MeshRenderer>().material.color = color; }
    public GameObject ColorIndicator;
    public GameObject colorWheel;

    protected override void OnEnable()
    {
        base.OnEnable();
        colorWheel.GetComponent<ColorPicker>().controller = controller;
        if (isHeld)
            colorWheel.GetComponent<ColorPicker>().isHeld = true;
        trackerLetter = "P";
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        colorWheel.GetComponent<ColorPicker>().isHeld = false;
    }

    protected override void Update()
    {
        base.Update();
        if (!photonView.isMine) return;

        if (ColorIndicator.GetComponent<TriggerEnter>().other)
        {
            if (Painting && ColorIndicator.GetComponent<TriggerEnter>().other.CompareTag("Trail"))
            {
                if (ColorIndicator.GetComponent<TriggerEnter>().other.GetComponent<MeshRenderer>() != null)
                {
                    ColorIndicator.GetComponent<TriggerEnter>().other.GetComponent<MeshRenderer>().material.color = ActiveColor;
                }
            }
        }


        if (controller.triggerButtonDown)
        {
            photonView.RPC("StartPainting", PhotonTargets.AllBufferedViaServer);
        }
        else if (controller.triggerButtonUp)
        {
            photonView.RPC("StopPainting", PhotonTargets.AllBufferedViaServer);
        }

    }
    [PunRPC]
    public void StartPainting()
    {
        Painting = true;
    }

    [PunRPC]
    public void StopPainting()
    {
        Painting = false;
    }
}
