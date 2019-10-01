using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// unclear on how things work exactly
/// hair tool is handled a little differently than shape and section for the sake of having NURB data
/// </summary>
public class LineTool : ToolBase {

    private GameObject scene;
    GameObject go;

    public enum ObjectType { Shape, Section, Hair }
    public ObjectType objectType = ObjectType.Shape;

    public GameObject shapeObject;
    public GameObject sectionObject;
    public GameObject hairObject;

    public Text lengthText;

    GameObject objectCursor;
    GameObject currentObject;
    private Vector3 offset;
    public bool ConstrainX { get { return ConstraintManager.ConstrainX; } }
    public bool ConstrainY { get { return ConstraintManager.ConstrainY; } }
    public bool ConstrainZ { get { return ConstraintManager.ConstrainZ; } }
    public bool ConstrainLength { get { return ConstraintManager.ConstrainLength; } }
    public float LengthConstraint { get { return ConstraintManager.LengthConstraint; } }
    ProcShape shape;
    ProcSection section;
    ProcHair hair;
    Vector3 startPosition;
    float StartRoll;

    public MirrorScript ms;
    private ProcSection.ShapeType[] ShapeTypes = { ProcSection.ShapeType.T, ProcSection.ShapeType.I, ProcSection.ShapeType.BRACKET, ProcSection.ShapeType.LEFTCORNER, ProcSection.ShapeType.RIGHTCORNER };
    ushort pulseLength = 7000;
    public GameObject colorWheel;
    private float timeUpdate = 0.02f;
    float timeLeft = 0.0f;
    float timeSizeLeft = 0.0f;
    string tooltype;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        trackerLetter = "L";

        shapeObject.GetComponent<ProcShape>().Init();
        shapeObject.transform.Rotate(new Vector3(45, 0, 0));
        sectionObject.GetComponent<ProcSection>().Init();
        hairObject.GetComponent<ProcHair>().Init();

        timeLeft = timeUpdate;
        timeSizeLeft = timeUpdate;

        scene = GameObject.Find("TestScene");

