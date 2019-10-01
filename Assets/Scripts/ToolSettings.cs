using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// base class for all tool settings
/// </summary>
public class ToolSettings : MonoBehaviour {

    public UnityEvent OnLeft, OnRight, OnUp, OnDown;
	// Use this for initialization
	void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnEnable()
    {
        gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        gameObject.SetActive(false);
    }
}
