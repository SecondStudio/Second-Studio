using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolWheel : MonoBehaviour {

    [SerializeField]
    public List<GameObject> Tools;
    public float Radius;
    
    int ToolCount { get { return Tools.Count; } }
    Transform Head;
    private float PrevRadius;
    public void Start()
    {
        Head = FindObjectOfType<VRPlayer>().headObject.transform;
        PositionTools();
    }

    private void Update()
    {
        if(Radius != PrevRadius)
        {
            PrevRadius = Radius;
            PositionTools();
        }
    }
    void PositionTools()
    {
        var angleIncrement = 360 / ToolCount;

        for(int i = 0; i < ToolCount; i++)
        {
            Tools[i].transform.parent = transform;
            Tools[i].transform.position = Quaternion.AngleAxis(i * angleIncrement, Vector3.up) * (Vector3.forward * Radius);
            Tools[i].transform.rotation = Quaternion.LookRotation((Tools[i].transform.position - transform.position).normalized , Vector3.up);
        }
    }

}
