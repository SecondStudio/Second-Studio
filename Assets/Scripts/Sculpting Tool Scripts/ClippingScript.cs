using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClippingScript : MonoBehaviour
{
    GameObject clippingObject;
    List<GameObject> ClippedObjects;
    public Shader clippingShader;

    void Start ()
	{
        ClippedObjects = new List<GameObject>();
	}
	
	void Update ()
	{
		
	}


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trail")
        {
            var cutter = other.gameObject.GetComponent<OnePlaneCuttingController>();
            if (cutter == null) cutter = other.gameObject.AddComponent<OnePlaneCuttingController>();
            cutter.enabled = true;
            cutter.plane = gameObject;
            other.GetComponent<MeshRenderer>().material.shader = clippingShader;
            ClippedObjects.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Trail" && ClippedObjects.Contains(other.gameObject))
        {
            other.gameObject.GetComponent<OnePlaneCuttingController>().enabled = false;
            other.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            ClippedObjects.Remove(other.gameObject);
        }
    }

    public void Disable()
    {
        foreach (var other in ClippedObjects)
        {
            other.gameObject.GetComponent<OnePlaneCuttingController>().enabled = false;
            other.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            
        }
        ClippedObjects.Clear();

    }
}
