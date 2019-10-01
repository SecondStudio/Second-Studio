using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// used to be for picking colors out of lasers, which is no longer done.
/// now, colors are picked using the wheel on the thumb
/// it is flexible enough to be moved onto things like a watch
/// handles the coloring of all drawing tools
/// implementation of the paint tool should be moved from here to the paint tool instead
/// </summary>
public class ColorPicker : Photon.MonoBehaviour
{
    protected int _index;

    private float _distanceLimit;

    public ProcShape splineShape;
    public ProcSection splineSection;
    public ProcHair splineHair;
    public ProcShape lineShape;
    public ProcSection lineSection;
    public ProcHair lineHair;
    public WandController controller;
    public PaintTool Paint;
    public bool isHeld;
    // Use this for initialization
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        if (photonView.isMine)
        {
            if (isHeld)
            {
                if (controller.dpadAxis.magnitude > .1)
                {
                    Texture2D tex2 = Resources.Load<Texture2D>("ColorWheel");
                    Vector2 axis = (controller.dpadAxis);
                    axis.x += 1;
                    axis.x *= 0.5f * tex2.width;
                    axis.x = tex2.width - axis.x;
                    axis.y += 1;
                    axis.y *= 0.5f * tex2.height;
                    Color c2 = tex2.GetPixel((int)axis.x, (int)axis.y);
                    if (c2 == Color.black || c2 == Color.clear) return;
                    GameObject.Find("Tracker").GetComponent<TrackerScript>().colors.Add(c2);
                    photonView.RPC("ChangeColor", PhotonTargets.AllBufferedViaServer, c2.r, c2.g, c2.b);
                }
            }
            
        }
    }

    [PunRPC]
    void TurnOnLaser()
    {
        //laser.laserOn = true;
    }

    [PunRPC]
    void ChangeColor(float r, float g, float b)
    {
        Color c = new Color(r, g, b);
        //laser.SetColor(c);

        if (lineShape != null || splineShape !=null)
        {
            lineShape.ChangeColor(c);
            lineSection.ChangeColor(c);
            if(lineHair)lineHair.ChangeColor(c);
            splineShape.ChangeColor(c);
            splineSection.ChangeColor(c);
            splineHair.ChangeColor(c);
        }

        Paint.SetColor(c);
        splineShape.GetComponent<MeshRenderer>().material.color = c;
        splineSection.GetComponent<MeshRenderer>().material.color = c;
        splineHair.GetComponent<MeshRenderer>().material.color = c;
        lineShape.GetComponent<MeshRenderer>().material.color = c;
        lineSection.GetComponent<MeshRenderer>().material.color = c;
        lineHair.GetComponent<MeshRenderer>().material.color = c;
    }

    [PunRPC]
    void TurnOffLaser()
    {
        //laser.laserOn = false;
    }
}
