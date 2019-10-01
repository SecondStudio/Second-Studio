using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPassiveObserver : Photon.MonoBehaviour {
    // Use this for initialization
    public GameObject headCamera;

	void Start () {
		if(!photonView.isMine)
        {
            gameObject.SetActive(false);

            if (!photonView.isMine)
            {
                headCamera.GetComponent<Camera>().enabled = false;
            }
            else if (photonView.isMine)
            {
                headCamera.GetComponent<Camera>().enabled = true;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
