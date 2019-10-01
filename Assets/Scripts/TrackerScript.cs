using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Analytics;

//initializing needed data types to upload correctly to AWS
[System.Serializable]
public class TrackerItem
{
    public int numMesh;
    public int numErase;
    public float userHeight;
    public float roomWidth;
    public float roomHeight;

    public float timeSpline;
    public float numSpline;
    public float timeEraser;
    public float numEraser;
    public float timeMirror;
    public float numMirror;
    public float timeLine;
    public float numLine;
    public float timeCamera;
    public float numCamera;
    public float timeBoolean;
    public float numBoolean;
    public float timeClone;
    public float numClone;
    public float timeDrawFace;
    public float numDrawFace;
    public float timeExtrude;
    public float numExtrude;
    public float timeLoft;
    public float numLoft;
    public float timeGroup;
    public float numGroup;
    public float timePrimitive;
    public float numPrimitive;
    public float timeClipping;
    public float numClipping;
    public float timeSelection;
    public float numSelection;
    public float timePolygon;
    public float numPolygon;
    public float timePaint;
    public float numPaint;
    public float timeGravity;
    public float numGravity;
    public float timeExtrudeCurve;
    public float numExtrudeCurve;
    public float timeMeasuring;
    public float numMeasuring;

    public float timeInactive;
    public float timeSpent;
    public string mostChosenTool;
    public string longestUsedTool;
    public string mostChosenToolPair;
    public bool isOnline;
    public bool isHost;
    public int maxPeople;
    public int maxActive;
    public int maxPassiveController;
    public int maxPassiveVR;
    public int maxTheatre;
    public string commonShape;
    public string commonSection;
    public string shapeVsSection;
    public string avgColor;
    public int numSaves;
    public float avgSaveSize;
    public string Timestamp;
}
//to upload things, they need an item inside of a body, which will then be loaded into a payload to be uploaded
[System.Serializable]
public class RawDataItem
{
    public string rawString;
    public string Timestamp;
}

[System.Serializable]
public class BodyAnalytics
{
    public BodyAnalytics() { Item = new TrackerItem(); }
    public TrackerItem Item;
}

[System.Serializable]
public class BodyRawData
{
    public BodyRawData() { Item = new RawDataItem(); }
    public RawDataItem Item;
}


[System.Serializable]
public class Payload<T>
{
    public Payload() {  }
    public T body;
}

/// <summary>
/// There's a lot going on here, but all of the analytics are handled here.
/// </summary>
public class TrackerScript : MonoBehaviour
{
    bool quit;
    public int numMesh = 0;
    public int numErase = 0;
    public List <Color> colors = new List<Color>();
    public string toolListString = "";
    Dictionary<string , ToolData> toolMap; //dictionary with letter to ToolData
    public float timeCurrentTool = 0;
    public float timeInactive = 0;
    float timeInactiveTemp = 0;
    float lastActionTime = 0;
    public bool isOnline = false;
    public bool isHost = false;
    public int maxPeople = 0;
    public int maxActive = 0;
    public int maxPassiveController = 0;
    public int maxPassiveVR = 0;
    public int maxTheatre = 0;
    string mostToolGrab = "NONE";
    string mostToolTime = "NONE";
    string mostCommonToolPairs = "NONE";
    public int numTriangle = 0;
    public int numSquare = 0;
    public int numCircle = 0;
    public int numIsec = 0;
    public int numCsec = 0;
    public int numLsec = 0;
    public int numTsec = 0;