        tooltype = "Square";
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        colorWheel.GetComponent<ColorPicker>().controller = controller;
        if (isHeld)
            colorWheel.GetComponent<ColorPicker>().isHeld = true;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        colorWheel.GetComponent<ColorPicker>().isHeld = false;
    }


    // Update is called once per frame
    //know which shape to use based on touch while holding this tool
    protected override void Update()
    {
        base.Update();
        if (!photonView.isMine)
            return;

        if (timeLeft > 0)
            timeLeft -= Time.deltaTime;

        if (timeSizeLeft > 0)
            timeSizeLeft -= Time.deltaTime;

        if (tooltype == "Triangle")
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Shape, 3);
            ToolTracker.net[0] = 2;
        }

        if (tooltype == "Square")
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Shape, 4);
            ToolTracker.net[0] = 3;
        }

        if (tooltype == "Circle")
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Shape, 12);
            ToolTracker.net[0] = 1;
        }

        if (tooltype == "Hair") //copied tracker values for circle - doesn't have its own yet
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Hair, 12);
        }

        if (tooltype == "I Section")
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Section, (int)ProcSection.ShapeType.I);
            ToolTracker.net[0] = 7;
        }

        if (tooltype == "C Section")
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Section, (int)ProcSection.ShapeType.BRACKET);
            ToolTracker.net[0] = 6;
        }

        if (tooltype == "L Section")
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Section, (int)ProcSection.ShapeType.LEFTCORNER);
            ToolTracker.net[0] = 5;
        }

        if (tooltype == "T Section")
        {
            photonView.RPC("ChangeShape", PhotonTargets.AllBufferedViaServer, ObjectType.Section, (int)ProcSection.ShapeType.T);
            ToolTracker.net[0] = 4;
        }


        if (controller.triggerButtonDown)
        {
            Debug.Log("START!");
            photonView.RPC("StartDrawing", PhotonTargets.AllBufferedViaServer, objectCursor.transform.position, objectCursor.transform.rotation);
        }

        else if (controller.triggerButtonPressed && timeLeft <= 0)
        {
            photonView.RPC("UpdateDrawing", PhotonTargets.AllBufferedViaServer, shapeObject.transform.position, transform.up, transform.forward, transform.right);
            timeLeft = timeUpdate;
        }
        else if (controller.triggerButtonUp)
        {
            photonView.RPC("EndDrawing", PhotonTargets.AllBufferedViaServer);
            lengthText.text = "";
        }

        //hardcoded to work with scalebrush
        if (controller.dpadAxis.x > 0.2 && timeSizeLeft <= 0 && ActiveToolset.gameObject.name == "ScaleBrush")
        {
            photonView.RPC("ScaleUp", PhotonTargets.AllBufferedViaServer);
            timeSizeLeft = timeUpdate;
        }

        else if (controller.dpadAxis.x < -0.2 && timeSizeLeft <= 0 && ActiveToolset.gameObject.name == "ScaleBrush")
        {
            photonView.RPC("ScaleDown", PhotonTargets.AllBufferedViaServer);
            timeSizeLeft = timeUpdate;
        }

        colorWheel.GetComponent<ColorPicker>().controller = controller;
        if (isHeld)
            colorWheel.GetComponent<ColorPicker>().isHeld = true;
        else
            colorWheel.GetComponent<ColorPicker>().isHeld = false;

    }

    public void SetToObjectType(ObjectType ot)
    {
        objectType = ot;
        SetObject();
    }

    void SetObject()
    {
        // checks for the object
        if (objectType == ObjectType.Shape)
        {
            shapeObject.SetActive(true);
            sectionObject.SetActive(false);
            hairObject.SetActive(false);
            objectCursor = shapeObject;
        }
        else if (objectType == ObjectType.Section)
        {
            shapeObject.SetActive(false);
            sectionObject.SetActive(true);
            hairObject.SetActive(false);
            objectCursor = sectionObject;
        }
        else if (objectType == ObjectType.Hair)
        {
            shapeObject.SetActive(false);
            sectionObject.SetActive(false);
            hairObject.SetActive(true);
            objectCursor = hairObject;
        }
    }

    public void CreateReflection() {
        Debug.Log("REFLECT");
    }

    // sets the beginning position for line tool
    [PunRPC]
    public void StartDrawing(Vector3 position, Quaternion rotation)
    {
        startPosition = position;
        GameObject go = Instantiate(objectCursor, Vector3.zero,  Quaternion.identity) as GameObject;

        objectCursor.SetActive(false);

        if (objectType == ObjectType.Shape)
        {
            shape = go.GetComponent<ProcShape>();
            shape.ResetPoint(GridSnapTool.Snap(position) , transform.rotation.eulerAngles.x);

            // checks if there is a mirror -> make a copy
            if (ms.gameObject.activeSelf)
            {
                ms.CreateReflection(go, scene.transform);
                ms.reflection.transform.position = go.transform.position;
                ms.reflection.GetComponent<ProcShape>().startPoint = ms.ReflectPoint(GridSnapTool.Snap(position));
            }
        }
        if (objectType == ObjectType.Section)
        {
            section = go.GetComponent<ProcSection>();
            section.ResetPoint(GridSnapTool.Snap(position) , transform.rotation.eulerAngles.x);

            // checks if there is a mirror -> make a copy
            if (ms.gameObject.activeSelf)
            {
                ms.CreateReflection(go, scene.transform);
                ms.reflection.transform.position = go.transform.position;
                ms.reflection.GetComponent<ProcSection>().startPoint = ms.ReflectPoint(GridSnapTool.Snap(position));
            }
        }
        if (objectType == ObjectType.Hair)
        {
            go = Instantiate(Resources.Load("HairSplineTrail"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            go.GetComponent<HairSplineTrail>().Init(hairObject);
            hair = go.GetComponent<ProcHair>();
            hair.ResetPoint(GridSnapTool.Snap(position), transform.rotation.eulerAngles.x);

            // checks if there is a mirror -> make a copy
            if (ms.gameObject.activeSelf)
            {
                ms.CreateReflection(go, scene.transform);
                ms.reflection.transform.position = go.transform.position;
                ms.reflection.GetComponent<ProcHair>().startPoint = ms.ReflectPoint(GridSnapTool.Snap(position));
            }
        }
    }

    [PunRPC]
    public void UpdateDrawing(Vector3 position, Vector3 up, Vector3 forward, Vector3 right)
    {
        // updates shape
        if (objectType == ObjectType.Shape)
        {
            Vector3 actualPosition = position;
            if (ConstrainX) actualPosition.x = shape.startPoint.x;
            if (ConstrainY) actualPosition.y = shape.startPoint.y;
            if (ConstrainZ) actualPosition.z = shape.startPoint.z;

            if (!ConstrainLength)
            {
                shape.SetEndPoint(GridSnapTool.Snap(actualPosition));
            } else
            {
                Vector3 dir = (actualPosition - shape.startPoint).normalized;
                shape.SetEndPoint(GridSnapTool.Snap(shape.startPoint + LengthConstraint * dir));
            }

            lengthText.text = "" + Vector3.Distance(shape.endPoint, shape.startPoint);
            
            // checks if there is a mirror -> make a copy
            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<ProcShape>().SetEndPoint(ms.ReflectPoint(GridSnapTool.Snap(actualPosition)));
            }
        }
        else if (objectType == ObjectType.Section)
        {
            Vector3 actualPosition = position;
            if (ConstrainX) actualPosition.x = section.startPoint.x;
            if (ConstrainY) actualPosition.y = section.startPoint.y;
            if (ConstrainZ) actualPosition.z = section.startPoint.z;

            if (!ConstrainLength)
            {
                section.SetEndPoint(GridSnapTool.Snap(actualPosition));
            }
            else
            {
                Vector3 dir = (actualPosition - section.startPoint).normalized;
                section.SetEndPoint(GridSnapTool.Snap(section.startPoint + LengthConstraint * dir));
            }

            lengthText.text = "" + Vector3.Distance(section.endPoint, section.startPoint);

            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<ProcSection>().SetEndPoint(ms.ReflectPoint(GridSnapTool.Snap(actualPosition)));
            }
        }
        else if (objectType == ObjectType.Hair)
        {
            Vector3 actualPosition = position;
            if (ConstrainX) actualPosition.x = hair.startPoint.x;
            if (ConstrainY) actualPosition.y = hair.startPoint.y;
            if (ConstrainZ) actualPosition.z = hair.startPoint.z;

            //shape.SetEndPoint(actualPosition);
            hair.ResetPoint(GridSnapTool.Snap(startPosition), transform.rotation.eulerAngles.x);
            hair.GetComponent<HairSplineTrail>().BuildMeshTrail(GridSnapTool.Snap(actualPosition), up, forward, right , true);



            if (!ConstrainLength)
            {
                hair.SetEndPoint(GridSnapTool.Snap(actualPosition));
            }
            else
            {
                Vector3 dir = (actualPosition - hair.startPoint).normalized;
                hair.SetEndPoint(GridSnapTool.Snap(hair.startPoint + LengthConstraint * dir));
            }

            lengthText.text = "" + Vector3.Distance(hair.endPoint, hair.startPoint);

            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<ProcHair>().SetEndPoint(ms.ReflectPoint(GridSnapTool.Snap(actualPosition)));
            }
        }
    }

    [PunRPC]
    public void EndDrawing()
    {
        // set the collider and the mesh
        if (objectType == ObjectType.Shape)
        {
            shape.SetMeshCollider();
            ObjectManager.instance.AddObject(shape.gameObject);
            shape.gameObject.AddComponent<MeshEditor>().StartGroupGeneration();
            shape = null;

            // add reflection
            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<ProcShape>().SetMeshCollider();
                ObjectManager.instance.AddObject(ms.reflection);
            }
        }
        else if (objectType == ObjectType.Section)
        {
            section.SetMeshCollider();
            ObjectManager.instance.AddObject(section.gameObject);
            section.gameObject.GetComponent<MeshEditor>().StartGroupGeneration();
            section = null;

            // add reflection
            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<ProcSection>().SetMeshCollider();
                ObjectManager.instance.AddObject(ms.reflection);
            }
        }
        else if (objectType == ObjectType.Hair)
        {
            hair.SetMeshCollider();
            hair.gameObject.GetComponent<MeshEditor>().StartGroupGeneration();

            // add reflection
            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<ProcHair>().SetMeshCollider();
                ObjectManager.instance.AddObject(ms.reflection);
            }

            var refTrail = hair.GetComponent<HairSplineTrail>();
            GameObject trail = Instantiate(Resources.Load("NURBSTrail"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            trail.GetComponent<NURBSTrail>().InitNURBS(refTrail.spline, refTrail.trackLength, new Vector3(0.005f, 0, 0));
            trail.GetComponent<MeshRenderer>().material.color = hair.GetComponent<MeshRenderer>().material.color;
            hair = null;
            Destroy(hair.GetComponent<HairSplineTrail>());
            Destroy(hair);
            refTrail.Init(hairObject);
            ObjectManager.instance.AddObject(trail);
            Destroy(go.GetComponent<HairSplineTrail>());
        }
        if (ms.gameObject.activeSelf)
        {
            ms.reflection.GetComponent<MeshEditor>().StartGroupGeneration();
        }

        // set object cursor back
        objectCursor.SetActive(true);
        GameObject.Find("Tracker").GetComponent<TrackerScript>().numMesh++;
    }

    [PunRPC]
    public void ScaleDown()
    {
        if (objectType == ObjectType.Shape)
        {
            shapeObject.GetComponent<ProcShape>().ScaleShapeDown();
            if (shape != null)
                shape.ScaleShapeDown();
        }
        else if (objectType == ObjectType.Section)
        {
            sectionObject.GetComponent<ProcSection>().ScaleShapeDown();
            if (section != null)
                section.ScaleShapeDown();
        }
    }

    [PunRPC]
    public void ScaleUp()
    {
        if (objectType == ObjectType.Shape)
        {
            shapeObject.GetComponent<ProcShape>().ScaleShapeUp();
            if (shape != null)
                shape.ScaleShapeUp();
        }
        else if (objectType == ObjectType.Section)
        {
            sectionObject.GetComponent<ProcSection>().ScaleShapeUp();
            if (section != null)
                section.ScaleShapeUp();
        }
    }

    [PunRPC]
    void ChangeShape(ObjectType typeObject, int index)
    {
        if (typeObject == ObjectType.Shape)
        {
            SetToObjectType(LineTool.ObjectType.Shape);
            ProcShape shape = shapeObject.GetComponent<ProcShape>();
            shape.radialSegmentCount = index;
        }
        if (typeObject == ObjectType.Section)
        {
            SetToObjectType(LineTool.ObjectType.Section);
            ProcSection sect = sectionObject.GetComponent<ProcSection>();
            sect.shapeType = ShapeTypes[index];
        }
        if (typeObject == ObjectType.Hair)
        {
            SetToObjectType(LineTool.ObjectType.Hair);
            ProcHair hair = hairObject.GetComponent<ProcHair>();
            hair.radialSegmentCount = index;
        }    
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ToolType>())
        {
            tooltype = other.GetComponent<ToolType>().tooltype;
        }
    }

    public void Upscale()
    {
        photonView.RPC("ScaleUp", PhotonTargets.AllBufferedViaServer);
    }

    public void Downscale()
    {
        photonView.RPC("ScaleDown", PhotonTargets.AllBufferedViaServer);
    }
}
