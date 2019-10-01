using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadModule : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Portal")
        {
            Debug.Log("Collision");
            SceneManager.LoadScene(1);
        }
    }
}
