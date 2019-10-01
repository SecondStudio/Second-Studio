using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the data type used to upload information about uses of tools
/// </summary>
public struct ToolData
{
    public string Name;
    public float Time;
    public int Count;

    public ToolData(string name, float time, int count)
    {
        Name = name;
        Time = time;
        Count = count;
    }
}
