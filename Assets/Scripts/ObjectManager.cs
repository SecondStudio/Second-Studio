using UnityEngine;
using System.Collections;

public class ObjectManager : MonoBehaviour {
    ArrayList objectList;
    public static ObjectManager instance;
    public int actionLimit = 25;
    static int count = 0;
    public Material OutlineMaterial;
    public bool gravity = false;

    public int NextId { get { return count++; } }
	// Use this for initialization
	void Start () {
        objectList = new ArrayList();
        instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void RestartScene()
    {
        objectList.Clear();
    }

    public void AddObject(GameObject go) //Adds an object to the management system. Automatically adds an ObjectID script, as well as a rigidbody and collider 
    {
        Debug.Log("Adding Object!");
        if (go == null)
        {
            Debug.Log("ERROR: OBJECT is NULL!");
            return;
        }
        if (objectList == null) objectList = new ArrayList();
        objectList.Add(go);
        go.transform.parent = transform;
        if (go.GetComponent<ProcSection>()) Destroy(go.GetComponent<ProcSection>());
        if (go.GetComponent<ProcShape>()) Destroy(go.GetComponent<ProcShape>());
        if (objectList.Count > actionLimit) // limits how many objects to track
            objectList.RemoveAt(objectList.Count - 1);

        ObjectID oid = go.GetComponent<ObjectID>();
        if (oid == null) oid = go.AddComponent<ObjectID>();
        oid.SetId(count);
        ++count;

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.useGravity = gravity;
        rb.isKinematic = !gravity;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        if (go.GetComponent<MeshCollider>())
        {
            go.GetComponent<MeshCollider>().convex = gravity;
        }
        else {
            go.AddComponent<MeshCollider>();
            }
        var editor = go.GetComponent<MeshEditor>();
        //        if (editor != null) editor.GenerateHandles();

        
        
        if(go.GetComponent<MeshFilter>() != null && oid.OutlineRenderer == null)
        {
            var outline = new GameObject();
            outline.transform.parent = go.transform;
            outline.transform.position = go.transform.position;
            outline.transform.rotation = go.transform.rotation;
            outline.transform.localScale = new Vector3(1f, 1f, 1f);
            var filt = outline.AddComponent<MeshFilter>();
            var rend = outline.AddComponent<MeshRenderer>();
            rend.material = OutlineMaterial;
            rend.material.color = Color.yellow;
            filt.mesh = go.GetComponent<MeshFilter>().mesh;
            oid.OutlineRenderer = rend;
            rend.enabled = false;
            rend.transform.position = rend.transform.parent.position;
            
        }
        
        // prevents overflow (lol if someone instantiates more than 4 billion objects they deserve a crash)
        if (count > int.MaxValue)
            count = 0;
    }

    public void DeleteObject(GameObject go)
    {
        Debug.Log("Deleting Object!");

        if (go == null)
        {
            Debug.Log("ERROR: OBJECT is NULL!");
            return;
        }

        go.SetActive(false);
        objectList.Add(go);

        if (objectList.Count > actionLimit) // limits how many objects to track
            objectList.RemoveAt(objectList.Count - 1);
    }

    public void DeleteObject(int id)
    {
        GameObject go = FindObject(id);
        Debug.Log("Deleting Object!");

        if (go == null)
        {
            Debug.Log("ERROR: OBJECT is NULL!");
            return;
        }

        go.SetActive(false);
        objectList.Add(go);

        if (objectList.Count > actionLimit) // limits how many objects to track
            objectList.RemoveAt(objectList.Count - 1);
    }

    public void DestroyObject(GameObject go)
    {
        Debug.Log("Destroy Object!");

        if (go == null)
        {
            Debug.Log("ERROR: OBJECT is NULL!");
            return;
        }

        Destroy(go);
    }

    public void DestroyObject(int id)
    {
        Debug.Log("Deleting Object!");
        GameObject go = FindObject(id);

        if (go == null)
        {
            Debug.Log("ERROR: OBJECT is NULL!");
            return;
        }

        go.SetActive(false);
    }


    public void Undo()
    {
        if (objectList.Count == 0)
            return;

        GameObject lastObject = objectList[objectList.Count - 1] as GameObject;
        objectList.RemoveAt(objectList.Count - 1);

        if (lastObject == null)
            return;

        if (lastObject.activeSelf == true)
        {
            Destroy(lastObject);
        }
        if (lastObject.activeSelf == false)
        {
            lastObject.SetActive(true);
        }
    }

    public GameObject FindObject(int id)
    {
        ObjectID[] idList = GetComponentsInChildren<ObjectID>();
        foreach (ObjectID i in idList)
        {
            if (i.id == id)
            {
                return i.gameObject;
            }
        }

        Debug.Log("Id for " + id + " cannot be found!");
        return null;
    }

    public void FlipGravity()
    {
        gravity = !gravity;
        foreach(Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            try
            {
                rb.useGravity = gravity;
                
                if (rb.GetComponent<MeshCollider>())
                {
                    rb.GetComponent<MeshCollider>().convex = gravity;
                }
                if (!gravity) rb.velocity = Vector3.zero;
                rb.isKinematic = !gravity;
            } catch {
                print("something went wrong in gravity flipping");
            }   
        }
    }

}
