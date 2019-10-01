using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All of the tools are children of this class
/// </summary>
public abstract class ToolBase : Photon.MonoBehaviour {

    public ToolSettings[] SettingsPages;
    public ToolSettings ActiveToolset;


    [HideInInspector] public int ActiveIndex = 0;
    [HideInInspector] public WandController controller;
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public bool isClone = false;
    protected string trackerLetter = "";

    //cycles through the various settings
    protected virtual void Start()
    {
        if (SettingsPages != null && ActiveIndex < SettingsPages.Length)
            ActiveToolset = SettingsPages[ActiveIndex];

        SettingsPages = GetComponentsInChildren<ToolSettings>();
        if(SettingsPages.Length > 0)
        {
            ActiveToolset = SettingsPages[0];
        }

        for (int i = 0; i < SettingsPages.Length; i++)
        {
            SettingsPages[i].gameObject.SetActive(ActiveIndex == i);
        }
    }

    protected virtual void OnEnable()
    {
        if(trackerLetter != "")
            GameObject.Find("Tracker").GetComponent<TrackerScript>().UpdateTool(trackerLetter);
    }

    protected virtual void OnDisable()
    {
        //trackerLetter = "U";
    }

    public void CycleSettings()
    {
        photonView.RPC("ChangeSettingsPage", PhotonTargets.AllBufferedViaServer);
    }


    [PunRPC]
    public void ChangeSettingsPage()
    {
        ActiveToolset.gameObject.SetActive(false);
        ActiveIndex = ActiveIndex + 1 >= SettingsPages.Length ? 0 : ActiveIndex + 1;
        ActiveToolset = SettingsPages[ActiveIndex];
        ActiveToolset.gameObject.SetActive(true);
    }

    protected virtual void Update()
    {
        if (controller == null)
            return;

        if (isHeld)
        {
            if (controller.dpadLeftDown)
            {
                print("Left tool option pressed");
                ActiveToolset.OnLeft.Invoke();
            }
            else if (controller.dpadRightDown)
            {
                print("Right tool option pressed");
                ActiveToolset.OnRight.Invoke();
            }
            else if (controller.dpadUpDown)
            {
                print("Up tool option pressed");
                ActiveToolset.OnUp.Invoke();
            }
            else if (controller.dpadDownDown)
            {
                print("Down tool option pressed");
                ActiveToolset.OnDown.Invoke();
            }
        }
    }
}
