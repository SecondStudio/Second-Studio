using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionConstraintSettings : ToolSettings
{
	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void ConstrainX()
    {
        GetComponentInParent<ConstraintManager>().ToggleConstrainX();
    }

    public void ConstrainY()
    {
        GetComponentInParent<ConstraintManager>().ToggleConstrainY();
    }

    public void ConstrainZ()
    {
        GetComponentInParent<ConstraintManager>().ToggleConstrainZ();
    }
}
