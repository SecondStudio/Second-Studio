using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectID : MonoBehaviour {

    public int id = 0;
    public Color ObjectColor;
    public bool HasParent = false;
    public MeshRenderer OutlineRenderer;
	// Use this for initialization
	void Start () {
        if (id == -1)
        {
            GetComponentInParent<ObjectManager>().AddObject(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr)
        {
            lr.SetPosition(0, transform.parent.position);
            lr.SetPosition(1, transform.position);
        }

	}

    public void SetId(int id)
    {
        this.id = id;
    }
}