    //what all of the letters mean are listed here.
    //other tools can eventually follow this model as it is flexible
    #region tools
    float timeSpline = 0;
    float numSpline = 0;
    float timeLine = 0;
    float numLine = 0;
    float timeCamera = 0;
    float numCamera = 0;
    float timeBoolean = 0;
    float numBoolean = 0;
    float timeClone = 0;
    float numClone = 0;
    float timeDrawFace = 0;
    float numDrawFace = 0;
    float timeExtrude = 0;
    float numExtrude = 0;
    float timeLoft = 0;
    float numLoft = 0;
    float timeGroup = 0;
    float numGroup = 0;
    float timePrimitive = 0;
    float numPrimitive = 0;
    float timeClipping = 0;
    float numClipping = 0;
    float timeMirror = 0;
    float numMirror = 0;
    float timeSelection = 0;
    float numSelection = 0;
    float timePolygon = 0;
    float numPolygon = 0;
    float timePaint = 0;
    float numPaint = 0;
    float timeEraser = 0;
    float numEraser = 0;
    float timeGravity = 0;
    float numGravity = 0;
    float timeExtrudeCurve = 0;
    float numExtrudeCurve = 0;
    float timeMeasuring = 0;
    float numMeasuring = 0;
    #endregion
    //initialize things like the time and default tools in case they pick up nothing
    float currentToolTime = 0;
    public float roomHeight = 0;
    public float roomWidth = 0;
    float userHeight = 0;
    string commonShape = "NONE";
    string commonSection = "NONE";
    int numSaves = 0;
    float avgSaveSize = 0;
    public List<float> saveSizes = new List<float>();
    string avgColor = "NONE";
    string shapeVsSection = "NONE";
    float timer = 9;
    List<float> heights = new List<float>();
    private static string actionList;

    public WandController leftController;
    public WandController rightController;

    TrackerItem TI;
    RawDataItem TI2;

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        quit = false;
        ToolTracker.Create();
        TI = new TrackerItem();
        TI2 = new RawDataItem();
        actionList = "";


