using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
public class MeshConverter : MonoBehaviour
{

    MeshFilter[] meshList;

    public static MeshConverter Instance;
    private string directory;
    private string userName;
    private DateTime date;
    private int count = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        //userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        userName = Environment.UserName;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Saves the scene out for later loading with the model manager class. for information about the structure of thesave
    /// look as the SceneSave class
    /// </summary>
    public void SaveScene()
    {
        List<Transform> parents = new List<Transform>();
        SceneSave save = new SceneSave();
        count = Directory.GetFiles(Application.persistentDataPath + "/Saves/").Length;

        ObjectID[] objectList = GetComponentsInChildren<ObjectID>();
        save.OBJStrings = new string[objectList.Length];
        save.Positions = new V3[objectList.Length];
        save.Rotations = new V3[objectList.Length];
        save.Scales = new V3[objectList.Length];
        save.Colors = new float[objectList.Length, 3];
        save.Groups = new int[objectList.Length];
        for (int i = 0; i < objectList.Length; i++)
        {
            save.OBJStrings[i] = ObjExporter.MeshToString(objectList[i].gameObject.GetComponent<MeshFilter>());
            save.Positions[i] = new V3(objectList[i].transform.position);
            save.Rotations[i] = new V3(objectList[i].transform.rotation.eulerAngles);
            save.Scales[i] = new V3(objectList[i].transform.lossyScale);
            Color c = objectList[i].GetComponent<MeshRenderer>().material.color;
            save.Colors[i, 0] = c.r;
            save.Colors[i, 1] = c.g;
            save.Colors[i, 2] = c.b;
            if (objectList[i].gameObject.GetComponent<ObjectID>().HasParent)
            {
                if (parents.Contains(objectList[i].transform.parent))
                {
                    save.Groups[i] = parents.IndexOf(objectList[i].transform.parent);
                } else
                {
                    save.Groups[i] = parents.Count;
                    parents.Add(objectList[i].transform.parent);
                }

            } else
            {
                save.Groups[i] = -1;
            }
        }
        directory = string.Format(Application.persistentDataPath + "/Saves/"); ;
        Directory.CreateDirectory(directory);
        date = DateTime.Today;
        string fileName = string.Format(@"{0}/Scene {1}.obj", directory, count);
        FileStream fs = new FileStream(fileName, FileMode.Create);
        new BinaryFormatter().Serialize(fs, save);
        fs.Close();

        GameObject.Find("Tracker").GetComponent<TrackerScript>().saveSizes.Add(new System.IO.FileInfo(fileName).Length);
        ModelManager.instance.LoadScenes();
    }

}


