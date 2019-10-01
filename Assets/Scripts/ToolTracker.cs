using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// These tools are used for raw data analytics
/// </summary>
public static class ToolTracker
{
    public static List<int> net;
    public static List<string> value;

    public static void Create() //max 6 entries for now
    {
        net = new List<int>();
        net.Add(0);
        net.Add(0);
        net.Add(0);
        net.Add(0);

        value = new List<string>();
        value.Add("0");
        value.Add("0");
        value.Add("0");
    }

    public static void setEmpty(int num) //num is how many digits we're keeping
    {
        for(int j=0; j< value.Count; j++)
        {
            if(j > num)
            {
                net[j] = 0;
            }
        }
    }


    public static string GetActions()
    {
        string returnString = "";

        for(int i = 0; i < 4; i++)
        {
            {
                returnString += " " + net[i];
            }
        }

        for (int i = 0; i < 3; i++)
        {
            {
                returnString += " " + value[i];
            }
        }

        return returnString;
    }
}
