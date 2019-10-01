using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorScript : Photon.MonoBehaviour
{
    public GameObject reflection;
    public GameObject theMirror;

    // Use this for initialization
    void Start ()
    {
		if(!photonView.isMine)
        {
            MeshRenderer[] mrList = gameObject.GetComponentsInChildren<MeshRenderer>();
            for (int i=0; i<mrList.Length; ++i)
            {
                mrList[i].enabled = false;
            }
        }
        gameObject.SetActive(false);
	}
   
    public Vector3 ReflectPoint(Vector3 inPoint)
    {
        return Vector3.Reflect(inPoint - transform.position, theMirror.transform.up) + transform.position;
    }

    public void CreateReflection(GameObject go, Transform sceneTransform)
    {
        if (go != null)
        {
            // creates game object to mimic other side
            reflection = Instantiate(go, ReflectPoint(go.transform.position), go.transform.rotation) as GameObject;


            // set gameobject hieararchy
            reflection.transform.parent = sceneTransform;
            ObjectManager.instance.AddObject(reflection);
        }
    }
    // Update is called once per frame
    void Update ()
    {

    }

    public Vector3 ReflectVector(Vector3 inVector)
    {
        return Vector3.Reflect(inVector, theMirror.transform.up);
    }
}
