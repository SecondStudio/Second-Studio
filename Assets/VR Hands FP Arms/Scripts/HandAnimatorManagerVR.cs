using UnityEngine;
using System.Collections;
using System;

public class HandAnimatorManagerVR : MonoBehaviour
{
	public StateModel[] stateModels;
	Animator handAnimator;
    public WandController controller;

	public int currentState = 100;
	int lastState = -1;
	public bool hold = false;

	//grip axis 11 or 12
	public string holdKey = "";

	public int numberOfAnimations = 8;

	// Use this for initialization
	void Start ()
	{
		string[] joys = UnityEngine.Input.GetJoystickNames ();
		foreach (var item in joys) {
			Debug.Log (item);
		}
		handAnimator = GetComponent<Animator> ();
        handAnimator.speed = 1.5f;
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (controller.isHoldingAnim) {
			hold = true;
		} else
			hold = false;

		if (lastState != currentState)
        {
			lastState = currentState;
			handAnimator.SetInteger ("State", currentState);
			TurnOnState (currentState);
		}
		handAnimator.SetBool ("Hold", hold);
	}

	void TurnOnState (int stateNumber)
	{
		foreach (var item in stateModels) {
			if (item.stateNumber == stateNumber && !item.go.activeSelf)
				item.go.SetActive (true);
			else if (item.go.activeSelf)
				item.go.SetActive (false);
		}
	}


}

