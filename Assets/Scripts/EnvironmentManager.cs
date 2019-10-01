using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnvironmentManager : MonoBehaviour
{
    public GameObject left;
    public GameObject right;
    public Material[] env;
    public Material m1;
    public Material m2;
    public Material m3;
    public Material m4;
    public Material m5;
    public Material m6;
    public Material m7;
    public Material m8;
    public Material m9;
    public Material m10;
    int index = 0;

    void Start()
    {
        env = new Material[10];
        env[0] = m1;
        env[1] = m2;
        env[2] = m3;
        env[3] = m4;
        env[4] = m5;
        env[5] = m6;
        env[6] = m7;
        env[7] = m8;
        env[8] = m9;
        env[9] = m10;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WandController")
        {
            if (this.gameObject == left)
            {
                Debug.Log("left");
                if (index == 0) { index = 9; }
                else { index = index - 1; }
                RenderSettings.skybox = env[index];
                Debug.Log("Env moved left");
            }
            if(this.gameObject == right)
            {
                Debug.Log("right");
                if (index == 9) { index = 0; }
                else { index = index + 1; }
                RenderSettings.skybox = env[index];
                Debug.Log("Env moved right");
            }
        }

        //SteamVR_Controller.Input((int)col.gameObject.GetComponent<SteamVR_TrackedObject>().index).TriggerHapticPulse(1000);

    }
}