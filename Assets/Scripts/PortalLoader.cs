using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalLoader : MonoBehaviour {
    public RoomJoinButton JB;
    public GameObject tinter;

    // Use this for initialization
    void Start ()
    {
        
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "MainCamera" || col.tag == "Observer")
        {
            print("entered portal");

            NetworkManager.JoinRoom(JB.MyInfo[JB.Index].name);
            tinter.SetActive(false);

            if (col.tag == "Observer")
            {
                Destroy(col);
            }
        }
    }
}
