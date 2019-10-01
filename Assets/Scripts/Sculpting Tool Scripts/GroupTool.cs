using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// groups objects together so that they can be moved and scaled by the parent
/// however, it doesn't actually combine any of the meshes together, so they can be easily ungrouped and split apart again
/// this means that manipulations on the meshes only happen to the individual objects themselves, instead of the entire group
/// boolean operations can actually combine meshes instead
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class GroupTool : ToolBase
{

    List<GameObject> SelectedObjects;
    bool IsGrouping = false;

	protected override void Start ()
    {
        base.Start();
        SelectedObjects = new List<GameObject>();
        trackerLetter = "G";
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!photonView.isMine)
            return;

        if (controller.triggerButtonDown)
        {
            photonView.RPC("StartGrouping", PhotonTargets.AllBufferedViaServer);
        }
        else if (controller.triggerButtonUp)
        {
            int[] objectIDs = new int[SelectedObjects.Count];
            for(int i = 0; i < SelectedObjects.Count; i++)
            {
                objectIDs[i] = SelectedObjects[i].GetComponent<ObjectID>().id;
            }
            photonView.RPC("MakeGroup", PhotonTargets.AllBufferedViaServer, objectIDs);

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        print("Hit something!");
        if(IsGrouping && other.tag == "Trail")
        {
            if (!SelectedObjects.Contains(other.gameObject))
            {
                if (other.gameObject.GetComponent<LineRenderer>())
                {
                    other.gameObject.GetComponent<LineRenderer>().enabled = false;
                }
                SelectedObjects.Add(other.gameObject);
                other.GetComponent<ObjectID>().ObjectColor = other.GetComponent<MeshRenderer>().material.color;
                other.GetComponent<MeshRenderer>().material.color = Color.green;
                print("Adding: " + other.name + " to the group");
            } else
            {
                SelectedObjects.Remove(other.gameObject);
                other.GetComponent<MeshRenderer>().material.color = other.GetComponent<ObjectID>().ObjectColor;
            }
            
        }
    }

    [PunRPC]
    void StartGrouping()
    {
        print("Starting to group");
        SelectedObjects.Clear();
        IsGrouping = true;
    }

    [PunRPC]
    void MakeGroup(int[] objectIDs)
    {
        List<GameObject> SelectedObjects = new List<GameObject>();
        foreach(int i in objectIDs)
        {
            SelectedObjects.Add(ObjectManager.instance.FindObject(i));
        }

        if (SelectedObjects.Count == 0)
        {
            IsGrouping = false;
            //TrackerScript.AddAction("K");
            return;            

        }
        GameObject groupParent = new GameObject();

        Rigidbody parentRigid = groupParent.AddComponent<Rigidbody>();
        parentRigid.useGravity = ObjectManager.instance.gravity;
        parentRigid.isKinematic = !ObjectManager.instance.gravity;

        ObjectManager.instance.AddObject(groupParent);

        Vector3 AveragePosition = Vector3.zero;

        foreach (GameObject go in SelectedObjects)
        {
            AveragePosition += go.transform.position;
        }

        AveragePosition /= SelectedObjects.Count;
        groupParent.transform.position = AveragePosition;

        
        foreach (GameObject go in SelectedObjects){
            go.transform.SetParent(groupParent.transform);
            if (go.GetComponent<FixedJoint>())
            {
                DestroyImmediate(go.GetComponent<FixedJoint>());
            }
            var joint = go.AddComponent<FixedJoint>();
            joint.connectedBody = parentRigid;
            joint.enableCollision = false;
            go.GetComponent<MeshRenderer>().material.color = go.GetComponent<ObjectID>().ObjectColor;
            LineRenderer lr = go.GetComponent<LineRenderer>();
            if (!lr)
            {
               lr = go.AddComponent<LineRenderer>();
                lr.startWidth = 0.005f;
                lr.endWidth = 0.01f;
                lr.positionCount = 2;
                lr.material = Resources.Load<Material>("LineRendererMaterial");
            }

            lr.enabled = true;
            lr.startColor = Color.white;
            lr.endColor = go.GetComponent<ObjectID>().ObjectColor;
            lr.SetPosition(0, AveragePosition);
            lr.SetPosition(1 , go.transform.position);
            go.GetComponent<ObjectID>().HasParent = true;
            //if(go.GetComponent<MeshCollider>() != null) go.GetComponent<MeshCollider>().convex = true;
            go.GetComponent<Rigidbody>().isKinematic = true;//!ObjectManager.instance.gravity;

        }
        IsGrouping = false;
        //TrackerScript.AddAction("J");

    }


    void UpdateCollisionMask(GameObject parent)
    {

        foreach(Collider first in parent.GetComponentsInChildren<Collider>())
        {
            foreach (Collider second in parent.GetComponentsInChildren<Collider>())
            {
                if (first == second) return;
                Physics.IgnoreCollision(first , second , true);
            }
        }
        
        

    }
}
