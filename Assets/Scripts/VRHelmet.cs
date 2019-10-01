using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class VRHelmet : MonoBehaviour
{

    public UnityEvent OnWear;
    public UnityEvent UnWear;
    private ParticleSystem TouchParticles;
    bool held;
    bool worn;
    GameObject go;
    GameObject holdingObject = null;
    GameObject objectDropped;
    Rigidbody attachPoint;
    public float snapDistance;
    public float wornDistance;
    public SteamVR_TrackedObject trackedObjLeft;
    public SteamVR_TrackedObject trackedObjRight;
    FixedJoint joint;
    public GameObject headCamera;
    private float timeUpdate = 0.02f;
    float timeLeft = 0.0f;
    SteamVR_Controller.Device controllerIn;
    //private SteamVR_Controller.Device controllerLeft { get { return SteamVR_Controller.Input((int)trackedObjLeft.index); } }
    //private SteamVR_Controller.Device controllerRight { get { return SteamVR_Controller.Input((int)trackedObjRight.index); } }
    enum handUsed { left, right, both, none};
    handUsed heldHand;

    public float ResetSnapDistance;


    //unfinished, meant for both hands, now only does left


    void Start()
    {
        held = false;
        worn = false;
        heldHand = handUsed.none;
    }

    void Update()
    {
        if (timeLeft > 0)
            timeLeft -= Time.deltaTime;

        if (worn && !held)
        {
            transform.position = headCamera.transform.position;
        }

        if (holdingObject == null && go != null)
        {
            if(controllerIn.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
            {
                print("Grabbing");
                heldHand = handUsed.left;
                Grab(go);
            }


           
        }

        else if (holdingObject != null && controllerIn.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip)  && timeLeft <= 0)
        {
            timeLeft = timeUpdate;
        }
        else if (holdingObject != null && controllerIn.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            LetGo();
        }

        if(!held && transform.parent.position != transform.position &&!worn)
        {
            /*if((transform.parent.position - transform.position).magnitude < ResetSnapDistance)
            {
                transform.position = transform.parent.position;
            } else*/
            {
                transform.position = Vector3.MoveTowards(transform.position,transform.parent.position, 1.0f * Time.deltaTime);
            }
        }

        if (Vector3.Distance(transform.position, headCamera.transform.position) < snapDistance && !worn && !held)
        {
            worn = true;
            LetGo();
            transform.position = headCamera.transform.position;
            OnWear.Invoke(); 
            print("hat worn");
        }
        else if(Vector3.Distance(transform.position, headCamera.transform.position) > snapDistance || held)
            worn = false;

        if(Vector3.Distance(transform.position, headCamera.transform.position) < wornDistance && held)
        {
            OnWear.Invoke();
        }
        else if(Vector3.Distance(transform.position, headCamera.transform.position) > wornDistance && held)
        {
            UnWear.Invoke();
        }
    }

    private void Awake()
    {
        TouchParticles = GetComponent<ParticleSystem>();
        if (OnWear == null)
        {
            OnWear = new UnityEvent();
        }
    }

    void Grab(GameObject go)
    {
        holdingObject = go;
        held = true;
        worn = false;
        UnWear.Invoke();
        print("unworn");

        joint = holdingObject.AddComponent<FixedJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();
    }

    void LetGo()
    {
        held = false;

        if (Vector3.Distance(transform.position, headCamera.transform.position) < snapDistance)
        {
            transform.position = headCamera.transform.position;
            worn = true;
            OnWear.Invoke();
        }

        else
        {
            worn = false;
        }

        Destroy(holdingObject.GetComponent<Joint>());
        //objectDropped = objectHeld;
        holdingObject = null;
    }

    private void OnTriggerEnter(Collider col)
    {
        print("helmet touched");

        if(col.tag == ("WandController"))
        {
            controllerIn = SteamVR_Controller.Input((int)col.gameObject.GetComponent<SteamVR_TrackedObject>().index);
            attachPoint = col.gameObject.GetComponent<Rigidbody>();
        }

        if (TouchParticles != null) TouchParticles.Play();

        if (col.transform.CompareTag("Observer"))
        {
            print("button pressed by controller");

            OnWear.Invoke();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("WandController"))
        {
            go = other.gameObject;

            if (other.gameObject.name == "Controller (left)")
            {
                heldHand = handUsed.left;

                if (other.gameObject.name == "Controller (right)")
                {
                    heldHand = handUsed.both;
                }
            }
            else
                heldHand = handUsed.right;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("WandController"))
            go = null;

        heldHand = handUsed.none;
    }
}
