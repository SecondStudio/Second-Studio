using UnityEngine;
using System.Collections;

public class Observer : Photon.MonoBehaviour {

    public float speed = 5.0f;
    bool inMenu = false;
    Vector3 mainPos;
    Vector3 mainRot;
    Vector3 menuPos;
    Vector3 menuRot;
    Transform grabbedTransform;
    RaycastHit hitObject;
    public Rigidbody Bullet;
    GameObject go;

	// Use this for initializations
	void Start () {

        if (!photonView.isMine)
            transform.GetChild(0).GetComponent<Camera>().enabled = false;
        else if (photonView.isMine)
            transform.GetChild(0).GetComponent<Camera>().enabled = true;


        mainPos = transform.position;
        mainRot = transform.eulerAngles;
        //menuPos = GameObject.Find("Ghost").transform.position;
        //menuRot = GameObject.Find("Ghost").transform.eulerAngles;
        grabbedTransform = null;
    }

    // Update is called once per frame
    void Update () {

        if (!photonView.isMine)
            return;

        transform.Translate(Input.GetAxis("LeftJoystickX") * speed * Time.deltaTime, 0, 0);
        transform.Translate(0, 0, -1 * Input.GetAxis("LeftJoystickY") * speed * Time.deltaTime);
        transform.Rotate(0, Input.GetAxis("RightJoystickX") * Time.deltaTime * 20.0f * speed, 0);
        transform.GetChild(0).Rotate(Input.GetAxis("RightJoystickY") * Time.deltaTime * 20.0f * speed, 0, 0);
        transform.Translate(0, -1 * Input.GetAxis("LeftTrigger") * speed * 0.7f * Time.deltaTime, 0);
        transform.Translate(0, 1 * Input.GetAxis("RightTrigger") * speed * 0.7f * Time.deltaTime, 0);

        if (grabbedTransform && Vector3.Distance(grabbedTransform.position, transform.position) > 1.0f)
        {
            grabbedTransform.position = Vector3.MoveTowards(grabbedTransform.position, transform.position + transform.forward, 10.0f * Time.deltaTime);
        }


        if (inMenu)
        {
            GameObject.Find("Ghost").transform.position = transform.position;
            GameObject.Find("Ghost").transform.rotation = transform.rotation;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.position -= transform.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.J))
        {
            transform.Rotate(Vector3.up, -speed * 20.0f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.L))
        {
            transform.Rotate(Vector3.up, speed * 20.0f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.I))
        {
            transform.GetChild(0).Rotate(Vector3.right, -speed * 20.0f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.K))
        {
            transform.GetChild(0).Rotate(Vector3.right, speed * 20.0f * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, 10) && hit.transform.tag == "Trail")
            {
                print("theres a trail at distance "+hit.distance);
                grabbedTransform = hit.transform;
                hitObject = hit;
                hitObject.rigidbody.useGravity = false;
                hitObject.rigidbody.isKinematic = true;

                Transform topLevelTransform = hit.transform;

                while (topLevelTransform.parent != null && !topLevelTransform.parent.CompareTag("TrailContainer"))
                {
                    topLevelTransform = topLevelTransform.parent;
                }

                go = topLevelTransform.gameObject;

                photonView.RPC("Grab", PhotonTargets.OthersBuffered, go.GetComponent<ObjectID>().id);
            }
        }
        if(Input.GetKey(KeyCode.JoystickButton5))
        {
            photonView.RPC("SetTransform", PhotonTargets.OthersBuffered, go.transform.position, go.transform.rotation.eulerAngles);
        }
        if(Input.GetKeyUp(KeyCode.JoystickButton5))
        {
            hitObject.rigidbody.useGravity = true;
            hitObject.rigidbody.isKinematic = false;
            hitObject.rigidbody.AddForce(transform.forward * 1000.0f);

            photonView.RPC("LetGo", PhotonTargets.OthersBuffered, go.transform.position, go.transform.rotation.eulerAngles, hitObject.rigidbody.velocity, hitObject.rigidbody.angularVelocity);

            go = null;
            hitObject = new RaycastHit();
            grabbedTransform = null;

        }
        if(Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            Rigidbody bullet = (Rigidbody) Instantiate(Bullet, transform.position + transform.forward, transform.rotation);
            bullet.velocity = transform.forward * 10.0f;
        }
        //for pause menu - disabled for now
        /*
        if (Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (!inMenu)
            {
                mainPos = transform.position;
                mainRot = transform.eulerAngles;
                transform.position = menuPos;
                transform.eulerAngles = menuRot;

                transform.GetChild(0).GetComponent<Camera>().enabled = false;
                GameObject.Find("GhostCamera").GetComponent<Camera>().enabled = true;
                inMenu = true;
            }
            else if(inMenu)
            {
                menuPos = transform.position;
                menuRot = transform.eulerAngles;
                transform.position = mainPos;
                transform.eulerAngles = mainRot;

                transform.GetChild(0).GetComponent<Camera>().enabled = true;
                GameObject.Find("GhostCamera").GetComponent<Camera>().enabled = false;
                inMenu = false;
            }
        }*/

    }
    
    [PunRPC]
    void Grab(int id)
    {
        go = ObjectManager.instance.FindObject(id);
        var rigidbody = go.GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
    }

    [PunRPC]
    void LetGo(Vector3 pos, Vector3 angles, Vector3 velocity, Vector3 angularVelocity)
    {
        var rigidbody = go.GetComponent<Rigidbody>();

        rigidbody.isKinematic = !ObjectManager.instance.gravity;

        rigidbody.velocity = velocity;
        rigidbody.angularVelocity = angularVelocity;
        rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
        grabbedTransform.position = pos;
        grabbedTransform.rotation = Quaternion.Euler(angles);

        hitObject = new RaycastHit();
    }

    [PunRPC]
    void SetTransform(Vector3 pos, Vector3 angles)
    {
        hitObject.transform.position = pos;
        hitObject.transform.rotation = Quaternion.Euler(angles);
    }
}
