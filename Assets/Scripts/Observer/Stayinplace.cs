using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//temporary script to have the tool belt stay in place
public class Stayinplace : MonoBehaviour {

    float yPos;

	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {

        //transform.position = transform.parent.position + Vector3.down * .9f + Vector3.forward * -.15f;
        transform.rotation = Quaternion.identity;
        //transform.Rotate(new Vector3(0, 0, 0));
        yPos = transform.parent.position.y * .6f;
        Vector3 pos = new Vector3(transform.parent.position.x, yPos, transform.parent.position.z);
        transform.position = pos;
    }

    private void OnEnable()
    {
        yPos = transform.parent.position.y * .6f;
    }
}
