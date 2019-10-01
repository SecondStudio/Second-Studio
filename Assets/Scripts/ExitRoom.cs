using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class ExitRoom : Photon.MonoBehaviour
{
    public EVRButtonId button = EVRButtonId.k_EButton_ApplicationMenu;

    protected int _index;

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
    void Update()
    {
        if (photonView.isMine)
        {
            if (SteamVR_Controller.Input(_index).GetPressDown(button))
            {
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("Lobby");
            }
        }
    }
}
