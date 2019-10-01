using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        AudioSource _as = gameObject.GetComponent<AudioSource>();
        _as.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
