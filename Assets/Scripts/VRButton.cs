using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent( typeof(Collider) )]
public class VRButton : MonoBehaviour {

    public UnityEvent OnTouched;
    public ushort PulseTime = 10000;
    private ParticleSystem TouchParticles;
    private void Awake()
    {
        TouchParticles = GetComponent<ParticleSystem>();
        if (OnTouched == null)
        {
            OnTouched = new UnityEvent();
            
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!col.transform.CompareTag("WandController")) return;
        print("button pressed");
        //SteamVR_Controller.Input((int)col.gameObject.GetComponent<SteamVR_TrackedObject>().index).TriggerHapticPulse(PulseTime);
        if (TouchParticles != null) TouchParticles.Play();
        OnTouched.Invoke();
    }


}
