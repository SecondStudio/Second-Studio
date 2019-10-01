using UnityEngine;
using System.Collections.Generic;

public class GrabObjects : Photon.MonoBehaviour
{
    public GameObject go;
    public GameObject objectHeld;
    public GameObject ObjectToScale;
    Transform objectParent;
    public Rigidbody attachPoint;
    public bool IsLeftHand;
    public SteamVR_TrackedObject trackedObj;
    FixedJoint joint;
    public WandController leftController, rightController;
    public GrabObjects OtherController;
    private float timeUpdate = 0.02f;
    float timeLeft = 0.0f;
    public bool CanRelease = true;
    public bool IsScaling = false;
    private float ControllerStartDistance;
    Vector3 StartScale;
    public Material OutlineMaterial;

    void Start()
    {
        if(trackedObj == null)
            trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    protected virtual void Update()
    {
        if (!photonView.isMine)
            return;

        if (timeLeft > 0)
            timeLeft -= Time.deltaTime;
        var device = SteamVR_Controller.Input((int)trackedObj.index);

        if (IsScaling)
        {
            if ((IsLeftHand ? leftController.gripButtonUp : rightController.gripButtonUp))
            {
                photonView.RPC("StopScaling", PhotonTargets.AllBufferedViaServer);
            } else
            {
                photonView.RPC("SetScale", PhotonTargets.AllBufferedViaServer);
            }
        }
        if (objectHeld == null && go != null && (IsLeftHand ? leftController.gripButtonDown : rightController.gripButtonDown))
        {
            if(OtherController.objectHeld == go)
            {
                photonView.RPC("StartScaling" , PhotonTargets.AllBufferedViaServer );
                
            } else
            {
                photonView.RPC("Grab", PhotonTargets.AllBufferedViaServer, go.GetComponent<ObjectID>().id , go.transform.position);
            }
            
        }
        else if (objectHeld != null && (IsLeftHand ? leftController.gripButtonPressed : rightController.gripButtonPressed) && timeLeft <= 0)
        {
            photonView.RPC("SetTransform", PhotonTargets.AllBufferedViaServer, objectHeld.transform.position, objectHeld.transform.rotation.eulerAngles);
            timeLeft = timeUpdate;
        }
        else if (objectHeld != null && CanRelease && (IsLeftHand ? leftController.gripButtonUp : rightController.gripButtonUp))
        {
            photonView.RPC("LetGo", PhotonTargets.AllBufferedViaServer, objectHeld.transform.position, objectHeld.transform.rotation.eulerAngles, device.velocity, device.angularVelocity);
        }
        foreach (var col in Physics.OverlapSphere(transform.position, 0.75f))
        {
            var rigid = col.GetComponent<Rigidbody>();
            if (rigid != null) rigid.WakeUp();
        }
    }

    public void OnEnable()
    {
        foreach (var t in GameObject.FindGameObjectsWithTag("Trail"))
        {
            if (t.GetComponent<ObjectID>() == null) continue;
            t.GetComponent<ObjectID>().OutlineRenderer.material = OutlineMaterial;
            t.GetComponent<ObjectID>().OutlineRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Grabs the scene object specified by the identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="position">The position the grab action took place at.</param>
    [PunRPC]
    protected void Grab(int id , Vector3 position)
    {
        objectHeld = ObjectManager.instance.FindObject(id);
        objectHeld.transform.position = position;
        objectParent = objectHeld.transform.parent;
        objectHeld.transform.parent = transform;
        var rigidbody = objectHeld.GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        joint = objectHeld.AddComponent<FixedJoint>();
        joint.connectedBody = attachPoint;
    }


    [PunRPC]
    protected void LetGo()
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        
        var rigidbody = objectHeld.GetComponent<Rigidbody>();
        Object.DestroyImmediate(joint);
        joint = null;

        var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
        if (origin != null)
        {
            rigidbody.velocity = origin.TransformVector(device.velocity);
            rigidbody.angularVelocity = origin.TransformVector(device.angularVelocity);
        }
        else
        {
            rigidbody.velocity = device.velocity;
            rigidbody.angularVelocity = device.angularVelocity;
        }

        rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
        
        objectHeld.transform.parent = objectParent;

        // set position and euler angles

        objectHeld = null;
    }

    [PunRPC]
    public void LetGo(Vector3 pos, Vector3 angles, Vector3 velocity, Vector3 angularVelocity)
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        Destroy(joint);
        var rigidbody = objectHeld.GetComponent<Rigidbody>();

        rigidbody.isKinematic = !ObjectManager.instance.gravity;
        rigidbody.useGravity = ObjectManager.instance.gravity;
        var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
        if (origin != null)
        {
            rigidbody.velocity = origin.TransformVector(velocity);
            rigidbody.angularVelocity = origin.TransformVector(angularVelocity);
        }
        else
        {
            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
        }
        objectHeld.transform.position = GridSnapTool.Snap(pos , objectHeld);
        objectHeld.transform.rotation = Quaternion.Euler(angles);
        objectHeld.transform.parent = objectParent;
        var col = objectHeld.GetComponent<MeshCollider>();
        objectHeld = null;
    }

    [PunRPC]
    protected void SetTransform(Vector3 pos, Vector3 angles)
    {
        objectHeld.transform.position = pos;//GridSnapTool.SnapToGrid(pos);
        objectHeld.transform.rotation = Quaternion.Euler(angles);
    }


    [PunRPC]
    protected void StartScaling()
    {
        ObjectToScale = OtherController.objectHeld;
        IsScaling = true;
        //OtherController.CanRelease = false;
        ControllerStartDistance = Vector3.Distance(transform.position, OtherController.transform.position);
        StartScale = ObjectToScale.transform.localScale;
    }

    [PunRPC]
    protected void SetScale()
    {
        Vector3 newScale = StartScale * (Vector3.Distance(transform.position, OtherController.transform.position) / ControllerStartDistance);
        if (ConstraintManager.ConstrainX)
            newScale.x = StartScale.x;
        if (ConstraintManager.ConstrainY)
            newScale.y = StartScale.y;
        if (ConstraintManager.ConstrainZ)
            newScale.z = StartScale.z;
        ObjectToScale.transform.localScale = newScale;
        //ObjectToScale.transform.position = 0.5f * (transform.position + OtherController.transform.position);
    }

    [PunRPC]
    public void StopScaling()
    {
        ObjectToScale = null;
        IsScaling = false;
        //OtherController.CanRelease = true;
    }
    /// <summary>
    /// Keeps a reference to the current colliding scene object.
    /// </summary>
    /// <param name="other">The other.</param>
    void OnTriggerStay(Collider other)
    {
        
        if (other.gameObject.CompareTag("Trail"))
        {
            //Move up to the objects parent until the object is parentless
            //The parent of the trail object represents the group it is a part of
            Transform topLevelTransform = other.transform;
            go = null;
            while (topLevelTransform.parent != null && !topLevelTransform.parent.CompareTag("TrailContainer") && topLevelTransform.parent.GetComponent<GrabObjects>() == null)
            {
                topLevelTransform = topLevelTransform.parent;
            }
            go = topLevelTransform.gameObject;
            foreach(var r in go.GetComponentsInChildren<ObjectID>())
            {
                if (r.OutlineRenderer)
                {
                    r.OutlineRenderer.enabled = true;
                    r.GetComponent<ObjectID>().OutlineRenderer.material.color = Color.green;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == go)
        {
            go = null;
        }

    }
}