        toolMap = new Dictionary<string, ToolData>();
        toolMap.Add("A", new ToolData("Camera Tool", 0, 0));
        toolMap.Add("B", new ToolData("Boolean Tool", 0, 0));
        toolMap.Add("C", new ToolData("Clone Tool", 0, 0));
        toolMap.Add("D", new ToolData("Draw Face Tool", 0, 0));
        toolMap.Add("E", new ToolData("Extrude Face Tool", 0, 0));
        toolMap.Add("F", new ToolData("Loft Tool", 0, 0));
        toolMap.Add("G", new ToolData("Group Tool", 0, 0));
        toolMap.Add("I", new ToolData("Primitive Tool", 0, 0));
        toolMap.Add("K", new ToolData("Clipping Tool", 0, 0));
        toolMap.Add("L", new ToolData("Line Tool", 0, 0));
        toolMap.Add("M", new ToolData("Mirror Tool", 0, 0));
        toolMap.Add("N", new ToolData("Selection Tool", 0, 0));
        toolMap.Add("O", new ToolData("Polygon Tool", 0, 0));
        toolMap.Add("P", new ToolData("Paint Tool", 0, 0));
        toolMap.Add("R", new ToolData("Eraser Tool", 0, 0));
        toolMap.Add("S", new ToolData("Spline Tool", 0, 0));
        toolMap.Add("U", new ToolData("No Tool", 0, 0));
        toolMap.Add("V", new ToolData("Gravity Tool", 0, 0));
        toolMap.Add("X", new ToolData("Extrude Curve Tool", 0, 0));
        toolMap.Add("Z", new ToolData("Measuring Tool", 0, 0));
    }

    public void Update()
    {
        if (Time.realtimeSinceStartup - lastActionTime > 5.0f)
        {
            timeInactiveTemp = Time.realtimeSinceStartup - lastActionTime - 5;
        }

        if(leftController && rightController)
        if (leftController.triggerButtonDown || leftController.gripButtonDown || leftController.dpadDownDown ||
            leftController.dpadUpDown || leftController.dpadLeftDown || leftController.dpadRightDown ||
            rightController.triggerButtonDown || rightController.gripButtonDown || rightController.dpadDownDown ||
            rightController.dpadUpDown || rightController.dpadLeftDown || rightController.dpadRightDown)
        {
            ResetInactive();
        }


        //every 10 seconds, I get the height of the user, and try to guess that
        timer += Time.deltaTime;
        currentToolTime += Time.deltaTime;
        if (timer > 10)
        {
            if (GameObject.Find("Camera (eye)"))
            {
                heights.Add(GameObject.Find("Camera (eye)").transform.position.y);
                timer = 0;
            }
        }
    }

    //analytics will upload once the program quits
    public void OnApplicationQuit()
    {
        if(!quit)
        {
            Application.CancelQuit();

            if (Time.realtimeSinceStartup > 1.0f)
            {
                ResetInactive();
                calculateAnalytics();
                uploadAnalytics();
            }

            quit = true;
            OnApplicationQuit();
        }
    }

    public static void AddAction()
    {
        actionList += ToolTracker.GetActions() + " \n";
    }

    public static void AddAction(String addition)
    {
        actionList += addition;
    }


    /// <summary>
    /// used to find the most common order of tools used in a pair
    /// </summary>
    /// <returns></returns>
    string MostCommonSubstring()
    {
        string result = "";
        Dictionary<char, Dictionary<char, int>> Vals = new Dictionary<char, Dictionary<char, int>>();
        

        for(int i = 0; i < toolListString.Length -1; i++)
        {
            if (!Vals.ContainsKey(toolListString[i]))
            {
                Vals.Add(toolListString[i], new Dictionary<char, int>());
            }
            if (!Vals[toolListString[i]].ContainsKey(toolListString[i + 1])) Vals[toolListString[i]].Add(toolListString[i + 1], 0);

            Vals[toolListString[i]][toolListString[i + 1]]++;
        }

        int maxCount = -1;
        foreach(var k in Vals)
        {
            foreach(var c in k.Value)
            {
                if(c.Value > maxCount)
                {
                    maxCount = c.Value;
                    result = k.Key.ToString() + c.Key;
                }
            }
        }

        return result;
    }

    void calculateAnalytics()
    {
        //gonna need a better algorithm for more tools

        if (Valve.VR.OpenVR.IsHmdPresent())
        {
            Valve.VR.HmdQuad_t roomDims = new Valve.VR.HmdQuad_t();
            SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref roomDims);
            Vector3 roomScale = new Vector3(Mathf.Abs(roomDims.vCorners0.v0 - roomDims.vCorners2.v0), Mathf.Abs(roomDims.vCorners0.v2 - roomDims.vCorners2.v2), 1);
            roomWidth = roomScale.x;
            roomHeight = roomScale.y;
        }

        int mostToolNum = 0;
        float mostToolTimed = 0;

        foreach(KeyValuePair<string, ToolData> pair in toolMap)
        {
            if(pair.Value.Count > mostToolNum)
            {
                mostToolGrab = pair.Value.Name;
                mostToolNum = pair.Value.Count;
            }

            if(pair.Value.Time > mostToolTimed)
            {
                mostToolTime = pair.Value.Name;
                mostToolTimed = pair.Value.Time;
            }
        }

        string mostCommonPairLetters = MostCommonSubstring();

        if (mostCommonPairLetters.Length == 2)
        {
            string firstTool = toolMap[mostCommonPairLetters[0].ToString()].Name;
            string secondTool = toolMap[mostCommonPairLetters[1].ToString()].Name;
            mostCommonToolPairs = firstTool + " to " + secondTool;
        }
        
        timeCamera = toolMap["A"].Time;
        numCamera = toolMap["A"].Count;
        timeBoolean = toolMap["B"].Time;
        numBoolean = toolMap["B"].Count;
        timeClone = toolMap["C"].Time;
        numClone = toolMap["C"].Count;
        timeDrawFace = toolMap["D"].Time;
        numDrawFace = toolMap["D"].Count;
        timeExtrude = toolMap["E"].Time;
        numExtrude = toolMap["E"].Count;
        timeLoft = toolMap["F"].Time;
        numLoft = toolMap["F"].Count;
        timeGroup = toolMap["G"].Time;
        numGroup = toolMap["G"].Count;
        timePrimitive = toolMap["I"].Time;
        numPrimitive = toolMap["I"].Count;
        timeClipping = toolMap["K"].Time;
        numClipping = toolMap["K"].Count;
        timeLine = toolMap["L"].Time;
        numLine = toolMap["L"].Count;
        timeMirror = toolMap["M"].Time;
        numMirror = toolMap["M"].Count;
        timeSelection = toolMap["N"].Time;
        numSelection = toolMap["N"].Count;
        timePolygon = toolMap["O"].Time;
        numPolygon = toolMap["O"].Count;
        timePaint = toolMap["P"].Time;
        numPaint = toolMap["P"].Count;
        timeEraser = toolMap["R"].Time;
        numEraser = toolMap["R"].Count;
        timeSpline = toolMap["S"].Time;
        numSpline = toolMap["S"].Count;
        timeGravity = toolMap["V"].Time;
        numGravity = toolMap["V"].Count;
        timeExtrudeCurve = toolMap["X"].Time;
        numExtrudeCurve = toolMap["X"].Count;
        timeMeasuring = toolMap["Z"].Time;
        numMeasuring = toolMap["Z"].Count;

        //here i get the most used shapes and sections
        // this can be done more efficiently
        List<int> shapeList = new List<int>();
        List<int> sectionList = new List<int>();
        shapeList.Add(numTriangle);
        shapeList.Add(numSquare);
        shapeList.Add(numCircle);
        sectionList.Add(numIsec);
        sectionList.Add(numCsec);
        sectionList.Add(numLsec);
        sectionList.Add(numTsec);

        int shapeNum = shapeList[0];
        int shapeInd = 0;
        int secNum = sectionList[0];
        int secInd = 0;

        for (int k = 0; k < shapeList.Count; k++)
        {
            if (shapeList[k] > shapeNum)
            {
                shapeNum = shapeList[k];
                shapeInd = k;
            }
        }

        for (int l = 0; l < sectionList.Count; l++)
        {
            if (sectionList[l] > shapeNum)
            {
                secNum = sectionList[l];
                secInd = l;
            }
        }

        if (shapeNum > 0)
        {
            switch (shapeInd)
            {
                case 0:
                    commonShape = "Triangle";
                    break;
                case 1:
                    commonShape = "Square";
                    break;
                case 2:
                    commonShape = "Circle";
                    break;
            }
        }
        else
            commonShape = "NONE";

        if (secNum > 0)
        {
            switch (secInd)
            {
                case 0:
                    commonSection = "I Section";
                    break;
                case 1:
                    commonSection = "C Section";
                    break;
                case 2:
                    commonSection = "L Section";
                    break;
                case 3:
                    commonSection = "T Section";
                    break;
            }
        }
        else
            commonSection = "NONE";

        if (shapeNum > secNum)
            shapeVsSection = "Shape";
        else if (secNum > shapeNum)
            shapeVsSection = "Section";
        else if (secNum == shapeNum)
            shapeVsSection = "Equal";

        float saveSum = 0;
        numSaves = saveSizes.Count;
        if (numSaves > 0)
        {
            for (int i = 0; i < numSaves; i++)
            {
                saveSum += saveSizes[i];
            }
            avgSaveSize = saveSum / numSaves;
        }
        else
            avgSaveSize = 0;


        //for now, just get color average, not common colors - pretty innacurate
        float sumR = 0;
        float sumG = 0;
        float sumB = 0;
        for(int ci = 0; ci<colors.Count; ci++)
        {
            float r = colors[ci].r;
            float g = colors[ci].g;
            float b = colors[ci].b;

            sumR += r;
            sumG += g;
            sumB += b;
        }

        float avgR = sumR / colors.Count;
        float avgG = sumG / colors.Count;
        float avgB = sumB / colors.Count;

        avgR *= 255.0f;
        avgG *= 255.0f;
        avgB *= 255.0f;

        avgColor = "(" + avgR + "," + avgG + "," + avgB + ")";

        if (heights.Count > 0)
            userHeight = heights[heights.Count * 3 / 4];
        else
            userHeight = 0;

        userHeight *= 1.5f;

        maxPeople = maxActive + maxPassiveVR + maxPassiveController + maxTheatre;

        TrackerScript.AddAction("*");
    }

    void uploadAnalytics()
    {
        //fill struct
        TI.userHeight = userHeight;
        TI.roomWidth = roomWidth;
        TI.roomHeight = roomHeight;
        TI.isOnline = isOnline;
        TI.isHost = isHost;
        TI.maxPeople = maxPeople;
        TI.maxActive = maxActive;
        TI.maxPassiveController = maxPassiveController;
        TI.maxPassiveVR = maxPassiveVR;
        TI.maxTheatre = maxTheatre;
        TI.timeSpent = Time.realtimeSinceStartup;
        TI.timeInactive = timeInactive;
        TI.timeSpline = timeSpline;
        TI.numSpline = numSpline;
        TI.timeLine = timeLine;
        TI.numLine = numLine;
        TI.timeMirror = timeMirror;
        TI.numMirror = numMirror;
        TI.timeEraser = timeEraser;
        TI.numEraser = numEraser;
        TI.timeCamera = timeCamera;
        TI.numCamera = numCamera;
        TI.timeBoolean = timeBoolean;
        TI.numBoolean = numBoolean;
        TI.timeClone = timeClone;
        TI.numClone = numClone;
        TI.timeDrawFace = timeDrawFace;
        TI.numDrawFace = numDrawFace;
        TI.timeExtrude = timeExtrude;
        TI.numExtrude = numExtrude;
        TI.timeLoft = timeLoft;
        TI.numLoft = numLoft;
        TI.timeGroup = timeGroup;
        TI.numGroup = numGroup;
        TI.timePrimitive = timePrimitive;
        TI.numPrimitive = numPrimitive;
        TI.timeClipping = timeClipping;
        TI.numClipping = numClipping;
        TI.timeSelection = timeSelection;
        TI.numSelection = numSelection;
        TI.timePolygon = timePolygon;
        TI.numPolygon = numPolygon;
        TI.timePaint = timePaint;
        TI.numPaint = numPaint;
        TI.timeGravity = timeGravity;
        TI.numGravity = numGravity;
        TI.timeExtrudeCurve = timeExtrudeCurve;
        TI.numExtrudeCurve = numExtrudeCurve;
        TI.timeMeasuring = timeMeasuring;
        TI.numMeasuring = numMeasuring;
        TI.mostChosenTool = mostToolGrab;
        TI.longestUsedTool = mostToolTime;
        TI.mostChosenToolPair = mostCommonToolPairs;
        TI.numMesh = numMesh;
        TI.numErase = numErase;
        TI.avgSaveSize = avgSaveSize;
        TI.avgColor = avgColor;
        TI.commonSection = commonSection;
        TI.commonShape = commonShape;
        TI.shapeVsSection = shapeVsSection;
        TI.Timestamp = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString();

        TI2.rawString = actionList;
        TI2.Timestamp = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString();

        BodyAnalytics newbody = new BodyAnalytics();
        newbody.Item = new TrackerItem();
        newbody.Item = TI;
        Payload<BodyAnalytics> payload = new Payload<BodyAnalytics>();
        payload.body = newbody;

        BodyRawData newbody2 = new BodyRawData();
        newbody2.Item = new RawDataItem();
        newbody2.Item = TI2;
        Payload<BodyRawData> payload2 = new Payload<BodyRawData>();
        payload2.body = newbody2;
    
        UploadAnalyitics.Upload(payload, payload2);

        //We stopped updating Unity Analytics, but it will still upload what is here
        #region Unity Analytics
        Analytics.CustomEvent("General Stats", new Dictionary<string, object>
            {
                { "total time spent (s)", Time.realtimeSinceStartup },
                { "time spent inactive (s)", timeInactive },
                { "height of user (m)", userHeight },
                { "width of room (m x m)" , roomWidth },
                { "height of room (m x m)" , roomHeight },
            });

        Analytics.CustomEvent("User Stats", new Dictionary<string, object>
            {
                { "max number of people entered room", maxPeople },
                { "is connected online", isOnline },
                { "is the host", isHost },
                { "max number of active players" , maxActive},
                { "max number of passive controller players" , maxPassiveController},
                { "max number of passive VR players" , maxPassiveVR},
                { "max number of theatre viewers", maxTheatre}
            });

        Analytics.CustomEvent("Tool Stats", new Dictionary<string, object>
            {
                { "most chosen tool", mostToolGrab },
                { "most chosen tool pairs", mostCommonToolPairs },
                { "time using spline tool", timeSpline },
                { "time using line tool", timeLine },
                { "time using mirror tool", timeMirror },
                { "time using erase tool", timeEraser },
            });

        Analytics.CustomEvent("Mesh Stats", new Dictionary<string, object>
            {
                { "number of total meshes" , numMesh },
                { "number of total erases" , numErase },
                { "number of saves" , numSaves },
                { "average size of save(byte)" , avgSaveSize },
                { "most selected shape", commonShape },
                { "most selected section", commonSection },
                { "shape or section preferred", shapeVsSection },
            });

#endregion


        //print all the informtion to file
        string path = "log.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(System.DateTime.Now.ToShortDateString());
        writer.WriteLine(System.DateTime.Now.ToShortTimeString());
        writer.WriteLine("total time spent(s): " + Time.realtimeSinceStartup);
        writer.WriteLine("time spent inactive (s): " + timeInactive);
        writer.WriteLine("height of user (m): " + userHeight);
        writer.WriteLine("width of room (m x m): " + roomWidth);
        writer.WriteLine("height of room (m x m): " + roomHeight);
        writer.WriteLine("max number of people entered room: " + maxPeople);
        writer.WriteLine("is connected online: " + isOnline);
        writer.WriteLine("is the host: " + isHost);
        writer.WriteLine("number of active players: " + maxActive);
        writer.WriteLine("number of passive controller players: " + maxPassiveController);
        writer.WriteLine("number of passive VR players: " + maxPassiveVR);
        writer.WriteLine("number of theatre viewers: " + maxTheatre);
        writer.WriteLine("most chosen tool: " + mostToolGrab);
        writer.WriteLine("longest used tool: " + mostToolTime);
        writer.WriteLine("most chosen tool pairs: " + mostCommonToolPairs);

        foreach(KeyValuePair<string, ToolData> data in toolMap)
        {
            writer.WriteLine("number of times grabbing " + data.Value.Name + ": " + data.Value.Count);
            writer.WriteLine("time spent holding " + data.Value.Name + ": " + data.Value.Time);
        }

        writer.WriteLine("number of total meshes: " + numMesh);
        writer.WriteLine("number of total erases: " + numErase);
        writer.WriteLine("most selected shape: " + commonShape);
        writer.WriteLine("most selected section: " + commonSection);
        writer.WriteLine("shape or section preferred: " + shapeVsSection);
        writer.WriteLine("number of saves: " + numSaves);
        writer.WriteLine("average size save(byte): " + avgSaveSize);
        writer.WriteLine("average color: " + avgColor);
        writer.WriteLine("\nAction List:\n" + actionList); //numbers based on actions
        writer.WriteLine("\n");
        writer.Close();
    }

    void ResetInactive()
    {
        lastActionTime = Time.realtimeSinceStartup;
        timeInactive = timeInactiveTemp;
        timeInactiveTemp = 0;
    }

    public void UpdateTool(string letter)
    {
        ToolData tool = toolMap[letter];
        tool.Time = currentToolTime;
        tool.Count += 1;
        toolMap[letter] = tool;
        currentToolTime = 0;
        toolListString += letter;
    }
}