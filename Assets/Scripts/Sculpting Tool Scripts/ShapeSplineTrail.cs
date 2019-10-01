using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapeSplineTrail : MonoBehaviour
{
    // how often does the script track object's position
    public float seconds = 0.03f;
    public float trackLength = 0.03f;

    // stores cursor for tracking
    Transform cursorTransform;

    // mesh data for rendering and collision
    private MeshBuilder meshBuilder;
    private MeshCollider meshCollider;
    private MeshFilter filter;

    // controls radius and shape of the mesh
    private float m_Radius = 0.2f; // determines radius
    private int m_RadialSegmentCount; // determines shape

    // tracks the transform of the cursor at lenght of spline
    List<Vector3> knotNormals;
    List<Vector3> knotBinormals;
    List<Vector3> knotTangents;
    public List<float> knotLengths;
    List<float> knotRadius;

    // global variables
    private int startKnot;
    private Vector3 splineTangent;

    private Vector3 lastPosition;
    private Vector3 lastNormal;
    private Vector3 lastBinormal;
    private Vector3 lastTangent;
    private float lastRadius;

    private Vector3 firstPosition;
    private Vector3 firstNormal;
    private Vector3 firstBinormal;
    private Vector3 firstTangent;
    private float firstRadius;

    private bool first = true;

    // spline for trail
    [HideInInspector]
    public CatmullRomSpline spline;

    // boolean of whether tracking is running active
    [HideInInspector]
    public bool isRunning = false;

    private void Start()
    {
//        GetComponent<MeshRenderer>().bounds.center;
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
        List<Knot> knots = spline.knots;

        CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();

        int maxPoint = meshBuilder.Vertices.Count;

        float distance = 0;
        Vector3 position = new Vector3(0, 0, 0);
        Vector3 n = new Vector3(0, 0, 0);
        Vector3 bn = new Vector3(0, 0, 0);
        Vector3 t = new Vector3(0, 0, 0);

        float r = 0;

        for (int k = 0; k < 3; ++k)
        {
            int beginKnot = startKnot + k;
            int endKnot = startKnot + k + 1;

            float beginDistance = knotLengths[beginKnot];
            float endDistance = knotLengths[endKnot];

            int begin = (int)(beginDistance / trackLength);
            int end = (int)(endDistance / trackLength);

            float totalLength = endDistance - beginDistance;
            float nextDistance = end * trackLength;

            for (int i = begin; i < end; ++i)
            {
                if (i != 0 && beginKnot >= 3)
                {
                    // place the marker at spline to get distance
                    distance = trackLength * i;

                    spline.PlaceMarker(marker, distance);

                    // get the variable needed for the point on spline
                    position = spline.GetPosition(marker);
                    splineTangent = spline.GetTangent(marker);

                    // finds transform for cursor at point in time
                    n = Vector3.Lerp(knotNormals[endKnot], knotNormals[beginKnot], (endDistance - distance) / totalLength);
                    bn = Vector3.Lerp(knotBinormals[endKnot], knotBinormals[beginKnot], (endDistance - distance) / totalLength);
                    t = Vector3.Lerp(knotTangents[endKnot], knotTangents[beginKnot], (endDistance - distance) / totalLength);

                    r = Mathf.Lerp(knotRadius[endKnot], knotRadius[beginKnot], (endDistance - distance) / totalLength);
                    if (first)
                    {
                        first = false;
                        firstPosition = position;
                        firstNormal = n;
                        firstBinormal = bn;
                        firstTangent = t;
                        firstRadius = r;
                    }


                    if (i * (m_RadialSegmentCount + 1) >= maxPoint)
                        BuildNewShape(position, n, bn, t, r, meshBuilder.Vertices.Count > 0); // add new vertices
                    else
                        BuildExistingShape(position, n, bn, t, r, i); // update vertices and normals*/
                }
            }

            
        }

        ++startKnot;

        lastPosition = position;
        lastNormal = n;
        lastBinormal = bn;
        lastTangent = t;
        lastRadius = r;

        //If the MeshFilter exists, attach the new mesh to it.
        //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
        if (filter != null)
        {
            filter.sharedMesh = meshBuilder.CreateMesh();

        }
    }

    // Adds new vertices
    protected void BuildNewShape(Vector3 centre, Vector3 normal, Vector3 binormal, Vector3 tangent, float radius, bool buildTriangles)
    {
        float angleInc = (Mathf.PI * 2.0f) / m_RadialSegmentCount;

        for (int i = 0; i <= m_RadialSegmentCount; i++)
        {
            // use spline to render the points
            float angle = angleInc * i;

            // Finds the radial position wrt direction
            Vector3 right = Mathf.Sin(angle) * -binormal;
            Vector3 forward = Mathf.Cos(angle) * normal;
            Vector3 unitPosition = right + forward;

            // determines direction of binormal
            if(Vector3.Dot(splineTangent, tangent) < 0)
            {
                unitPosition = -right + forward;
            }

            // mesh vertices
            meshBuilder.Vertices.Add(centre + unitPosition * radius);

            meshBuilder.Normals.Add(unitPosition);

            meshBuilder.UVs.Add(new Vector2((float)i / m_RadialSegmentCount, 0));

            if (i > 0 && buildTriangles)
            {
                int baseIndex = meshBuilder.Vertices.Count - 1;

                int vertsPerRow = m_RadialSegmentCount + 1;

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
    protected void BuildExistingShape(Vector3 centre, Vector3 normal, Vector3 binormal, Vector3 tangent, float radius, int ringIndex)
    {
        float angleInc = (Mathf.PI * 2.0f) / m_RadialSegmentCount;

        for (int i = 0; i <= m_RadialSegmentCount; i++)
        {
            // use spline to render the points
            float angle = angleInc * i;

            // Finds the radial position wrt direction
            Vector3 right = Mathf.Sin(angle) * -binormal;
            Vector3 forward = Mathf.Cos(angle) * normal;
            Vector3 unitPosition = right + forward;

            // determines direction of binormal
            if (Vector3.Dot(splineTangent, tangent) < 0)
            {
                unitPosition -= 2 * right;
            }

            // mesh vertices
            meshBuilder.Vertices[ringIndex * (m_RadialSegmentCount + 1) + i] = centre + unitPosition * radius;
            meshBuilder.Normals[ringIndex * (m_RadialSegmentCount + 1) + i] = unitPosition;
        }
    }

    /// <summary>
    /// Adds a cap to the top or bottom of the cylinder.
    /// </summary>
    /// <param name="meshBuilder">The mesh builder currently being added to.</param>
    /// <param name="centre">The postion at the centre of the cap.</param>
    /// <param name="reverseDirection">Should the normal and winding order of the cap be reversed? (Should be true for bottom cap, false for the top)</param>
    private void BuildCap(MeshBuilder meshBuilder, Vector3 centre, Vector3 n, Vector3 bn, Vector3 t, float radius, bool reverseDirection)
    {
        //the normal will either be up or down:
        Vector3 normal = reverseDirection ? -t : t;

        //add one vertex in the center:
        meshBuilder.Vertices.Add(centre);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(new Vector2(0.5f, 0.5f));

        //store the index of the vertex we just added for later reference:
        int centreVertexIndex = meshBuilder.Vertices.Count - 1;

        //build the vertices around the edge:
        float angleInc = (Mathf.PI * 2.0f) / m_RadialSegmentCount;

        for (int i = 0; i <= m_RadialSegmentCount; i++)
        {
            float angle = angleInc * i;

            // Finds the radial position wrt direction
            Vector3 right = Mathf.Sin(angle) * bn;
            Vector3 forward = Mathf.Cos(angle) * n;
            Vector3 unitPosition = right + forward;

            // determines direction of binormal
/*            if (Vector3.Dot(splineTangent, normal) < 0)
            {
                unitPosition = -right + forward;
            }
*/
            meshBuilder.Vertices.Add(centre + unitPosition * radius);
            meshBuilder.Normals.Add(normal);

            Vector2 uv = new Vector2(unitPosition.x + 1.0f, unitPosition.z + 1.0f) * 0.5f;
            meshBuilder.UVs.Add(uv);

            //build a triangle:
            if (i > 0)
            {
                int baseIndex = meshBuilder.Vertices.Count - 1;

                if (reverseDirection)
                    meshBuilder.AddTriangle(centreVertexIndex, baseIndex - 1, baseIndex);
                else
                    meshBuilder.AddTriangle(centreVertexIndex, baseIndex, baseIndex - 1);
            }
        }

    }

    private void addCursorTransform()
    {
        knotNormals.Add(cursorTransform.up);
        knotBinormals.Add(cursorTransform.forward);
        knotTangents.Add(cursorTransform.right);
        knotRadius.Add(cursorTransform.GetComponent<ProcShape>().m_Radius);

        knotLengths.Add(spline.Length());
    }

    private void addCursorTransform(Vector3 up, Vector3 forward, Vector3 right)
    {
        knotNormals.Add(up);
        knotBinormals.Add(forward);
        knotTangents.Add(right);
        knotLengths.Add(spline.Length());

        knotRadius.Add(cursorTransform.GetComponent<ProcShape>().m_Radius);
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

    public void Init(GameObject Cursor)
    {
        if (Cursor == null)
        {
            Debug.Log("GameObject 'Shape' is not assigned!");
            Application.Quit();
        }

        // setups the script to track input cursor in 3D space
        cursorTransform = Cursor.transform;
        transform.position = new Vector3(0, 0, 0);

        // gets radial segments, radius and color
        ProcShape ps = Cursor.GetComponent<ProcShape>();
        var ph = Cursor.GetComponent<ProcHair>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (ps != null)
        {
            m_RadialSegmentCount = ps.m_RadialSegmentCount;
            m_Radius = ps.m_Radius;
            mr.material.color = ps.m_RGB;
        } else
        {
            m_RadialSegmentCount = ph.m_RadialSegmentCount;
            m_Radius = ph.radius;
            mr.material.color = ph.m_RGB;
        }
        

        //Get MeshRenderer and set to color of the cursor


        //Look for a MeshCollider component attached to this GameObject:
        meshCollider = GetComponent<MeshCollider>();

        //Create a new mesh builder:
        meshBuilder = new MeshBuilder();

        //Look for a MeshFilter component attached to this GameObject:
        filter = GetComponent<MeshFilter>();

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
            knotRadius = new List<float>();

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

        StartTracking();
    }

    public void StartTracking()
    {
        if (!isRunning)
        {
//            InvokeRepeating("BuildMeshTrail", 0, seconds);
            isRunning = true;
        }
    }

    public void CancelTracking()
    {
        if (isRunning)
        {
            CancelInvoke();

            if (knotLengths.Count >= 4)
            {
                List<Knot> knots = spline.knots;
                CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();

                // end cap
                splineTangent = spline.GetTangent(marker);
                BuildCap(meshBuilder, lastPosition, lastNormal, lastBinormal, lastTangent, lastRadius, true);
                BuildCap(meshBuilder, lastPosition, lastNormal, lastBinormal, lastTangent, lastRadius, false);

                // begin cap
                float beginDistance = knotLengths[3];
                int begin = (int)(beginDistance / trackLength);

                spline.PlaceMarker(marker, beginDistance);
                BuildCap(meshBuilder, firstPosition, firstNormal, firstBinormal, firstTangent, firstRadius, true);
                BuildCap(meshBuilder, firstPosition, firstNormal, firstBinormal, firstTangent, firstRadius, false);

                //If the MeshFilter exists, attach the new mesh to it.
                //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
                if (filter != null)
                {
                    Mesh mesh = meshBuilder.CreateMesh();

                    // center the mesh properly in 3D world
                    filter.transform.localPosition = filter.transform.TransformPoint(mesh.bounds.center);
                    var vertices = mesh.vertices;
                    for(int i=0; i<vertices.Length; ++i)
                    {
                        vertices[i] -= mesh.bounds.center;
                    }
                    mesh.vertices = vertices;
                    mesh.RecalculateBounds();

                    // assigns mesh
                    filter.sharedMesh = mesh;
                    meshCollider.inflateMesh = true;
                    meshCollider.sharedMesh = mesh;

                    //if (FindObjectOfType<EdgeTool>() != null || FindObjectOfType<FaceTool>() != null || FindObjectOfType<HandleTool>() != null) GetComponent<MeshEditor>().GenerateHandles();
                }

            }
            else Destroy(filter.gameObject);

            isRunning = false;
            first = true;
        }
    }
}
