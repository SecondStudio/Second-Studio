using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : Photon.MonoBehaviour
{
    public Transform wandTransform;

    public ushort pulseLength = 7000;

    #region Controller Defs

    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    [HideInInspector] public bool gripButtonDown = false;
    [HideInInspector] public bool gripButtonUp = false;
    [HideInInspector] public bool gripButtonPressed = false;

    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    [HideInInspector] public bool triggerButtonDown = false;
    [HideInInspector] public bool triggerButtonUp = false;
    [HideInInspector] public bool triggerButtonPressed = false;

    private Valve.VR.EVRButtonId dpadButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;
    [HideInInspector] public bool dpadButtonDown = false;
    [HideInInspector] public bool dpadButtonUp = false;
    [HideInInspector] public bool dpadButtonPressed = false;

    private Valve.VR.EVRButtonId appButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;
    [HideInInspector] public bool appButtonDown = false;
    [HideInInspector] public bool appButtonUp = false;
    [HideInInspector] public bool appButtonPressed = false;

    [HideInInspector] public bool dpadLeftDown = false;
    [HideInInspector] public bool dpadLeftUp = false;
    [HideInInspector] public bool dpadLeftPressed = false;

    [HideInInspector] public bool dpadRightDown = false;
    [HideInInspector] public bool dpadRightUp = false;
    [HideInInspector] public bool dpadRightPressed = false;

    private Valve.VR.EVRButtonId dpadUp = Valve.VR.EVRButtonId.k_EButton_DPad_Up;
    [HideInInspector] public bool dpadUpDown = false;
    [HideInInspector] public bool dpadUpUp = false;
    [HideInInspector] public bool dpadUpPressed = false;

    private Valve.VR.EVRButtonId dpadDown = Valve.VR.EVRButtonId.k_EButton_DPad_Down;
    [HideInInspector] public bool dpadDownDown = false;
    [HideInInspector] public bool dpadDownUp = false;
    [HideInInspector] public bool dpadDownPressed = false;

    public Vector2 dpadAxis = new Vector2(0, 0);

    public Transform DrawingControlContainer;
    #endregion

    public GameObject currentTool;

    public GameObject theForce;
    public GameObject Head;
    public GameObject grabTool;

    public WandController otherController;
    public bool isHoldingAnim;
    public GameObject controllerObject;
    [SerializeField]
    private SteamVR_TrackedObject trackedObj;
    public SteamVR_Controller.Device DeviceController { get { return SteamVR_Controller.Input((int)trackedObj.index); } }

    Renderer trailRenderer;
    Color trailColor;
    public float emission = 3.0f;
    public GameObject touchedTool;
    GameObject SketchContainer;
    Vector3 originalPos;
    Quaternion originalRot;
    Vector3 originalLocal;

    // Use this for initialization
    void Start()
    {
        if (trackedObj == null)
            trackedObj = controllerObject.GetComponent<SteamVR_TrackedObject>();

        wandTransform = transform;
        SketchContainer = GameObject.Find("Sketching Tools Container");
        currentTool = grabTool;
        currentTool.SetActive(true);

        ToolTracker.net[0] = 1;
        ToolTracker.net[1] = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.isMine)
            return;

        if (DeviceController == null)
        {
            Debug.Log("controller is not init");
            return;
        }

        #region Button Bools
        gripButtonDown = DeviceController.GetPressDown(gripButton);
        gripButtonUp = DeviceController.GetPressUp(gripButton);
        gripButtonPressed = DeviceController.GetPress(gripButton);

        triggerButtonDown = DeviceController.GetPressDown(triggerButton);
        triggerButtonUp = DeviceController.GetPressUp(triggerButton);
        triggerButtonPressed = DeviceController.GetPress(triggerButton);

        dpadButtonDown = DeviceController.GetPressDown(dpadButton);
        dpadButtonUp = DeviceController.GetPressUp(dpadButton);
        dpadButtonPressed = DeviceController.GetPress(dpadButton);

        dpadAxis = DeviceController.GetAxis();

        appButtonDown = DeviceController.GetPressDown(appButton);
        appButtonUp = DeviceController.GetPressUp(appButton);
        appButtonPressed = DeviceController.GetPress(appButton);

        dpadLeftDown = (dpadAxis.x < -0.8f && dpadButtonDown);
        dpadLeftUp = (dpadAxis.x < -0.8f && dpadButtonUp);
        dpadLeftPressed = (dpadAxis.x < -0.8f && dpadButtonPressed);

        dpadRightDown = (dpadAxis.x > 0.8f && dpadButtonDown);
        dpadRightUp = (dpadAxis.x > 0.8f && dpadButtonUp);
        dpadRightPressed = (dpadAxis.x > 0.8f && dpadButtonPressed);

        dpadDownDown = (dpadAxis.y < -0.8f && dpadButtonDown);
        dpadDownUp = (dpadAxis.y < -0.8f && dpadButtonUp);
        dpadDownPressed = (dpadAxis.y < -0.8f && dpadButtonPressed);

        dpadUpDown = (dpadAxis.y > 0.8f && dpadButtonDown);
        dpadUpUp = (dpadAxis.y > 0.8f && dpadButtonUp);
        dpadUpPressed = (dpadAxis.y > 0.8f && dpadButtonPressed);

        dpadAxis = DeviceController.GetAxis();
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


        if (currentTool == grabTool && gripButtonPressed)
            isHoldingAnim = true;
        else if (currentTool == grabTool && !gripButtonPressed)
            isHoldingAnim = false;

        if (currentTool != grabTool && gripButtonDown)
        {
            photonView.RPC("ReleaseTool", PhotonTargets.AllBufferedViaServer);
        }

        else
        {
            if (touchedTool)
            {
                if (!touchedTool.GetComponent<ToolBase>().isHeld && gripButtonPressed)
                {
                    photonView.RPC("SwitchTool", PhotonTargets.AllBufferedViaServer);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "trail")
        {
            DynamicGI.SetEmissive(trailRenderer, trailColor / emission);
            trailRenderer = null;
        }

        if (other.GetComponent<ToolBase>())
        {
            touchedTool = null;
        }
    }

    // Check for UI Collisions
    void OnTriggerStay(Collider other)
    {
        if (!photonView.isMine)
            return;

        //if (other.gameObject == currentTool)
            //return;

        #region Tool Events

        if (other.tag == "Trail")
        {
            trailRenderer = other.GetComponent<Renderer>();
            trailColor = trailRenderer.material.color;
            DynamicGI.SetEmissive(trailRenderer, trailColor * emission);
        }

        if(other.GetComponent<ToolBase>())
        {
            touchedTool = other.gameObject;
        }

        #endregion
    }

    #region PUN Events
  
    [PunRPC]
    void UseTheForce() //Luke
    {
        Debug.Log("You're a Jedi");
        touchedTool = theForce;
        SwitchTool();
        //ConstraintManager.ResetConstraints();
        //splineTool.GetComponent<SplineTool>().EndDrawing();
    } 
   
    /*
    [PunRPC]
    void UndoScene()
    {
        Debug.Log("Undo activated!");
        splineTool.GetComponent<SplineTool>().EndDrawing();
    }*/

    [PunRPC]
    void SwitchTool()
    {
        if (touchedTool.transform.parent.tag == "CenterPoint")
        {
            if (currentTool == grabTool)
            {
                DeviceController.TriggerHapticPulse(pulseLength);
                grabTool.GetComponent<GrabObjects>().StopScaling();

                currentTool = touchedTool;

                originalPos = currentTool.transform.parent.position;
                originalRot = currentTool.transform.parent.rotation;
                originalLocal = currentTool.transform.localPosition;

                currentTool.transform.parent.parent = transform;
                currentTool.transform.parent.position = transform.position;
                currentTool.transform.parent.localPosition = new Vector3(0, 0, .05f);
                currentTool.transform.parent.rotation = transform.rotation;
                currentTool.transform.parent.Rotate(Vector3.right, -90);


                currentTool.GetComponent<ToolBase>().controller = this;
                currentTool.GetComponent<ToolBase>().isHeld = true;
                currentTool.GetComponent<ToolBase>().enabled = true;

                isHoldingAnim = true;
            }
        }
    }

    [PunRPC]
    void ReleaseTool()
    {
        grabTool.GetComponent<GrabObjects>().StopScaling();

        if (grabTool.GetComponent<GrabObjects>().objectHeld != null)
        {
            var device = SteamVR_Controller.Input((int)trackedObj.index);
            GameObject temp = grabTool.GetComponent<GrabObjects>().objectHeld;
            grabTool.GetComponent<GrabObjects>().LetGo
                (temp.transform.position, temp.transform.rotation.eulerAngles, device.velocity, device.angularVelocity);
        }

        if (currentTool != grabTool)
        {
            currentTool.transform.parent.position = originalPos;
            currentTool.transform.parent.rotation = originalRot;
            currentTool.transform.localPosition = originalLocal;
            currentTool.transform.parent.parent = SketchContainer.transform;
            currentTool.GetComponent<ToolBase>().isHeld = false;
            currentTool.GetComponent<ToolBase>().enabled = false;
        }

        currentTool = grabTool;
        isHoldingAnim = false;

    }
    #endregion

}