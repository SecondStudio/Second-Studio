using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SectionSplineTrail : MonoBehaviour
{
    // how often does the script track object's position
    public float seconds = 0.03f;
    public float trackLength = 0.3f;

    // stores cursor for tracking
    Transform cursorTransform;

    // mesh data for rendering and collision
    private MeshCollider meshCollider;
    private MeshBuilder meshBuilder;
    private MeshFilter filter;

    // determines the type of section rendered
    private ProcSection.ShapeType shapeType;

    // stores the data needed to render section
    private Vector3[] cornerNormals;
    private Vector3[] shapePoints;
    private int[] normalsIndices;

    // tracks the transform of the cursor at lenght of spline
    List<Vector3> knotNormals;
    List<Vector3> knotBinormals;
    List<Vector3> knotTangents;
    List<float> knotLengths;
    List<float> knotWidth;
    List<float> knotHeight;
    List<float> knotThickness;

    // global variables
    private int startKnot;
    private Vector3 splineTangent;

    private Vector3 lastPosition;
    private Vector3 lastTangent;


    private Vector3 firstPosition;
    private Vector3 firstNormal;
    private Vector3 firstBinormal;
    private Vector3 firstTangent;
    private float firstHeight;
    private float firstWidth;
    private float firstThickness;

    private bool first = true;

    // spline for trail
    [HideInInspector]
    public CatmullRomSpline spline;

    // boolean of whether tracking is running active
    [HideInInspector]
    public bool isRunning = false;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            CancelTracking();
    }

    public void BuildMeshTrail(Vector3 pos, Vector3 up, Vector3 forward, Vector3 right)
    {
        // Checks and update the spline path for cursor
        List<Knot> knots = spline.knots;
        Vector3 point = pos;

        knots[knots.Count - 1].position = point;
        knots[knots.Count - 2].position = point;

        if (Vector3.Distance(knots[knots.Count - 3].position, point) > trackLength &&
            Vector3.Distance(knots[knots.Count - 4].position, point) > trackLength) // point exceeds length path travelled
        {

            // add knot and the normals at the point
            knots.Add(new Knot(point));
            spline.Parametrize();

            addCursorTransform(up, forward, right);

            BuildMesh(); // update mesh
        }
    }

    //Build the mesh:
    public void BuildMesh()
    {
        CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();

        int maxPoint = meshBuilder.Vertices.Count;

        Vector3 position = new Vector3(0, 0, 0);
        Vector3 t = new Vector3(0, 0, 0);

        float width = 0;
        float height = 0;
        float thickness = 0;

        for (int k = 0; k < 3; ++k)
        {
            int beginKnot = startKnot + k;
            int endKnot = startKnot + k + 1;

            float beginDistance = knotLengths[beginKnot];
            float endDistance = knotLengths[endKnot];

            int begin = (int)(beginDistance / trackLength);
            int end = (int)(endDistance / trackLength);

            float totalLength = endDistance - beginDistance;

            for (int i = begin; i < end; ++i)
            {
                if (i != 0 && beginKnot >= 3)
                {
                    // place the marker at spline to get distance
                    float distance = trackLength * i;
                    spline.PlaceMarker(marker, distance);

                    // get the variable needed for the point on spline
                    position = spline.GetPosition(marker);
                    splineTangent = spline.GetTangent(marker);

                    // finds transform for cursor at point in time
                    Vector3 n = Vector3.Lerp(knotNormals[endKnot], knotNormals[beginKnot], (endDistance - distance) / totalLength);
                    Vector3 bn = Vector3.Lerp(knotBinormals[endKnot], knotBinormals[beginKnot], (endDistance - distance) / totalLength);
                    t = Vector3.Lerp(knotTangents[endKnot], knotTangents[beginKnot], (endDistance - distance) / totalLength);

                    width = Mathf.Lerp(knotWidth[endKnot], knotWidth[beginKnot], (endDistance - distance) / totalLength);
                    height = Mathf.Lerp(knotHeight[endKnot], knotHeight[beginKnot], (endDistance - distance) / totalLength);
                    thickness = Mathf.Lerp(knotThickness[endKnot], knotThickness[beginKnot], (endDistance - distance) / totalLength);

                    if (first)
                    {
                        first = false;
                        firstPosition = position;
                        firstNormal = n;
                        firstBinormal = bn;
                        firstTangent = t;

                        firstWidth = width;
                        firstHeight = height;
                        firstThickness = thickness;
                    }

                    // setup shape for specified section
                    SetupShape(n, bn, t, width, height, thickness);

                    if (i * (shapePoints.Length) >= maxPoint)
                        BuildNewShape(position, meshBuilder.Vertices.Count > 0); // add new vertices
                    else
                        BuildExistingShape(position, i); // update vertices and normals*/
                }
            }

        }

        lastTangent = t;
        lastPosition = position;

        ++startKnot;

        //If the MeshFilter exists, attach the new mesh to it.
        //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
        if (filter != null)
        {
            filter.sharedMesh = meshBuilder.CreateMesh();
        }
        
    }

    // Adds new vertices
    protected void BuildNewShape(Vector3 centre, bool buildTriangles)
    {
        int segmentCount = shapePoints.Length - 1; // gets number of points
        for (int i = 0; i <= segmentCount; i++)
        {
            // Adds the vertices based upon shape wrt centre
            meshBuilder.Vertices.Add(centre + shapePoints[i]);

            // Adds normals at corners of shape
            meshBuilder.Normals.Add(cornerNormals[normalsIndices[i]]);

            // UV texture
            meshBuilder.UVs.Add(new Vector2((float)i / segmentCount, 0));

            if (i > 0 && buildTriangles)
            {
                int baseIndex = meshBuilder.Vertices.Count - 1;

                int vertsPerRow = segmentCount + 1;

                int index0 = baseIndex;
                int index1 = baseIndex - 1;
                int index2 = baseIndex - vertsPerRow;
                int index3 = baseIndex - vertsPerRow - 1;

                meshBuilder.AddTriangle(index0, index2, index1);
                meshBuilder.AddTriangle(index2, index3, index1);
            }
        }
    }

    // Updates preexisting vertices
    protected void BuildExistingShape(Vector3 centre, int ringIndex)
    {
        int segmentCount = shapePoints.Length - 1; // gets number of points
        for (int i = 0; i <= segmentCount; i++)
        {
            // mesh vertices
            meshBuilder.Vertices[ringIndex * (segmentCount + 1) + i] = centre + shapePoints[i];
            meshBuilder.Normals[ringIndex * (segmentCount + 1) + i] = cornerNormals[normalsIndices[i]];
        }
    }

    // setup shape based on "ShapeType"
    protected void SetupShape(Vector3 normal, Vector3 binormal, Vector3 tangent, float width, float height, float thickness)
    {
        Vector3 bn = -binormal;
        Vector3 n = normal;

        // determines direction of binormal
        if (Vector3.Dot(splineTangent, tangent) < 0)
        {
            bn = -bn;
        }

        // corner normals
        Vector3[] normals =
        {
            -bn + n, // 0: upper-left
            bn + n, // 1: upper-right
            bn - n, // 2: lower-right
            -bn - n, // 3: lower-left
        };

        // normalize the vectors
        for (int i = 0; i < normals.Length; ++i)
            normals[i].Normalize();

        cornerNormals = normals;

        // Creates variables for getting points of letter
        Vector3 up = n * height / 2.0f;
        Vector3 h_thickness = n * thickness / 2.0f;

        Vector3 right = bn * width / 2.0f;
        Vector3 w_thickness = bn * thickness / 2.0f;

        if (shapeType == ProcSection.ShapeType.I)
        {
            // shapePoints stores the points
            Vector3[] points = {

            // uppermost corners
                -right + up,
                right + up,

                // right arch
                right + up - h_thickness * 2,
                w_thickness + up - h_thickness * 2,
                w_thickness - up + h_thickness * 2,
                right - up + h_thickness  * 2,

                // lowermost corners
                right - up,
                - right - up,

                // left arch
                - right - up + h_thickness * 2,
                - w_thickness - up + h_thickness * 2,
                - w_thickness + up - h_thickness * 2,
                - right + up - h_thickness * 2,

                - right + up
            };

            int[] indices =
            {
                0, 1, 2, 2, 1, 1, 2, 3, 0, 0, 3, 3, 1
            };

            shapePoints = points;
            normalsIndices = indices;
        }
        else if (shapeType == ProcSection.ShapeType.T)
        {
            // shapePoints stores the points
            Vector3[] points = {

                // uppermost corners
                - right + up,
                right + up,

                // right arch
                right + up - h_thickness * 2,
                w_thickness + up - h_thickness * 2,

                // lowermost corners
                w_thickness - up,
                - w_thickness - up,

                // left arch
                - w_thickness + up - h_thickness * 2,
                - right + up - h_thickness * 2,

                - right + up
            };

            int[] indices =
            {
                0, 1, 2, 2, 2, 3, 3, 3, 0
            };

            shapePoints = points;
            normalsIndices = indices;
        }
        else if (shapeType == ProcSection.ShapeType.BRACKET)
        {
            // shapePoints stores the points
            Vector3[] points = {

                // uppermost corners
                - right + up,
                right + up,

                // right arch
                right + up - h_thickness * 2,
                - right + w_thickness * 2 + up - h_thickness * 2,
                - right + w_thickness * 2 - up + h_thickness * 2,
                right - up + h_thickness * 2,

                // lowermost corners
                right - up,
                - right - up,

                - right + up
            };

            int[] indices =
            {
                0, 1, 2, 2, 1, 1, 2, 3, 0
            };

            shapePoints = points;
            normalsIndices = indices;
        }
        else if (shapeType == ProcSection.ShapeType.LEFTCORNER)
        {
            // shapePoints stores the points
            Vector3[] points = {

                // uppermost corners
                - right + up,
                right + up,

                // right arch
                right + up - h_thickness * 2,
                - right + w_thickness * 2 + up - h_thickness * 2,

                // lowermost corners
                - right + w_thickness * 2 - up,
                - right - up,

                - right + up
            };

            int[] indices =
            {
                0, 1, 2, 2, 2, 3, 0
            };

            shapePoints = points;
            normalsIndices = indices;
        }
        else if (shapeType == ProcSection.ShapeType.RIGHTCORNER)
        {
            // shapePoints stores the points
            Vector3[] points = {

                // uppermost corners
                - right + up,
                right + up,

                // lowermost corners
                right - up,
                right - w_thickness * 2 - up,

                // left arch
                right - w_thickness * 2 + up - h_thickness * 2,
                - right + up - h_thickness * 2,

                - right + up
            };

            int[] indices =
            {
                0, 1, 2, 3, 3, 3, 0
            };

            shapePoints = points;
            normalsIndices = indices;
        }
    }

    private void addCursorTransform()
    {
        knotNormals.Add(cursorTransform.up);
        knotBinormals.Add(cursorTransform.forward);
        knotTangents.Add(cursorTransform.right);
        knotLengths.Add(spline.Length());

        ProcSection ps = cursorTransform.GetComponent<ProcSection>();
        knotHeight.Add(ps.m_Height);
        knotWidth.Add(ps.m_Width);
        knotThickness.Add(ps.m_Thickness);
    }

    private void addCursorTransform(Vector3 up, Vector3 forward, Vector3 right)
    {
        knotNormals.Add(up);
        knotBinormals.Add(forward);
        knotTangents.Add(right);
        knotLengths.Add(spline.Length());

        ProcSection ps = cursorTransform.GetComponent<ProcSection>();
        knotHeight.Add(ps.m_Height);
        knotWidth.Add(ps.m_Width);
        knotThickness.Add(ps.m_Thickness);
    }

    private void OnDrawGizmos()
    {
        /*
        if (spline != null)
        {
            spline.DebugDrawSpline();
        }
        */
    }

    /// <summary>
    /// Adds a cap to the top or bottom of the section.
    /// </summary>
    /// <param name="meshBuilder">The mesh builder currently being added to.</param>
    /// <param name="centre">The postion at the centre of the cap.</param>
    /// <param name="reverseDirection">Should the normal and winding order of the cap be reversed? (Should be true for bottom cap, false for the top)</param>
    private void BuildCap(MeshBuilder meshBuilder, Vector3 centre, Vector3 dir, bool reverseDirection)
    {

        // determines how the cap will be formed based upon vertices
        if (shapeType == ProcSection.ShapeType.T)
        {
            int[] quad = { 0, 1, 2, 7 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad);

            int[] quad2 = { 3, 4, 5, 6 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad2);
        }
        else if (shapeType == ProcSection.ShapeType.I)
        {
            int[] quad = { 0, 1, 2, 11 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad);

            int[] quad2 = { 3, 4, 9, 10 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad2);

            int[] quad3 = { 5, 6, 7, 8 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad3);
        }
        else if (shapeType == ProcSection.ShapeType.BRACKET)
        {
            int[] quad = { 0, 1, 2, 3 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad);

            int[] quad2 = { 0, 3, 4, 7 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad2);

            int[] quad3 = { 4, 5, 6, 7 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad3);
        }
        else if (shapeType == ProcSection.ShapeType.LEFTCORNER)
        {
            int[] quad = { 0, 1, 2, 3 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad);

            int[] quad2 = { 0, 3, 4, 5 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad2);
        }
        else if (shapeType == ProcSection.ShapeType.RIGHTCORNER)
        {
            int[] quad = { 0, 1, 4, 5 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad);

            int[] quad2 = { 4, 1, 2, 3 };
            addQuad(meshBuilder, reverseDirection, centre, dir, quad2);
        }

    }

    // adds quads for the vertices
    private void addQuad(MeshBuilder meshBuilder, bool reverseDirection, Vector3 centre, Vector3 dir, int[] vertices)
    {
        //the normal will either be up or down:
        Vector3 normal = reverseDirection ? dir : -dir;

        int num;

        for (int i = 0; i < 4; ++i)
        {
            if (reverseDirection)
                num = 3 - i;
            else num = i;

            meshBuilder.Vertices.Add(centre + shapePoints[vertices[num]]);
            meshBuilder.Normals.Add(normal);
            Vector2 uv = new Vector2(shapePoints[vertices[num]].x + 1.0f, shapePoints[vertices[num]].y + 1.0f) * 0.5f;
            meshBuilder.UVs.Add(uv);
        }

        int baseIndex = meshBuilder.Vertices.Count - 4;

        meshBuilder.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
        meshBuilder.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
    }

    public void Init(GameObject Cursor)
    {
        Debug.Log("Initiated!");

        if (Cursor == null)
        {
            Debug.Log("GameObject 'Shape' is not assigned!");
            Application.Quit();
        }

        // setups the script to track input cursor in 3D space
        cursorTransform = Cursor.transform;
        transform.position = new Vector3(0, 0, 0);

        // gets radial segment and radius
        ProcSection ps = Cursor.GetComponent<ProcSection>();
        shapeType = ps.shapeType;

        //Get MeshRenderer and set to color of the cursor
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.color = ps.m_RGB;

        //Create a new mesh builder:
        meshBuilder = new MeshBuilder();

        //Look for a MeshFilter component attached to this GameObject:
        filter = GetComponent<MeshFilter>();

        //Look for a MeshCollider component attached to this GameObject:
        meshCollider = GetComponent<MeshCollider>();

        // creates the spline
        if (spline == null)
        {
            spline = new CatmullRomSpline();

            // add knots via position
            List<Knot> knots = spline.knots;
            Vector3 point = cursorTransform.position;

            // initiate normals at the starting knot
            knotNormals = new List<Vector3>();
            knotBinormals = new List<Vector3>();
            knotTangents = new List<Vector3>();
            knotLengths = new List<float>();
            knotWidth = new List<float>();
            knotHeight = new List<float>();
            knotThickness = new List<float>();

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
        }

        //Build the mesh:
        StartTracking();
    }

    public void StartTracking()
    {
//        InvokeRepeating("BuildMeshTrail", 0, seconds);
        isRunning = true;
    }

    public void CancelTracking()
    {
        if (isRunning)
        {
            CancelInvoke();

            if (knotLengths.Count >= 4)
            {
                // generates the caps for the trail
                CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();

                // end cap
                splineTangent = spline.GetTangent(marker);
                BuildCap(meshBuilder, lastPosition, lastTangent, true);

                // begin cap
                spline.PlaceMarker(marker, knotLengths[3]);
                splineTangent = spline.GetTangent(marker);

                SetupShape(firstNormal, firstBinormal, firstTangent, firstWidth, firstHeight, firstThickness);

                BuildCap(meshBuilder, firstPosition, firstTangent, false);

                //If the MeshFilter exists, attach the new mesh to it.
                //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
                if (filter != null)
                {
                    Mesh mesh = meshBuilder.CreateMesh();

                    // center the mesh properly in 3D world
                    filter.transform.position = mesh.bounds.center;
                    var vertices = mesh.vertices;
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        vertices[i] -= mesh.bounds.center;
                    }
                    mesh.vertices = vertices;
                    mesh.RecalculateBounds();

                    // assigns mesh
                    filter.sharedMesh = mesh;
                    meshCollider.inflateMesh = true;
                    meshCollider.sharedMesh = mesh;
                }
            }
            else Destroy(filter.gameObject);

            isRunning = false;
            //if(FindObjectOfType<EdgeTool>() != null || FindObjectOfType<FaceTool>() != null || FindObjectOfType<HandleTool>() != null) GetComponent<MeshEditor>().GenerateHandles();
            first = true;
        }
    }
}
