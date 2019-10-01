using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(PhotonView))]
public class GravityTool : MonoBehaviour {

    //public Text GravityScaleText;
    public float CurrentGravityScale;
    public bool posGravity = true;
    //public bool negGravity = false;
	public Button negGravityButton;
	public Button posGravityButton;
	public Color activeColor;
	public Color inactiveColor;

    //preset values to what the gravity can possibly be.
    //0 gravity won't show any text right now
    //float[] scale = { -1f, 1f};
    //int index = 1;

    void Start()
    {
		CurrentGravityScale = Physics.gravity.magnitude;
        posGravity = true;

		//negGravityButton = Button.FindGameObjectWithTag("negGravity");
		//posGravityButton = Button.FindGameObjectWithTag("posGravity");
		activeColor = Color.green;
		inactiveColor = Color.white;
	}

    // Update is called once per frame
    /*void Update () {
        //base.Update();

        //if(controller.triggerButtonDown)
        //{
        //    photonView.RPC("GravityOn", PhotonTargets.AllBufferedViaServer);
        //}
	}

    /*
    public void IncreaseGravity()
    {
    //    photonView.RPC("GravityUp", PhotonTargets.AllBufferedViaServer);
    }

    public void DecreaseGravity()
    {
    //    photonView.RPC("GravityDown", PhotonTargets.AllBufferedViaServer);
    }

    //[PunRPC]
    void GravityUp()
    {
        //if (index < 2)
        CurrentGravityScale = 1f;
        Physics.gravity = Vector3.down * CurrentGravityScale;
        GravityScaleText.text = CurrentGravityScale.ToString("#.##");
    }

    //[PunRPC]
    void GravityDown()
    {
        //if (index > 0)
        CurrentGravityScale = -1f;
        Physics.gravity = Vector3.down * CurrentGravityScale;
        GravityScaleText.text = CurrentGravityScale.ToString("#.##");
    }
    */
    //void GravityOn()
    //{
    //ObjectManager.instance.FlipGravity();
    //}

    public void ToggleGravity(bool posGravity)
    {			
		if (posGravity) CurrentGravityScale = 1;
		else CurrentGravityScale = -1; 

	    //posGravity = !posGravity;
	    Physics.gravity = Vector3.down * CurrentGravityScale;
	    Debug.Log("Gravity now: " + CurrentGravityScale);
    }

    //CurrentGravityScale = val;
    //if (!ObjectManager.instance.gravity) ObjectManager.instance.FlipGravity();

    /*
    public void Gravity()
    {
        //gravity = !gravity;
        foreach (Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            try
            {
                rb.useGravity = true;

                //if (rb.GetComponent<MeshCollider>())
                //{
                //    rb.GetComponent<MeshCollider>().convex = CurrentGravityScale;
                //}
                //if (!gravity) rb.velocity = Vector3.zero;
                //rb.isKinematic = !gravity;
            }
            catch
            {
                print("something went wrong in gravity flipping");
            }
        }
    }*/
}
