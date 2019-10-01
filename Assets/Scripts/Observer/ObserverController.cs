using UnityEngine;
using System.Collections;

public class ObserverController : Photon.MonoBehaviour
{
    public static Transform wandTransform;

    #region Controller Defs

    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    public static bool gripButtonDown = false;
    public static bool gripButtonUp = false;
    public static bool gripButtonPressed = false;

    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    public static bool triggerButtonDown = false;
    public static bool triggerButtonUp = false;
    public static bool triggerButtonPressed = false;

    private Valve.VR.EVRButtonId dpadLeft = Valve.VR.EVRButtonId.k_EButton_DPad_Left;
    public static bool dpadLeftDown = false;
    public bool dpadLeftUp = false;
    public bool dpadLeftPressed = false;

    private Valve.VR.EVRButtonId dpadRight = Valve.VR.EVRButtonId.k_EButton_DPad_Right;
    public static bool dpadRightDown = false;
    public bool dpadRightUp = false;
    public bool dpadRightPressed = false;

    private Valve.VR.EVRButtonId dpadUp = Valve.VR.EVRButtonId.k_EButton_DPad_Up;
    public static bool dpadUpDown = false;
    public bool dpadUpUp = false;
    public bool dpadUpPressed = false;

    private Valve.VR.EVRButtonId dpadDown = Valve.VR.EVRButtonId.k_EButton_DPad_Down;
    public static bool dpadDownDown = false;
    public bool dpadDownUp = false;
    public bool dpadDownPressed = false;

    public static Vector2 dpadAxis = new Vector2(0, 0);

    #endregion

    public GameObject controllerObject;
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }


    // Use this for initialization
    void Start()
    {
        trackedObj = gameObject.GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine)
            return;

        if (controller == null)
        {
            Debug.Log("controller is not init");
            return;
        }

        #region Button Bools
        gripButtonDown = controller.GetPressDown(gripButton);
        gripButtonUp = controller.GetPressUp(gripButton);
        gripButtonPressed = controller.GetPress(gripButton);

        triggerButtonDown = controller.GetPressDown(triggerButton);
        triggerButtonUp = controller.GetPressUp(triggerButton);
        triggerButtonPressed = controller.GetPress(triggerButton);

        dpadLeftDown = controller.GetPressDown(dpadLeft);
        dpadLeftUp = controller.GetPressUp(dpadLeft);
        dpadLeftPressed = controller.GetPress(dpadLeft);

        dpadRightDown = controller.GetPressDown(dpadRight);
        dpadRightUp = controller.GetPressUp(dpadRight);
        dpadRightPressed = controller.GetPress(dpadLeft);

        dpadUpDown = controller.GetPressDown(dpadUp);
        dpadUpUp = controller.GetPressUp(dpadUp);
        dpadUpPressed = controller.GetPress(dpadUp);

        dpadDownDown = controller.GetPressDown(dpadDown);
        dpadDownUp = controller.GetPressUp(dpadDown);
        dpadDownPressed = controller.GetPress(dpadDown);

        dpadAxis = controller.GetAxis();
        #endregion

        #region Actions for Controllers
        if (gripButtonDown)
        {
            Debug.Log("grip btn was pressed");
        }

        if (gripButtonUp)
        {
            Debug.Log("grip btn was released");
        }

        if (triggerButtonDown)
        {
            Debug.Log("trigger was pressed");
        }

        if (triggerButtonUp)
        {
            Debug.Log("trigger was released");
        }

        if (dpadLeftDown)
        {
            Debug.Log("dpadLeft was pressed");
        }

        if (dpadLeftUp)
        {
            Debug.Log("dpadLeft was released");
        }

        if (dpadRightDown)
        {
            Debug.Log("dpadRight was pressed");
        }

        if (dpadRightUp)
        {
            Debug.Log("dpadRight was released");
        }

        if (dpadUpDown)
        {
            Debug.Log("dpadUp was pressed");
        }

        if (dpadUpUp)
        {
            Debug.Log("dpadUp was released");
        }

        if (dpadDownDown)
        {
            Debug.Log("dpadDown was pressed");
        }

        if (dpadDownUp)
        {
            Debug.Log("dpadDown was released");
        }
        #endregion


    }
}
