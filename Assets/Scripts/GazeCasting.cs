using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GazeCasting : MonoBehaviour
{

    //public float HeadTargetDistance;  // not needed
    public GameObject watch;            // watch model
    //public GameObject Projector;      // watch projector mesh
    //public GameObject hologram;         // watch light element to simulate hologram
    //public GameObject clockUI;          // watch UI element showing time (default)
    //public GameObject watchUI;          // watch UI element with control options (shown once hand moves towards watch)
	//public GameObject pointingHand;
	//public bool clock;
    //public bool watchActive;
    //public bool toggleOnce;
    //public GravityTool gravityChanger;
	//public FingerPointer pointerScript;
	public WatchUIManager watchUIManagerScript;

    private void Start()
    {
		//watch = GameObject.FindGameObjectWithTag("watch");
		watchUIManagerScript = watch.GetComponent<WatchUIManager>();
		//watchUIManagerScript.watchState = 0;

		//pointerScript = pointingHand.GetComponent<FingerPointer>();
		//pointerScript.showPointer = false;

		//clock = true;
		//toggleOnce = true;

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit TheHit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out TheHit))
        {
            //TargetDistance = TheHit.distance;
            
			if (TheHit.collider.tag == "VRHand")
			{
				print("Gazecast hit controller");
				//if (watchActive) WatchActive();
				//else WatchInactive();
			}

            if (TheHit.collider.tag == "buttonA") // watch gaze collider
            {
                //watchUIManagerScript.eyesOnWatch = true;
                //watchUIManagerScript.watchState = 1;
                print("Gazecast hit ButtonA");
            }

            if (TheHit.collider.tag == "watch") // watch gaze collider
			{
				watchUIManagerScript.eyesOnWatch = true;
				//watchUIManagerScript.watchState = 1;
				print("Gazecast hit watch");
            }

            else
			{
				watchUIManagerScript.eyesOnWatch = false;
				//watchUIManagerScript.watchState = 0;
				print("Gazecast off watch");
			}

        }

    }

}
