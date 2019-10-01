using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//helper class to detect the triggers of other objects
public class TriggerEnter : MonoBehaviour
{
    [HideInInspector]
    public GameObject other;

    private void OnTriggerEnter(Collider otherThing)
    {
        other = otherThing.gameObject;
    }

    private void OnTriggerExit(Collider otherThing)
    {
        other = null;
    }
}
