using UnityEngine;
using System.Collections;

public class CameraScreen : MonoBehaviour
{
    public Transform cameraParent;
    private Camera[] cameraList;
    public Camera cam;
    int index = 0;

    void Start()
    {
        cameraList = cameraParent.GetComponentsInChildren<Camera>();

        for (int i = 0; i < cameraList.Length; ++i)
        {
            if(cameraList[i] != cam)
                cameraList[i].enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cam.enabled = false;
            ++index;
            if (index >= cameraList.Length)
                index = 0;

            cam = cameraList[index];
            cam.enabled = true;
        }
        */
        RenderTexture rtt = cam.targetTexture;
        GetComponent<Renderer>().material.SetTexture("_MainTex", rtt);
    }
}