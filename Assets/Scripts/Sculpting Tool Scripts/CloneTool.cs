using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// clones the number of objects as is selected
/// will clone groups
/// two ways to clone, first is linear, second is radial
/// </summary>
public class CloneTool : ToolBase
{
    int numClone = 2;
    public Text numText , modeText;
    bool isRadial = false;

    Vector3 startPoint;
    Quaternion startRot;
    Vector3 endPoint;
    Quaternion endRot;
    float endRotation = 0;
    float endDistance = 0;
    int currentClones = 0;
    GameObject go , objectToClone;
    List<GameObject> clones;
    bool Cloning;

    protected override void Start()
    {
        base.Start();
        trackerLetter = "C";
    }

    protected override void Update()
    {
        base.Update();
		if(controller.triggerButtonDown && go)
        {
            clones = new List<GameObject>();
            startPoint = controller.transform.position;
            startRot = controller.transform.rotation;
            objectToClone = go;
            Cloning = true;
        }

        if(Cloning && controller.triggerButtonPressed)
        {
            endPoint = controller.transform.position;
            endRot = controller.transform.rotation;

            DrawClones();
        }

        if(Cloning && controller.triggerButtonUp)
        {
            currentClones = 0;
            clones.Clear();
            Cloning = false;
        }

        numText.text = ""+ numClone;
	}

    public void Lengthen()
    {
        photonView.RPC("LengthUp", PhotonTargets.AllBufferedViaServer);
    }

    public void Shorten()
    {
        photonView.RPC("LengthDown", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void LengthUp()
    {
        numClone += 1;
    }

    [PunRPC]
    void LengthDown()
    {
        if(numClone > 1)
            numClone -= 1;
    }

    [PunRPC]
    public void ToggleRadial()
    {
        isRadial = !isRadial;
        modeText.text = isRadial ? "Radial" : "Linear";
    }

    void DrawClones()
    {
        if (isRadial)
        {
            endRotation = Vector3.Angle(startRot.eulerAngles, endRot.eulerAngles);

            if (numClone > currentClones)
            {
                var newClone = Instantiate(objectToClone, objectToClone.transform.position, objectToClone.transform.rotation);
                clones.Add(newClone);
                ObjectManager.instance.AddObject(newClone);
            }

            else if (numClone < currentClones)
            {
                foreach (var c in clones.GetRange(numClone, currentClones - numClone)) Destroy(c);
                clones.RemoveRange(numClone, currentClones - numClone);
            }

            float rotationStep = 360 / numClone;
            for (int i = 0; i < numClone; i++)
            {
                clones[i].transform.position = objectToClone.transform.position +  Quaternion.AngleAxis(rotationStep * i, Vector3.up) * (endPoint - startPoint) + (clones[i].GetComponent<MeshRenderer>() != null ? (clones[i].transform.position - clones[i].GetComponent<MeshRenderer>().bounds.center) : Vector3.zero);
                foreach (MeshEditor m in clones[i].GetComponentsInChildren<MeshEditor>())
                    m.GenerateVertexGroupsNow();
            }

            currentClones = numClone;
        }

        else
        {
            endDistance = Vector3.Distance(startPoint, endPoint);

            if (numClone > currentClones)
            {
                var newClone = Instantiate(objectToClone, objectToClone.transform.position, objectToClone.transform.rotation);
                clones.Add(newClone);
                ObjectManager.instance.AddObject(newClone);
            }

            else if (numClone < currentClones)
            {
                foreach (var c in clones.GetRange(numClone, currentClones - numClone)) Destroy(c);
                clones.RemoveRange(numClone, currentClones - numClone);
            }

            for (int i = 0; i < numClone; i++)
            {
                clones[i].transform.position = Vector3.Normalize(endPoint - startPoint) * (Vector3.Distance(endPoint, startPoint) / numClone) * (i + 1) + objectToClone.transform.position + (clones[i].GetComponent<MeshRenderer>() != null ? (clones[i].transform.position - clones[i].GetComponent<MeshRenderer>().bounds.center) : Vector3.zero);
                foreach(MeshEditor m in clones[i].GetComponentsInChildren<MeshEditor>())
                    m.GenerateVertexGroupsNow();
            }

            currentClones = numClone;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Trail")
        {
            Transform topLevelTransform = other.transform;

            while (topLevelTransform.parent != null && !topLevelTransform.parent.CompareTag("TrailContainer") && topLevelTransform.parent.GetComponent<GrabObjects>() == null)
            {
                topLevelTransform = topLevelTransform.parent;
            }
            go = topLevelTransform.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Trail"))
            go = null;
    }

    
}
