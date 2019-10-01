using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomJoinHats : MonoBehaviour
{
    public GameObject PortalScreen;
    public GameObject PortalParticle;
    public GameObject PortalEffects;
    public GameObject Tinter;
    public Material activeMaterial;
    public Material passiveMaterial;
    public Material theatreMaterial;
    public Material defaultMaterial;

    void Start()
    {
        PortalScreen.SetActive(false);
        PortalParticle.SetActive(false);
        PortalEffects.GetComponent<Renderer>().material = defaultMaterial;
        Tinter.SetActive(false);
    }

    void Update()
    { 

    }

    public void JoinAsActiveVR()
    {
        Debug.Log("Active VR pressed");
        NetworkManager.buttonPressed = 1;
        PortalScreen.SetActive(true);
        PortalParticle.SetActive(true);
        Tinter.SetActive(true);
        PortalParticle.GetComponent<ParticleSystemRenderer>().material = activeMaterial;
        PortalEffects.GetComponent<Renderer>().material = activeMaterial;
        Tinter.GetComponent<Image>().color = new Color(1,0,0,0.1f);
    }

    public void JoinAsPassiveVR()
    {
        NetworkManager.buttonPressed = 2;
        PortalScreen.SetActive(true);
        PortalParticle.SetActive(true);
        Tinter.SetActive(true);
        PortalParticle.GetComponent<ParticleSystemRenderer>().material = passiveMaterial;
        PortalEffects.GetComponent<Renderer>().material = passiveMaterial;
        Tinter.GetComponent<Image>().color = new Color(1,0.5f,0,0.1f);
    }

    public void JoinAsVRTheater()
    {
        NetworkManager.buttonPressed = 4;
        PortalScreen.SetActive(true);
        PortalParticle.SetActive(true);
        Tinter.SetActive(true);
        PortalParticle.GetComponent<ParticleSystemRenderer>().material = theatreMaterial;
        PortalEffects.GetComponent<Renderer>().material = theatreMaterial;
        Tinter.GetComponent<Image>().color = new Color(0, 1, 0, 0.1f);
    }

    public void UnJoin()
    {
        PortalScreen.SetActive(false);
        PortalParticle.SetActive(false);
        PortalEffects.GetComponent<Renderer>().material = defaultMaterial;
        Tinter.SetActive(false);
    }
}
