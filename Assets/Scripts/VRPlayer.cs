using UnityEngine;
using System.Collections;
using Photon;
public class VRPlayer : Photon.MonoBehaviour
{
    public GameObject headCamera;
    public GameObject listener;
    public GameObject headObject;
    public GameObject leftObject;
    public GameObject rightObject;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(gameObject);
        if (!photonView.isMine)
        {
            headCamera.GetComponent<Camera>().enabled = false;
            listener.GetComponentInChildren<AudioListener>().enabled = false;
        }
        else if (photonView.isMine)
        {
            headCamera.GetComponent<Camera>().enabled = true;
            listener.GetComponentInChildren<AudioListener>().enabled = true;
        }

        if (PhotonNetwork.offlineMode) GetComponentInChildren<PhotonVoiceSpeaker>().enabled = false;
        if (PhotonNetwork.offlineMode) GetComponentInChildren<PhotonVoiceRecorder>().enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (photonView.isMine)
        {
            headObject.transform.position = ViveManager.Instance.head.transform.position;
            headObject.transform.rotation = ViveManager.Instance.head.transform.rotation;
            leftObject.transform.position = ViveManager.Instance.leftHand.transform.position;
            leftObject.transform.rotation = ViveManager.Instance.leftHand.transform.rotation;
            rightObject.transform.position = ViveManager.Instance.rightHand.transform.position;
            rightObject.transform.rotation = ViveManager.Instance.rightHand.transform.rotation;
        }
    }
}
