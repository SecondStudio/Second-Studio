using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SweepTool : ToolBase
{
    // how often does the script track object's position
    public float seconds = 0.1f;
    public float trackLength = 0.05f;

    public GameObject scene;

    private Transform cursorTransform;

    // mesh data for rendering and collision
    private MeshCollider meshCollider;
    private MeshBuilder meshBuilder;
    private MeshFilter filter;

    // tracks the transform of the cursor at lenght of spline
    List<Vector3> knotBinormals;
    List<float> knotLengths;

    int startKnot;

    private Vector3 lastPosition;

    bool isRunning = false;

    GameObject lineCursor;
    GameObject meshObject;
    GameObject go;

    bool flipped = false;

    private float timeUpdate = 0.03f;
    float timeLeft = 0.0f;

    // spline for trail
    [HideInInspector]
    public CatmullRomSpline spline;

    // Use this for initialization
    void Start()
    {
        base.Start();
        lineCursor = transform.Find("LineCursor").gameObject;
        meshObject = transform.Find("Mesh").gameObject;

        lastPosition = lineCursor.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (!photonView.isMine)
            return;

        if (timeLeft > 0)
            timeLeft -= Time.deltaTime;

        // instantiates and records a new mesh
        if (controller.triggerButtonDown || Input.GetKeyDown(KeyCode.A))
        {
            // creates new mesh object
            photonView.RPC("Init", PhotonTargets.AllBufferedViaServer);
        }

        if (controller.triggerButtonUp || Input.GetKeyDown(KeyCode.D))
        {
            photonView.RPC("CancelTracking", PhotonTargets.AllBufferedViaServer);
        }

        // scale the line to be used for the tool
        if (controller.dpadAxis.x < -0.2)
        {
            if (lineCursor.transform.localScale.z > 0.1f)
                photonView.RPC("ScaleDown", PhotonTargets.AllBufferedViaServer);
            timeLeft = timeUpdate;
        }

        else if (controller.dpadAxis.x > 0.2)
        {
            if (lineCursor.transform.localScale.z < 0.5f)
                photonView.RPC("ScaleUp", PhotonTargets.AllBufferedViaServer);
            timeLeft = timeUpdate;
        }
    }

    [PunRPC]
    public void ScaleUp()
    {
        lineCursor.transform.localScale += new Vector3(0, 0, 0.01f);
    }

    [PunRPC]
    public void ScaleDown()
    {
        lineCursor.transform.localScale -= new Vector3(0, 0, 0.01f);
    }

    public void BuildMeshTrail()
    {
        // Checks and update the spline path for cursor
        List<Knot> knots = spline.knots;
        Vector3 point = cursorTransform.position;

        knots[knots.Count - 1].position = point;
        knots[knots.Count - 2].position = point;

        if (Vector3.Distance(knots[knots.Count - 3].position, point) > trackLength &&
            Vector3.Distance(knots[knots.Count - 4].position, point) > trackLength) // point exceeds length path travelled
        {
            // add knot and the normals at the point
            knots.Add(new Knot(point));
            spline.Parametrize();

            addCursorTransform();

            UpdateMesh(); // update mesh
        }
    }

    private void addCursorTransform()
    {
        knotBinormals.Add(cursorTransform.forward * lineCursor.transform.localScale.z);
        knotLengths.Add(spline.Length());
    }

    //Build the mesh:
    public void UpdateMesh()
    {
        List<Knot> knots = spline.knots;

        CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();

        int maxPoint = meshBuilder.Vertices.Count;

        Vector3 position = new Vector3(0, 0, 0);
        Vector3 lastPosition = new Vector3(0, 0, 0);
        Vector3 t = new Vector3(0, 0, 0);

        for (int k = 0; k < 3; ++k)
        {
            int beginKnot = startKnot + k;
            int endKnot = startKnot + k + 1;

            float beginDistance = knotLengths[beginKnot];
            float endDistance = knotLengths[endKnot];

            int begin = (int)(beginDistance / trackLength);
            int end = (int)(endDistance / trackLength);

            float totalLength = endDistance - beginDistance;

            spline.PlaceMarker(marker, trackLength * begin - 1);
            lastPosition = spline.GetPosition(marker);

            for (int i = begin; i < end; ++i)
            {
                // place the marker at spline to get distance
                float distance = trackLength * i;
                spline.PlaceMarker(marker, distance);

                // get the variable needed for the point on spline
                position = spline.GetPosition(marker);

                // finds transform for cursor at point in time
                Vector3 bn = Vector3.Lerp(knotBinormals[endKnot], knotBinormals[beginKnot], (endDistance - distance) / totalLength);

                // find via cross product
                Vector3 normal = Vector3.Cross(position - lastPosition, bn);
                Vector3 linePoint = bn * 0.5f;

                // find points for mesh vertices
                Vector3 point = position + linePoint;
                Vector3 point2 = position - linePoint;

                // build or update mesh
                if (i * 4 >= maxPoint)
                    BuildQuad(meshBuilder, point, point2, normal);
                else
                    UpdateQuad(meshBuilder, point, point2, normal, i);

                // gets last position
                lastPosition = position;
            }
        }

        ++startKnot;

        // creates mesh for filter and renderer
        CreateMesh();
    }

    void SetCollider()
    {
        go.GetComponent<MeshCollider>().sharedMesh = meshBuilder.CreateMesh();
    }


    public void CreateMesh()
    {
        //Build the mesh:
        Mesh mesh = meshBuilder.CreateMesh();

        mesh.RecalculateNormals();

        //Look for a MeshFilter component attached to this GameObject:
        MeshFilter filter = go.GetComponent<MeshFilter>();

        //If the MeshFilter exists, attach the new mesh to it.
        //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    // builds the shape
    protected void BuildQuad(MeshBuilder meshBuilder, Vector3 point, Vector3 point2, Vector3 normal)
    {
        Vector2 uv = new Vector2(0f, 0f);

        meshBuilder.Vertices.Add(point);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point2);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point);
        meshBuilder.Normals.Add(-normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point2);
        meshBuilder.Normals.Add(-normal);
        meshBuilder.UVs.Add(uv);

        int baseIndex = meshBuilder.Vertices.Count - 8;

        if (baseIndex >= 0)
        {
            meshBuilder.AddTriangle(baseIndex + 4, baseIndex + 5, baseIndex);
            meshBuilder.AddTriangle(baseIndex + 5, baseIndex + 1, baseIndex);
            meshBuilder.AddTriangle(baseIndex + 2, baseIndex + 7, baseIndex + 6);
            meshBuilder.AddTriangle(baseIndex + 2, baseIndex + 3, baseIndex + 7);
        }
    }

    // update previous quads
    protected void UpdateQuad(MeshBuilder meshBuilder, Vector3 point, Vector3 point2, Vector3 normal, int i)
    {
        int baseIndex = i * 4;

        meshBuilder.Vertices[baseIndex] = point;
        meshBuilder.Normals[baseIndex] = normal;

        meshBuilder.Vertices[baseIndex + 1] = point2;
        meshBuilder.Normals[baseIndex + 1] = normal;

        meshBuilder.Vertices[baseIndex + 2] = point;
        meshBuilder.Normals[baseIndex + 2] = -normal;

        meshBuilder.Vertices[baseIndex + 3] = point2;
        meshBuilder.Normals[baseIndex + 3] = -normal;
    }

    [PunRPC]
    public void Init()
    {
        if (lineCursor == null)
        {
            Debug.Log("GameObject is not assigned!");
            Application.Quit();
        }

        // setups the script to track input cursor in 3D space
        cursorTransform = lineCursor.transform;

        //Get MeshRenderer and set to color of the cursor
        MeshRenderer mr = GetComponent<MeshRenderer>();

        //Create a new mesh builder:
        meshBuilder = new MeshBuilder();

        // IMPORTANT: instantiates new object for mesh
        go = Instantiate(meshObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        go.tag = "Trail";

        if (scene != null)
            go.transform.parent = scene.transform;

        //Look for a MeshFilter component attached to this GameObject:
        filter = go.GetComponent<MeshFilter>();

        //Look for a MeshCollider component attached to this GameObject:
        meshCollider = go.GetComponent<MeshCollider>();

        // creates the spline
        spline = new CatmullRomSpline();

        // add knots via position
        List<Knot> knots = spline.knots;
        Vector3 point = cursorTransform.position;

        // initiate normals at the starting knot
        knotBinormals = new List<Vector3>();
        knotLengths = new List<float>();

        for (int i = 0; i < 3; ++i)
        {
            addCursorTransform();
        }

        knots.Add(new Knot(point));
        knots.Add(new Knot(point));
        knots.Add(new Knot(point));
        knots.Add(new Knot(point));
        knots.Add(new Knot(point));

        startKnot = 0;

        //Build the mesh:
        StartTracking();
    }


    public void StartTracking()
    {
        InvokeRepeating("BuildMeshTrail", 0, seconds);
        isRunning = true;
    }

    [PunRPC]
    public void CancelTracking()
    {
        if (isRunning)
        {
            CancelInvoke();

            // generates the caps for the trail
            List<Knot> knots = spline.knots;

            //If the MeshFilter exists, attach the new mesh to it.
            //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
            if (filter != null)
            {
                Mesh mesh = meshBuilder.CreateMesh();
                mesh.RecalculateNormals();

                filter.sharedMesh = mesh;
                meshCollider.sharedMesh = mesh;
            }

            isRunning = false;
        }
    }

}
