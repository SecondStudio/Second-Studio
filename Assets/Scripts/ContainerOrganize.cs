using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Searches under Sketching Tool Container for "CenterPoint" tags and puts them in order
/// </summary>
[ExecuteInEditMode]
public class ContainerOrganize : MonoBehaviour {

    public float LeftMost = -.75f;
    public float RightMost = 1.1f;
    public float spaceInBetween = .35f;
    public float TopMost = 1.7f;
    public float spaceBelow = .4f;

	// Use this for initialization
	void Start ()
    {

    }
    
    //this places all of the tools in order across the wall in multiple rows
    public void OnEnable()
    {
        float i = LeftMost;
        float j = TopMost;
        foreach (Transform center in transform)
        {
            if (center.tag == "CenterPoint")
            {
                center.position = new Vector3(.2f, j, i);
                i += spaceInBetween;
                if (i > RightMost)
                {
                    j -= spaceBelow;
                    i = LeftMost;
                }
            }
        }

        /* this is in case the tools are to be rotating around the player like a belt, instead of having them on the wall.
        int i = 0;
        foreach (Transform center in transform)
        {
            if (center.tag == "CenterPoint")
            {
                float radius = .4f;
                center.position = new Vector3(radius * Mathf.Cos(i) + transform.parent.position.x, 
                    transform.parent.position.y + .1f, radius * Mathf.Sin(i) + transform.parent.position.z);
                i += 20;
            }
        }*/
        
    }

    // Update is called once per frame
    void Update () {
		
	}
}
