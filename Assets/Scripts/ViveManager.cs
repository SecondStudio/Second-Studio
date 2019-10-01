using UnityEngine;
using System.Collections;

public class ViveManager : MonoBehaviour {

    public GameObject head;
    public GameObject rightHand;
    public GameObject leftHand;

    public static ViveManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
