using UnityEngine;
using System.Collections;

/// <summary>
/// erasing is a little bit inconsistent to some of the other tools here and is similar to the mirror in that it simply detects of the object is on or off
/// and then do functions based on that.
/// this is also wierd the way global vs non global erasing is handled
/// </summary>
public class EraserTool : ToolBase
{
    MeshRenderer mr;
    public GameObject eraser;

    public Material onMat;
    public Material offMat;

    private float timeUpdate = 0.02f;
    private float timeSizeLeft = 0.0f;

    public Transform eraserTransform;
    Vector3 scaleVector;

    bool isGlobal;
    bool isErasing;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        mr = eraser.GetComponent<MeshRenderer>();
        mr.material = offMat;
        eraserTransform = transform;
        scaleVector = new Vector3(0.05f, 0.05f, 0.05f);
        isErasing = false;
        isGlobal = false;
        trackerLetter = "R";
    }

    [PunRPC]
    void EraseObject(int id)
    {
        ObjectManager.instance.DeleteObject(id);
        GameObject.Find("Tracker").GetComponent<TrackerScript>().numErase++;
    }

    public void toggleGlobal()
    {
        isGlobal = !isGlobal;
    }

    // enable or disenable erasing object
    // or erase all trails in the scene
    [PunRPC]
    void EraseOn()
    {
        isErasing = true;

        if (isGlobal)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Trail"))
            {
                photonView.RPC("EraseObject", PhotonTargets.AllBufferedViaServer, go.GetComponent<ObjectID>().id);
            }
        }

        else
        {
            mr.material = onMat;
        }
    }

    [PunRPC]
    void EraseOff()
    {
        isErasing = false;
        mr.material = offMat;
    }

    // enable or disenable erasing object
    [PunRPC]
    void ScaleUp()
    {
        if(eraser.transform.parent.parent.localScale.x < 1.5f)
        {

            eraser.transform.parent.parent.localScale += scaleVector;
            ToolTracker.net[3] = 2;
            ToolTracker.value[0] = "f";
            ToolTracker.value[2] = "" + scaleVector.magnitude;
            ToolTracker.setEmpty(2);
            TrackerScript.AddAction();
        }
    }

    // enable or disenable erasing object
    [PunRPC]
    void ScaleDown()
    {
        if (eraser.transform.parent.parent.localScale.x > 0.2f)
        {
            eraser.transform.parent.parent.localScale -= scaleVector;
            ToolTracker.net[3] = 3;
            ToolTracker.value[0] = "f";
            ToolTracker.value[2] = "" + scaleVector.magnitude;
            ToolTracker.setEmpty(2);
            TrackerScript.AddAction();
        }
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!photonView.isMine)
            return;

        if (timeSizeLeft > 0)
            timeSizeLeft -= Time.deltaTime;

        if (controller.dpadAxis.x < -0.2 && timeSizeLeft <= 0)
        {
            photonView.RPC("ScaleDown", PhotonTargets.AllBufferedViaServer);
            timeSizeLeft = timeUpdate;
        }

        else if (controller.dpadAxis.x > 0.2 && timeSizeLeft <= 0)
        {
            photonView.RPC("ScaleUp", PhotonTargets.AllBufferedViaServer);
            timeSizeLeft = timeUpdate;
        }

        if (controller.triggerButtonDown)
        {
            photonView.RPC("EraseOn", PhotonTargets.AllBufferedViaServer);
        }

        if (controller.triggerButtonUp)
        {
            photonView.RPC("EraseOff", PhotonTargets.AllBufferedViaServer);
        }

        if (isGlobal)
        {
            mr.material = onMat;
        }

        else
        {
            mr.material = offMat;
        }

        if(isHeld && !isErasing)
        {
            mr.material = offMat;
        }

        else
        {
            mr.material = onMat;
        }

        if (isErasing)
        {
            GameObject other = eraser.GetComponent<TriggerEnter>().other;
            if (other.tag == "Trail" && other.name != "Bike")
            {
                photonView.RPC("EraseObject", PhotonTargets.AllBufferedViaServer, other.gameObject.GetComponent<ObjectID>().id);
            }
        }

    }

}
