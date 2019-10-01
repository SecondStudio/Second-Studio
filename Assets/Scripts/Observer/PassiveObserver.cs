using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveObserver : MonoBehaviour {
    public Transform cameraList;
    public GameObject cameraPrefab;
    private GameObject[] FirstPersons;

//    public Transform activeCameraList;

    int index = 0;
    int startCount;

	// Use this for initialization
	void Start () {
        ChangeCamera(cameraList.GetChild(index).gameObject);

        SetCameraScene();
    }

    // Update is called once per frame
    void Update () {

        FirstPersons = GameObject.FindGameObjectsWithTag("ActiveObserver");

        if (FirstPersons.Length != startCount)
            SetCameraScene();

        if (Input.GetKeyDown(KeyCode.Space) || ObserverController.triggerButtonDown)
        {
            ++index;
            if (index >= cameraList.childCount)
                index = 0;

            ChangeCamera(cameraList.transform.GetChild(index).gameObject);
        }

        if (cameraList.transform.GetChild(index).gameObject.tag == "FirstPersonCamera")
        {
            cameraList.GetChild(index).gameObject.transform.position = (FirstPersons[index-4].transform.position);
            cameraList.GetChild(index).gameObject.transform.rotation = (FirstPersons[index-4].transform.rotation);
        }
    }

    void ChangeCamera(GameObject newCamera)
    {
        // set the parent to new Camera
        transform.parent = newCamera.transform;

        // set the position and rotation to new Camera's default
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    void SetCameraScene()
    {
        FirstPersons = GameObject.FindGameObjectsWithTag("ActiveObserver");

        for (int i = 0; i < FirstPersons.Length; i++)
        {
            var newCam = Instantiate(cameraPrefab, FirstPersons[i].transform.position, FirstPersons[i].transform.rotation);
            newCam.transform.parent = cameraList;
        }
        startCount = FirstPersons.Length;
    }
}
