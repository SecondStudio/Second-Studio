using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Primitives are objects that are under the Primitives gameobject and set in the array here
/// the primitives are all prefabs, but they do not have to be
/// some of the primitives are generated through scripts, instead of importing anything
/// this includes plane,cubetube,circleplane,circletube
/// </summary>
public class PrimitiveTool : ToolBase
{
    public GameObject[] Primitives;
    int CurrentIndex = 0;
    private GameObject CreatedObject;
    private Vector3 StartPoint , InitialScale;
    private bool Holding;

    protected override void Start()
    {
        base.Start();
        Primitives[CurrentIndex].SetActive(true);
        trackerLetter = "I";
    }

    protected override void Update()
    {
        base.Update();

        if (controller.triggerButtonUp)
        {
            photonView.RPC("EndPlacement", PhotonTargets.AllBufferedViaServer);
        }
        if (Holding)
        {
            CreatedObject.transform.localScale = InitialScale * (Vector3.Distance(StartPoint, transform.position) + .1f);
        }
        if (controller.triggerButtonDown)
        {
            photonView.RPC("StartPlacement", PhotonTargets.AllBufferedViaServer);
        }
    }


    [PunRPC]
    void UseTool(int index)
    {
        Primitives[CurrentIndex].SetActive(false);
        CurrentIndex = index;
        Primitives[CurrentIndex].SetActive(true);
    }

    [PunRPC]
    void StartPlacement()
    {
        CreatedObject = Instantiate(Primitives[CurrentIndex], Primitives[CurrentIndex].transform.position, Primitives[CurrentIndex].transform.rotation);
        CreatedObject.transform.localScale = new Vector3(.3f, .3f, .3f);
        InitialScale = CreatedObject.transform.localScale;
        StartPoint = transform.position;
        Holding = true;
    }

    [PunRPC]
    void EndPlacement()
    {
        Holding = false;
        CreatedObject.GetComponent<MeshEditor>().enabled = true;
        CreatedObject.GetComponent<MeshEditor>().StartGroupGeneration();
        CreatedObject.tag = "Trail";
        ObjectManager.instance.AddObject(CreatedObject);
        CreatedObject = null;
    }

    public void NextTool()
    {
        int index = CurrentIndex + 1 >= Primitives.Length ? 0 : CurrentIndex + 1;
        photonView.RPC("UseTool", PhotonTargets.AllBufferedViaServer, index);
    }

    public void PrevTool()
    {
        int index = CurrentIndex - 1 < 0 ? Primitives.Length - 1 : CurrentIndex - 1;
        photonView.RPC("UseTool", PhotonTargets.AllBufferedViaServer, index);
    }
}
