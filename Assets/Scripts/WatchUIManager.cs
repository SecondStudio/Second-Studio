using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatchUIManager : MonoBehaviour
{

	public GameObject watch;            	// watch object
	//public GameObject watchUI;			// virtual display on wrist
	public GameObject watchUIClock;     	// watch UI element showing time (default)
	public GameObject watchUIMenu;      	// watch UI element with various control options (shown once hand moves towards watch)
	public GameObject watchUIEnvironmentSettings;	// Watch UI element with Environment Settings
	public GameObject watchUIDrawSettings;	// Watch UI element with Drawing Settings

	public int watchState = 0;				// used to switch between watchUI states
	public int pointToButton = 0;			// identifies which UI element is being pointed at

	public float watchResetTimer = 0.0f;			// keeps menu active for up to 10 seconds
	public bool watchReset = false;			// resets watch to main menu
	public float watchSwitchTimer = 0.0f;		// adds delay to switching of windows
	public bool watchSwitchGo = true;		// enables menu switching when system is ready. 

	public bool switchStates = false;		
	public bool eyesOnWatch = false;		// looks at watch yes/no
	public bool pointToWatch = false;		// points at watch yes/no
	public bool triggerPulled = false;		// pulled trigger for interaction yes/no

	public GameObject Hologram;         	// light elements on watch to simulate hologram
	public bool watchActive = false;		//
	public bool clockActive = true;
	public bool toggleOnce = true;

	public GravityTool gravityChanger;

	//public FingerPointer pointerScript;
	//public GameObject pointingHand;			// the hand used to point at watch


	private void Start()
	{
		watchState = 0; //WatchInactive();
		gravityChanger = watch.GetComponent<GravityTool>();
		//clockActive = true;
		//watchActive = false;
		//toggleOnce = true;
	}

	// Update is called once per frame
	void Update()
	{
		//if (!eyesOnWatch) watchState = 0; // turn watch off
		if (watchState == 0 && eyesOnWatch) watchState = 1; // activate clock
		if (watchState == 1 && pointToWatch) watchState = 2; // activate menu

        if (!pointToWatch && eyesOnWatch)
        {
            watchState = 1;
            print("MNGR Script: not pointing at watch");
        }

        if (!eyesOnWatch)
        {
            watchState = 0;    //DelayedReset (true);
            print("MNGR Script: not looking at watch");
        }                                                  //if (pointToWatch && watchReset) DelayedReset (false);

        //else watchState = 0;

        //print ("watch state = " + watchState);

		switch (watchState) {
		case 0:
			WatchInactive ();
			print ("WatchState set to 0: watch turned off");
			break;
		case 1: // user is only looking at watch
			WatchActive ();
			watchUIClock.SetActive (true);
			watchUIMenu.SetActive (false);
			watchUIEnvironmentSettings.SetActive(false);
			watchUIDrawSettings.SetActive(false);
            print("WatchState set to 1: show clock on watch");
			break;
		case 2: // user is looking && pointing at watch - show menu
			WatchActive ();
			watchUIClock.SetActive(false);
			watchUIMenu.SetActive(true);
			watchUIEnvironmentSettings.SetActive(false);
			watchUIDrawSettings.SetActive(false);
			print ("WatchState set to 2: show watch menu");
			break;
		case 3: // show environment settings
			WatchActive ();
			watchUIClock.SetActive(false);
			watchUIMenu.SetActive(false);
			watchUIEnvironmentSettings.SetActive(true);
			watchUIDrawSettings.SetActive(false);
			print ("WatchState set to 3: show environment settings");
			break;
		case 4: // show drawing settings
			WatchActive ();
			watchUIClock.SetActive(false);
			watchUIMenu.SetActive(false);
			watchUIEnvironmentSettings.SetActive(false);
			watchUIDrawSettings.SetActive(true);
			print ("WatchState set to 4: show drawing settings");
			break;
		default: // user is not looking at watch
			WatchInactive();
			print ("WatchState DEFAULT: show nothing");
			break;
		}

		if(triggerPulled) {
			
			if (watchState == 3 && pointToButton == 2) gravityChanger.ToggleGravity (false);
			if (watchState == 3 && pointToButton == 3) gravityChanger.ToggleGravity (true);
			if (watchState == 2 && pointToButton == 2) watchState = 3; 
			if (watchState == 2 && pointToButton == 3) watchState = 4;
			if (pointToButton == 1) watchState = 2;

			triggerPulled = false;
			//switchOnce = false;
		}

	}

	public void WatchActive()
	{
		Hologram.SetActive(true); // light up hologram
		Hologram.transform.Rotate(Vector3.up * 360 * 10 * Time.deltaTime, Space.Self); // rotate 10 times per second
	}

	public void WatchInactive()
	{
		Hologram.SetActive(false); // hologram inactive
		Hologram.transform.Rotate(Vector3.up * 0); // no rotation
		watchUIClock.SetActive(false);
		watchUIMenu.SetActive(false);
		watchUIEnvironmentSettings.SetActive(false);
		watchUIDrawSettings.SetActive(false);
	}

	public void DelayedReset(bool watchReset)
	{
        /*if (watchReset) {
			print ("watch reset initiated");
			watchResetTimer = +Time.deltaTime;
			if (watchResetTimer >= 5) {
				watchState = 0;
				print ("watch reset complete");
				watchResetTimer = 0;
			} else {
				print ("watch reset counter" + watchResetTimer);
			}
		} else {
			watchResetTimer = 0;
			print ("not resetting");
		}*/
        //watchState = 1;
	}

}



// OLD FRAGMENT

//		switch (pointToButton) {
//		case 1: // user is pointing at Return button
//			print ("pointing at Return Button");
//			if (triggerPulled) 
//				watchState = 2; // always go back to Main Menu
//			break;
//		case 2: // user is pointing at Button A
//			print ("pointing at Button A");
//			if (triggerPulled) {
//				if (watchState == 2) // at Main Menu
//				{
//					watchState = 3; // go to Environment Settigns
//					print("go to Environment Settings");
//				}
//				if (watchState == 3) // at Environment Settings
//				{
//					gravityChanger.posGravity = false; // switch Gravity to Positive
//					gravityChanger.ToggleGravity(); // trigger gravity action
//					print ("Turn gravity positive");
//				}
//				if (watchState == 4) // at Main Menu
//				{
//					print("ButtonA Pressed");
//				}
//			}
//			break;
//		case 3: // user is pointing at Button B
//			print ("pointing at Button B");
//			if (triggerPulled) {
//				if (watchState == 2) // at Main Menu
//				{
//					watchState = 4; // go to Draw Settigns
//					print("go to draw settings");
//				}
//				if (watchState == 3) // at Environment Settings
//				{
//					gravityChanger.posGravity = true; // switch Gravity to Negative
//					gravityChanger.ToggleGravity();  // trigger gravity action
//					print ("Turn gravity negative");
//				}
//				if (watchState == 4) // at Main Menu
//				{
//					print("ButtonB Pressed");
//				}
//			}
//			break;
//		case 4: // user is pointing at Button C
//			print ("pointing at Button C");
//			break;
//		case 5: // user is pointing at Slider A
//			print ("pointing at Slider A");
//			break;
//		default: // user not pointing at button
//			print ("not pointing at any button");
//			break;
//		} 
//		*/
