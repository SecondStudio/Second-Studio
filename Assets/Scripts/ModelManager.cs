using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
public class ModelManager : MonoBehaviour {

    List<SceneModel> Scenes;
    public Transform DisplaySpace;
    public static ModelManager instance;
    int SceneIndex = 0;
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
            LoadScenes();
        }
        
    }
    public void LoadScenes()
    {
        ClearDisplay();
        Scenes = new List<SceneModel>();
        BinaryFormatter bf = new BinaryFormatter();
        foreach (string file in Directory.GetFiles(Application.persistentDataPath + "/Saves/"))
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            SceneModel model = new SceneModel();
            model.MyScene = bf.Deserialize(fs) as SceneSave;
            Scenes.Add(model);
            fs.Close();
        }
        SceneIndex = 0;
        if (Scenes.Count > 0) DisplayScene(SceneIndex);
    }

    private void DisplayScene( int i )
    {
        ClearDisplay();
        Scenes[i].GenerateModel(DisplaySpace);
    }

    private void ClearDisplay()
    {
        foreach (Transform c in DisplaySpace.GetComponentsInChildren<Transform>())
        {
            if (c == DisplaySpace) continue;
            Destroy(c.gameObject);
        }
    }

    public void OpenScene(int index)
    {
        Scenes[index].OpenScene();
    }

    public void NextScene()
    {
        SceneIndex = SceneIndex + 1 >= Scenes.Count ? 0 : SceneIndex + 1;
        DisplayScene(SceneIndex);
    }

    public void PrevScene()
    {
        SceneIndex = SceneIndex - 1 < 0 ? Scenes.Count - 1 : SceneIndex - 1;
        DisplayScene(SceneIndex);
    }

    public void OpenCurrentScene()
    {
        Scenes[SceneIndex].OpenScene();
    }

    public void OpenObj(string path)
    {
        var obj = Instantiate(Resources.Load("GenericMesh")) as GameObject;
        string objString = File.ReadAllText(path);
        var mesh = ObjImporter.ImportMeshFromString(objString);
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("ModelMaterial");
        obj.transform.position = new Vector3(0, mesh.bounds.extents.y, 0);
        float max = -1;
        for(int i = 0; i < 3; i++)
        {
            if (mesh.bounds.extents[i] > max) max = mesh.bounds.extents[i];
        }
        obj.transform.localScale *= (max / 20);
        var editor = obj.AddComponent<MeshEditor>();
        editor.StartGroupGeneration();
        editor.UpdateFaces();
        ObjectManager.instance.AddObject(obj);
    }

    public void SelectObj()
    {
        var path = EditorUtility.OpenFilePanel("Open OBJ", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "obj");
        OpenObj(path);
    }
}
