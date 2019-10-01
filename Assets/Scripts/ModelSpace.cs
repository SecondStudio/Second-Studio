using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ModelSpace : MonoBehaviour {

    [System.Serializable]
    public class ModelTouchEvent : UnityEvent<int>
    {
    }

    public UnityEvent OnTouched;
    private int MyId;
    // Use this for initialization
	void Start () {
        MyId = transform.GetSiblingIndex();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WandController"))
        {
            OnTouched.Invoke();
        }
    }
}
