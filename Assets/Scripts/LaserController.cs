using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
//default steamVR script that causes compile errors. May be removed
public class LaserController : Photon.MonoBehaviour {

    /*public Wacki.ViveUILaserPointer laser;
    protected SteamVR_TrackedObject _trackedObject;

    public EVRButtonId triggerButton = EVRButtonId.k_EButton_SteamVR_Trigger;

    protected int _index;

    private float _distanceLimit;

    // Use this for initialization
    void Start()
    {
        var trackedObject = GetComponent<SteamVR_TrackedObject>();

        if (trackedObject != null)
        {
            _index = (int)trackedObject.index;
        }
    }


    // Update is called once per frame
    void Update () {
        if (photonView.isMine)
        {
            if (SteamVR_Controller.Input(_index).GetPressDown(triggerButton))
            {
                photonView.RPC("TurnOnLaser", PhotonTargets.AllBufferedViaServer);
            }
            else if(SteamVR_Controller.Input(_index).GetPress(triggerButton)) // used to change color
            {
                RaycastHit hit;
                Physics.Raycast(transform.position, transform.forward, out hit, 200.0f); // shoots ray to find collision

                if (hit.collider && hit.collider.gameObject.name == "Object_61") // if pixel collided
                {
                    // get the color of the pixel
                    Texture2D tex = (Texture2D)hit.collider.gameObject.GetComponent<MeshRenderer>().material.mainTexture;
                    Vector2 uv = hit.textureCoord;
                    uv.x *= tex.width;
                    uv.y *= tex.height;

                    Color c = tex.GetPixel((int)uv.x, (int)uv.y);
                    GameObject.Find("Tracker").GetComponent<TrackerScript>().colors.Add(c);
                    photonView.RPC("ChangeColor", PhotonTargets.AllBufferedViaServer, c.r, c.g, c.b);
                }
            }
            else if (SteamVR_Controller.Input(_index).GetPressUp(triggerButton))
            {
                photonView.RPC("TurnOffLaser", PhotonTargets.AllBufferedViaServer);
            }
        }
    }

    [PunRPC]
    void TurnOnLaser()
    {
        laser.laserOn = true;
    }

    [PunRPC]
    void ChangeColor(float r, float g, float b)
    {
        Color c = new Color(r, g, b);
        laser.SetColor(c);        
    }

    [PunRPC]
    void TurnOffLaser()
    {
        laser.laserOn = false;
    }*/
}
