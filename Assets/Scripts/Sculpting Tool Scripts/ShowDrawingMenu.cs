using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class ShowDrawingMenu : MonoBehaviour {

    public EVRButtonId button = EVRButtonId.k_EButton_ApplicationMenu;
    public float offset = 0.1f;
    protected int _index;
    public Transform trans;
    public WandController controller;

    // Use this for initialization
    void Start()
    {
        var trackedObject = GetComponent<SteamVR_TrackedObject>();

        if (trackedObject != null)
        {
            _index = (int)trackedObject.index;
            trans = trackedObject.transform;
        }
    }

    // Update is called once per frame
    void Update () {
        if (SteamVR_Controller.Input(_index).GetPressDown(button))
        {
            //PositionDrawingControls();
            //disabled this because tools don't go back properly
        }

    }

    void PositionDrawingControls()
    {
        if(controller.DrawingControlContainer != null)
        {
            if (!controller.DrawingControlContainer.gameObject.activeInHierarchy)
            {
                controller.DrawingControlContainer.right = Vector3.Cross(Vector3.up, transform.right);
                controller.DrawingControlContainer.position = trans.position + trans.forward * offset;
                
                controller.DrawingControlContainer.gameObject.SetActive(true);
            }
            else
            {
                controller.DrawingControlContainer.gameObject.SetActive(false);
            }
        }
    }
}
