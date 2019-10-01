using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FingerPointer : MonoBehaviour
{
    public enum AxisType
    {
        XAxis,
        ZAxis
    }

    public Color color;
    public float thickness = 0.001f;
    public AxisType facingAxis = AxisType.XAxis;
    public float length = 100f;
    public bool showCursor;
	public bool showPointer;

    GameObject holder;
    GameObject pointer;
    GameObject cursor;

    Vector3 cursorScale = new Vector3(0.02f, 0.02f, 0.02f);
    float contactDistance = 0f;
    Transform contactTarget = null;

    void SetPointerTransform(float setLength, float setThicknes)
    {
        //if the additional decimal isn't added then the beam position glitches
        float beamPosition = setLength / (2 + 0.00001f);
			
			if (facingAxis == AxisType.XAxis) {
				pointer.transform.localScale = new Vector3 (setLength, setThicknes, setThicknes);
				pointer.transform.localPosition = new Vector3 (beamPosition, 0f, 0f);
				if (showCursor) {
					cursor.transform.localPosition = new Vector3 (setLength - cursor.transform.localScale.x, 0f, 0f);
				}
			} else {
				pointer.transform.localScale = new Vector3 (setThicknes, setThicknes, setLength);
				pointer.transform.localPosition = new Vector3 (0f, 0f, beamPosition);

				if (showCursor) {
					cursor.transform.localPosition = new Vector3 (0f, 0f, setLength - cursor.transform.localScale.z);
				}
			}
    }

    // Use this for initialization
    void Start()
    {
		showCursor = true;
		showPointer = false;
		Debug.Log("pointer is disabled");

			Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        	newMaterial.SetColor("_Color", color);

 	       	holder = new GameObject();
    	    holder.transform.parent = this.transform;
        	holder.transform.localPosition = Vector3.zero;

			pointer = GameObject.CreatePrimitive (PrimitiveType.Cube);

		if (showPointer)
        {
			pointer.transform.parent = holder.transform;
			pointer.GetComponent<MeshRenderer> ().material = newMaterial;
			pointer.GetComponent<BoxCollider> ().isTrigger = true;
			pointer.AddComponent<Rigidbody> ().isKinematic = true;
			pointer.layer = 2;
		}

        if (showCursor)
        {
            cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cursor.transform.parent = holder.transform;
            cursor.GetComponent<MeshRenderer>().material = newMaterial;
            cursor.transform.localScale = cursorScale;
            cursor.GetComponent<SphereCollider>().isTrigger = true;
            cursor.AddComponent<Rigidbody>().isKinematic = true;
            cursor.layer = 2;
        }

        SetPointerTransform(length, thickness);
    }

    float GetBeamLength(bool bHit, RaycastHit hit)
    {
        float actualLength = length;

        //reset if beam not hitting or hitting new target
        if (!bHit || (contactTarget && contactTarget != hit.transform))
        {
            contactDistance = 0f;
            contactTarget = null;
        }

        //check if beam has hit a new target
        if (bHit)
        {
            if (hit.distance <= 0)
            {

            }
            contactDistance = hit.distance;
            contactTarget = hit.transform;
        }

        //adjust beam length if something is blocking it
        if (bHit && contactDistance < length)
        {
            actualLength = contactDistance;
        }

        if (actualLength <= 0)
        {
            actualLength = length;
        }

        return actualLength; ;
    }

    void Update()
	{
		if (showPointer) {
			Debug.Log ("pointer is showing");
			Ray raycast = new Ray (transform.position, transform.forward);

			RaycastHit hitObject;
			bool rayHit = Physics.Raycast (raycast, out hitObject);

			float beamLength = GetBeamLength (rayHit, hitObject);
			SetPointerTransform (beamLength, thickness);
		} 

		else {
			Debug.Log ("pointer is disabled");
		}
	}
}
