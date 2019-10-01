using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneModel {

    public SceneSave MyScene;

    public void GenerateModel (Transform parent)
    {
        Transform ModelParent = new GameObject().GetComponent<Transform>();
        ModelParent.parent = parent;
        ModelParent.transform.position = parent.transform.position;
        List<MeshRenderer> Renderers = new List<MeshRenderer>();
        for(int i = 0; i < MyScene.OBJStrings.Length; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.parent = ModelParent;
            obj.transform.position = ModelParent.position;
            obj.AddComponent<MeshFilter>().mesh = ObjImporter.ImportMeshFromString(MyScene.OBJStrings[i]);
            obj.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("ModelMaterial");
            Color c = new Color(MyScene.Colors[i, 0], MyScene.Colors[i, 1], MyScene.Colors[i, 2]);
            obj.GetComponent<MeshRenderer>().material.color = c;
            Renderers.Add(obj.GetComponent<MeshRenderer>());
            obj.transform.localPosition = (MyScene.Positions[i].GetVector3());
            obj.transform.localEulerAngles = MyScene.Rotations[i].GetVector3();
            obj.transform.parent = null;
            obj.transform.localScale = MyScene.Scales[i].GetVector3();
            obj.transform.parent = ModelParent;
        }

        Bounds ModelAreaBounds = parent.GetComponent<Collider>().bounds;
        Bounds ModelBounds = new Bounds(ModelParent.transform.position, Vector3.zero);
        foreach(var r in Renderers)
        {
            ModelBounds.Encapsulate(r.bounds);
        }
        float MaxRatio = -1f;
        if (ModelBounds.extents.x / ModelAreaBounds.extents.x > MaxRatio) MaxRatio = ModelBounds.extents.x / ModelAreaBounds.extents.x;
        if (ModelBounds.extents.y / ModelAreaBounds.extents.y > MaxRatio) MaxRatio = ModelBounds.extents.y / ModelAreaBounds.extents.y;
        if (ModelBounds.extents.z / ModelAreaBounds.extents.z > MaxRatio) MaxRatio = ModelBounds.extents.z / ModelAreaBounds.extents.z;
        ModelParent.localScale /= MaxRatio;
    }

    public void OpenScene()
    {
        //TrackerScript.AddAction("I");
        Transform ObjectContainer = GameObject.Find("TestScene").transform;

        int numGroups = 0;
        for(int i = 0; i < MyScene.OBJStrings.Length; i++)
        {
            if (MyScene.Groups[i] + 1 > numGroups) numGroups = MyScene.Groups[i] + 1;
        }

        GameObject[] Parents = new GameObject[numGroups];
        
        for(int i = 0; i < numGroups; i++)
        {
            Parents[i] = new GameObject();
            Parents[i].AddComponent<Rigidbody>();
            Parents[i].transform.SetParent(ObjectContainer);
            ObjectManager.instance.AddObject(Parents[i]);
        }

        for (int i = 0; i < MyScene.OBJStrings.Length; i++)
        {
            GameObject obj = GameObject.Instantiate(Resources.Load("GenericMesh")) as GameObject;
            ObjectManager.instance.AddObject(obj);
            if (MyScene.Groups[i] != -1)
            {
                obj.transform.parent = Parents[MyScene.Groups[i]].transform;
                obj.GetComponent<ObjectID>().HasParent = true;
                var joint = obj.AddComponent<FixedJoint>();
                joint.connectedBody = Parents[MyScene.Groups[i]].GetComponent<Rigidbody>();
            }
            else
            {
                obj.transform.parent = ObjectContainer;
            }
            
            obj.GetComponent<MeshFilter>().mesh = ObjImporter.ImportMeshFromString(MyScene.OBJStrings[i]);
            obj.GetComponent<MeshCollider>().sharedMesh = obj.GetComponent<MeshFilter>().mesh;
            obj.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("ModelMaterial");
            Color c = new Color(MyScene.Colors[i, 0], MyScene.Colors[i, 1], MyScene.Colors[i, 2]);
            obj.GetComponent<MeshRenderer>().material.color = c;
            obj.transform.position = MyScene.Positions[i].GetVector3();
            obj.transform.eulerAngles = MyScene.Rotations[i].GetVector3();
            obj.transform.localScale = MyScene.Scales[i].GetVector3();
            
        }
    }

}
