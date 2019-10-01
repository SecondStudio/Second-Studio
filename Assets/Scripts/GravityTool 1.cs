/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class GravityTool : ToolBase {

    public Text GravityScaleText;
    float CurrentGravityScale;
    //preset values to what the gravity can possibly be.
    //0 gravity won't show any text right now
    float[] scale = { -9.81f, -4.9f, -2.5f, -1f, 0f, 1f, 2.5f, 4.9f, 9.81f};
    int index = 8;

    protected override void Start()
    {
        base.Start();
		CurrentGravityScale = Physics.gravity.magnitude;
        GravityScaleText.text = CurrentGravityScale.ToString("#.##");
        trackerLetter = "V";
    }
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();

        if(controller.triggerButtonDown)
        {
            photonView.RPC("GravityOn", PhotonTargets.AllBufferedViaServer);
        }
	}

    
    public void IncreaseGravity()
    {
        photonView.RPC("GravityUp", PhotonTargets.AllBufferedViaServer);
    }

    public void DecreaseGravity()
    {
        photonView.RPC("GravityDown", PhotonTargets.AllBufferedViaServer);
    }


    [PunRPC]
    void GravityUp()
    {
        if (index < 8)
            CurrentGravityScale = scale[++index];

        Physics.gravity = Vector3.down * CurrentGravityScale;
        GravityScaleText.text = CurrentGravityScale.ToString("#.##");
    }

    [PunRPC]
    void GravityDown()
    {
        if (index > 0)
            CurrentGravityScale = scale[--index];

        Physics.gravity = Vector3.down * CurrentGravityScale;
        GravityScaleText.text = CurrentGravityScale.ToString("#.##");
    }

    [PunRPC]
    void GravityOn()
    {
        ObjectManager.instance.FlipGravity();

    }

    public void SetGravity(float val)
    {
        Physics.gravity = Vector3.down * val;
        CurrentGravityScale = val;
        if (!ObjectManager.instance.gravity) ObjectManager.instance.FlipGravity();
    }
}
*/