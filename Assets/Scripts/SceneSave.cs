using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class V3
{
    float x, y, z;
    public V3(Vector3 inVector)
    {
        x = inVector.x;
        y = inVector.y;
        z = inVector.z;
    }
    public Vector3 GetVector3()
    {
        Vector3 v = new Vector3();
        v.x = x;
        v.y = y;
        v.z = z;
        return v;
    }
}
[System.Serializable]
public class SceneSave {

    public string[] OBJStrings;
    public V3[] Positions;
    public V3[] Rotations;
    public V3[] Scales;
    public float[,] Colors;
    public int[] Groups;
}
