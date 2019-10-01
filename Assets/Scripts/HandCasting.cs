using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandCasting : MonoBehaviour
{

    //public float HandTargetDistance;
	public GameObject watch;
	//public GameObject watchUI;
	//public GameObject gazeCollider;
	//public GameObject negGravityButton;
	//public GameObject posGravityButton;
	//public bool triggerDown = false;
	//public bool triggerUp = false;
	public bool triggerAction = false;
	//public bool gravityButton = false;
	//public bool gravityHandle = false;
	//public bool quickpulse = false;
	//public float vibrationTimer = 0.0f;
	public bool newHit = false;

	private SteamVR_TrackedObject trackedObj;
	//public GazeCasting watchUIchanger;
	public WatchUIManager watchUIManagerScript;
	public GameObject pointingHand;
	public FingerPointer pointerScript;
	///public GravityTool gravityChanger;

	//private int count = 0;

	private SteamVR_Controller.Device Controller
	{
		get { return SteamVR_Controller.Input((int)trackedObj.index); }
	}

	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}

    private void Start()
    {
        
        //GameObject headset = GameObject.FindGameObjectWithTag("vive");
        //watchUIchanger = headset.GetComponent<GazeCasting>();
        //watchUIchanger.clock = true;
		//GameObject watch = GameObject.FindGameObjectWithTag("watch");
		watchUIManagerScript = watch.GetComponent<WatchUIManager>();
		//watchUIManagerScript.watchState = 0;
		pointerScript = pointingHand.GetComponent<FingerPointer>();
		//pointerScript.showPointer = false;
		//gravityChanger = watch.GetComponent<GravityTool>();
        //gravityChanger.posGravity = true;

    }

    // Update is called once per frame
    void Update()
    {

        RaycastHit TheHit;
        
		if (Physics.Raycast (transform.position, transform.TransformDirection (Vector3.forward), out TheHit)) {
            //HandTargetDistance = TheHit.distance;

            if (TheHit.collider.tag == "returnButton") // && newHit)
            {
                print("pointing at Return button");
                watchUIManagerScript.pointToButton = 1;
                //watchUIchanger.clock = false;
                //gravityButton = true;
                //gravityHandle = false;
                //gravityChanger.posGravity = true;
                //quickpulse = true;
                newHit = false;
                //pulse();
            }

            else if (TheHit.collider.tag == "buttonA") // && newHit)
            { //TheHit.collider.tag == "posGravity" || 
                print("pointing at Button A");
                watchUIManagerScript.pointToButton = 2;
                newHit = false;
                //watchUIchanger.clock = false;
                //gravityButton = true;
                //gravityHandle = false;
                //gravityChanger.posGravity = true;
                //quickpulse = true;
                //awaitingTrigger ();
                //pulse();
            }

            else if (TheHit.collider.tag == "buttonB") // && newHit)
            { //TheHit.collider.tag == "negGravity" || 
                print("pointing at Button B");
                watchUIManagerScript.pointToButton = 3;
                newHit = false;
                //watchUIchanger.clock = false;
                //gravityButton = true;
                //gravityHandle = false;
                //gravityChanger.posGravity = false;
                //quickpulse = true;
                //pulse();
            }

            else if (TheHit.collider.tag == "buttonC") // && newHit)
            {
                print("pointing at Button C");
                watchUIManagerScript.pointToButton = 4;
                newHit = false;
                //watchUIchanger.clock = false;
                //gravityButton = true;
                //gravityHandle = false;
                //gravityChanger.posGravity = false;
                //quickpulse = true;
                //pulse();
            }

            else if (TheHit.collider.tag == "sliderA") // && newHit)
            { //TheHit.collider.tag == "gravitySlider" || 
                print("pointing at gravity SLIDER");
                watchUIManagerScript.pointToButton = 5;
                newHit = false;
                //watchUIchanger.clock = false;
                //gravityHandle = true;
                //awaitingTrigger();
            }

            else if (TheHit.collider.tag == "watch")
            { // && HandTargetDistance <=0.5)
                print("pointing at watch");
                watchUIManagerScript.pointToButton = 0;
                watchUIManagerScript.pointToWatch = true;
                pointerScript.showPointer = true;
                newHit = true;
                //watchState = 2; // show watch Menu
                //gravityButton = false;
                //gravityHandle = false;
            }

            else
            {
                watchUIManagerScript.pointToWatch = false;
                pointerScript.showPointer = false;
                watchUIManagerScript.pointToButton = 0;
                newHit = true;
                print("reset NewHit");
                //watchUIchanger.clock = true;
                //gravityButton = false;
                //gravityHandle = false;
            }
		}
			
		if (Controller.GetPressDown (SteamVR_Controller.ButtonMask.Trigger))
        {
			watchUIManagerScript.triggerPulled = true;
            if(watchUIManagerScript.pointToButton != 0) pulse ();
		}

//		if (Controller.GetPressUp (SteamVR_Controller.ButtonMask.Trigger)) {
//			triggerAction = false;
//			watchUIManagerScript.triggerPulled = false;
//		}
        

	}


    /*void awaitingTrigger()
    {
		pulse();
		Debug.Log ("awaiting trigger");

		if (gravityButton && triggerAction) {
			gravityChanger.ToggleGravity();
			Debug.Log ("switch gravity now");
		}
			
		if (gravityHandle && triggerAction) {
			//gravityChanger.posGravity = !gravityChanger.posGravity;
			gravityChanger.ToggleGravity();
			Debug.Log ("Sliding Gravity Handle");
		}
	}*/

	void pulse(){
		SteamVR_Controller.Input ((int)trackedObj.index).TriggerHapticPulse (1000);
		print("pulse");
	}

}
