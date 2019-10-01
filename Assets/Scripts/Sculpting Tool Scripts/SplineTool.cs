using UnityEngine;
using System.Collections;

/// <summary>
/// spline tool is handled here and works somewhat like the line tool, with some major differences
/// other things are handled under ProcShape,ProcLine,ProcHair as to how they're actually generated
/// </summary>
public class SplineTool : ToolBase
{
    private GameObject scene;

    enum ObjectType { Shape, Section, Hair }
    ObjectType objectType = ObjectType.Shape;

    GameObject go;
    public GameObject shapeObject;
    public GameObject sectionObject;
    public GameObject hairObject;
    private Vector3 offset;
    public bool ConstrainX { get { return ConstraintManager.ConstrainX; } }
    public bool ConstrainY { get { return ConstraintManager.ConstrainY; } }
    public bool ConstrainZ { get { return ConstraintManager.ConstrainZ; } }
    ProcShape shape;
    ProcSection section;
    ProcHair hair;
    Vector3 startPosition;
    Vector3 LastUpdatePosition;
    private float MinUpdateDistance = 0.0f;
    private float timeUpdate = 0.03f;
    float timeLeft = 0.0f;
    float timeSizeLeft = 0.0f;
    string tooltype;
    public MirrorScript ms;
    private Vector3 StartPoint;
    private ProcSection.ShapeType[] ShapeTypes = { ProcSection.ShapeType.T, ProcSection.ShapeType.I, ProcSection.ShapeType.BRACKET, ProcSection.ShapeType.LEFTCORNER, ProcSection.ShapeType.RIGHTCORNER };
    ushort pulseLength = 7000;
    public GameObject colorWheel;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        // checks if the object are assigned
        if (shapeObject == null || sectionObject == null || hairObject == null)
        {
            shapeObject.GetComponent<ProcShape>().Init();
            sectionObject.GetComponent<ProcSection>().Init();
            hairObject.GetComponent<ProcHair>().Init();
        }

        offset = shapeObject.transform.position - transform.position;

        scene = GameObject.Find("TestScene");

        SetObject();

        trackerLetter = "S";
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
            LastUpdatePosition = transform.position;
            photonView.RPC("StartDrawing", PhotonTargets.AllBufferedViaServer);
        }
        else if(controller.triggerButtonPressed && LastUpdatePosition != null && Vector3.Distance(LastUpdatePosition , transform.position) > MinUpdateDistance)
        {
            LastUpdatePosition = transform.position;
            photonView.RPC("UpdateDrawing", PhotonTargets.AllBufferedViaServer, shapeObject.transform.position, transform.up, transform.forward, transform.right);
            timeLeft = timeUpdate;
        }
        else if (controller.triggerButtonUp)
        {
            photonView.RPC("EndDrawing", PhotonTargets.AllBufferedViaServer);
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

    void SetToObjectType(ObjectType ot)
    {
        objectType = ot;
        SetObject();
    }

    
    [PunRPC]
    void ChangeShape(ObjectType typeObject, int index)
    {
        controller.DeviceController.TriggerHapticPulse(pulseLength);
        if (typeObject == ObjectType.Shape)
        {
            SetToObjectType(SplineTool.ObjectType.Shape);
            ProcShape shape = shapeObject.GetComponent<ProcShape>();
            shape.radialSegmentCount = index;
        }
        if (typeObject == ObjectType.Section)
        {
            SetToObjectType(SplineTool.ObjectType.Section);
            ProcSection sect = sectionObject.GetComponent<ProcSection>();
            sect.shapeType = ShapeTypes[index];
        }
        if (typeObject == ObjectType.Hair)
        {
            SetToObjectType(SplineTool.ObjectType.Hair);
            ProcHair hair = hairObject.GetComponent<ProcHair>();
            hair.radialSegmentCount = index;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<ToolType>())
        {
            tooltype = other.GetComponent<ToolType>().tooltype;
        }
    }

    void SetObject()
    {
        // checks for the object
        if (objectType == ObjectType.Shape)
        {
            shapeObject.SetActive(true);
            sectionObject.SetActive(false);
            hairObject.SetActive(false);
        }
        else if (objectType == ObjectType.Section)
        {
            shapeObject.SetActive(false);
            sectionObject.SetActive(true);
            hairObject.SetActive(false);
        }
        else if (objectType == ObjectType.Hair)
        {
            shapeObject.SetActive(false);
            sectionObject.SetActive(false);
            hairObject.SetActive(true);
        }
    }

    // sets the beginning position for line tool
    [PunRPC]
    public void StartDrawing()
    {
        if (objectType == ObjectType.Shape)
        {
            go = Instantiate(Resources.Load("ShapeSplineTrail"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            go.GetComponent<ShapeSplineTrail>().Init(shapeObject);
            StartPoint = GridSnapTool.Snap(shapeObject.transform.position);
            // checks if there is a mirror -> make a copy
            if (ms.gameObject.activeSelf)
            {
                ms.CreateReflection(go, scene.transform);
                ms.reflection.GetComponent<ShapeSplineTrail>().Init(shapeObject);
            }
        }
        else if (objectType == ObjectType.Section)
        {
            go = Instantiate(Resources.Load("SectionSplineTrail"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            go.GetComponent<SectionSplineTrail>().Init(sectionObject);
            StartPoint = GridSnapTool.Snap(sectionObject.transform.position);
            // checks if there is a mirror -> make a copy
            if (ms.gameObject.activeSelf)
            {
                ms.CreateReflection(go, scene.transform);
                ms.reflection.GetComponent<SectionSplineTrail>().Init(sectionObject);
            }
        }
        else if (objectType == ObjectType.Hair)
        {
            go = Instantiate(Resources.Load("HairSplineTrail"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            go.GetComponent<HairSplineTrail>().Init(hairObject);
            StartPoint = GridSnapTool.Snap(hairObject.transform.position);
            // checks if there is a mirror -> make a copy
            if (ms.gameObject.activeSelf)
            {
                ms.CreateReflection(go, scene.transform);
                ms.reflection.GetComponent<HairSplineTrail>().Init(hairObject);
            }
        }
    }

    [PunRPC]
    public void UpdateDrawing(Vector3 pos, Vector3 up, Vector3 forward, Vector3 right)
    {
        // set the collider and the mesh
        if (objectType == ObjectType.Shape)
        {
            shape = shapeObject.GetComponent<ProcShape>();
            Vector3 actualPosition = pos;
            if (ConstrainX) actualPosition.x = StartPoint.x;
            if (ConstrainY) actualPosition.y = StartPoint.y;
            if (ConstrainZ) actualPosition.z = StartPoint.z;

            //shape.SetEndPoint(actualPosition);
            go.GetComponent<ShapeSplineTrail>().BuildMeshTrail(GridSnapTool.Snap(actualPosition), up, forward, right);

            // update reflection
            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<ShapeSplineTrail>().BuildMeshTrail(ms.ReflectPoint(GridSnapTool.Snap(actualPosition)), ms.ReflectVector(up), ms.ReflectVector(forward), ms.ReflectVector(right));
            }
            ToolTracker.net[1] = 1;
            ToolTracker.net[2] = 2;
            ToolTracker.net[3] = 1;
            //ToolTracker.net[4] = (int)shapeObject.GetComponent<ProcShape>().m_Radius;
        }
        else if (objectType == ObjectType.Section)
        {
            Vector3 actualPosition = pos;
            section = sectionObject.GetComponent<ProcSection>();
            if (ConstrainX) actualPosition.x = StartPoint.x;
            if (ConstrainY) actualPosition.y = StartPoint.y;
            if (ConstrainZ) actualPosition.z = StartPoint.z;

            //section.SetEndPoint(actualPosition);
            go.GetComponent<SectionSplineTrail>().BuildMeshTrail(GridSnapTool.Snap(actualPosition), up, forward, right);

            // update reflection
            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<SectionSplineTrail>().BuildMeshTrail(ms.ReflectPoint(GridSnapTool.Snap(actualPosition)), -ms.ReflectVector(up), -ms.ReflectVector(forward), -ms.ReflectVector(right));
            }
            ToolTracker.net[1] = 2;
            ToolTracker.net[2] = 2;
            ToolTracker.net[3] = 1;
            //ToolTracker.net[4] = (int)sectionObject.GetComponent<ProcShape>().m_Radius;
        }
        else if (objectType == ObjectType.Hair)
        {
            Vector3 actualPosition = pos;
            hair = sectionObject.GetComponent<ProcHair>();
            if (ConstrainX) actualPosition.x = StartPoint.x;
            if (ConstrainY) actualPosition.y = StartPoint.y;
            if (ConstrainZ) actualPosition.z = StartPoint.z;

            go.GetComponent<HairSplineTrail>().BuildMeshTrail(GridSnapTool.Snap(actualPosition), up, forward, right , false);

            // update reflection
            if (ms.gameObject.activeSelf)
            {
                ms.reflection.GetComponent<HairSplineTrail>().BuildMeshTrail(ms.ReflectPoint(GridSnapTool.Snap(actualPosition)), -ms.ReflectVector(up), -ms.ReflectVector(forward), -ms.ReflectVector(right) , false);
            }
        }
    }

    [PunRPC]
    public void EndDrawing()
    {
        // set the collider and the mesh
        if (go != null)
        {
            if (objectType == ObjectType.Shape)
            {
                go.GetComponent<ShapeSplineTrail>().CancelTracking();

                // end reflection
                if (ms.gameObject.activeSelf)
                {
                    ms.reflection.GetComponent<ShapeSplineTrail>().CancelTracking();
                }
                GameObject.Find("Tracker").GetComponent<TrackerScript>().numMesh++;
                ObjectManager.instance.AddObject(go);
                go.GetComponent<MeshEditor>().StartGroupGeneration();
            }
            else if (objectType == ObjectType.Section)
            {
                go.GetComponent<SectionSplineTrail>().CancelTracking();

                // end reflection
                if (ms.gameObject.activeSelf)
                {
                    ms.reflection.GetComponent<SectionSplineTrail>().CancelTracking();
                }
                GameObject.Find("Tracker").GetComponent<TrackerScript>().numMesh++;
                ObjectManager.instance.AddObject(go);
                go.GetComponent<MeshEditor>().StartGroupGeneration();
            }
            else if (objectType == ObjectType.Hair)
            {
                go.GetComponent<HairSplineTrail>().CancelTracking();

                // end reflection
                if (ms.gameObject.activeSelf)
                {
                    ms.reflection.GetComponent<HairSplineTrail>().CancelTracking();
                }
                GameObject.Find("Tracker").GetComponent<TrackerScript>().numMesh++;

                var refTrail = go.GetComponent<HairSplineTrail>();
                GameObject trail = Instantiate(Resources.Load("NURBSTrail"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                trail.GetComponent<NURBSTrail>().InitNURBS(refTrail.spline, refTrail.trackLength, new Vector3(0.005f, 0, 0));
                trail.GetComponent<MeshRenderer>().material.color = go.GetComponent<MeshRenderer>().material.color;
                Destroy(go);
                refTrail.Init(hairObject);
                ObjectManager.instance.AddObject(trail);
            }

            shapeObject.GetComponent<ProcShape>().Init();
            sectionObject.GetComponent<ProcSection>().Init();
            hairObject.GetComponent<ProcHair>().Init();
 
            ToolTracker.setEmpty(4);
            TrackerScript.AddAction();
        }

        // add reflection
        if (ms.gameObject.activeSelf)
        {
            ObjectManager.instance.AddObject(ms.reflection);
        }

        go = null;
    }

    [PunRPC]
    public void ScaleDown()
    {
        if (objectType == ObjectType.Shape)
        {
            shapeObject.GetComponent<ProcShape>().ScaleShapeDown();
        }
        else if (objectType == ObjectType.Section)
        {
            sectionObject.GetComponent<ProcSection>().ScaleShapeDown();
        }
    }

    [PunRPC]
    public void ScaleUp()
    {
        if (objectType == ObjectType.Shape)
        {
            shapeObject.GetComponent<ProcShape>().ScaleShapeUp();
        }
        else if (objectType == ObjectType.Section)
        {
            sectionObject.GetComponent<ProcSection>().ScaleShapeUp();
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
