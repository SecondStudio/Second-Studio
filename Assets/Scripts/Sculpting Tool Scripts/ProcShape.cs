using UnityEngine;
using System.Collections;

public class ProcShape : ProcBase
{
    public float m_Radius = 0.5f;

    public Color32 m_RGB = new Color32(255, 255, 255, 255);
    MeshRenderer mr;
    float startingRoll;
    // change this variable to change radius
    public float radius
    {
        get
        {
            return m_Radius;
        }
        set
        {
            if (value <= 0)
            {
                Debug.Log("Invalid Radius: " + radius);
                Application.Quit();
            }
            else
            {
                m_Radius = value;
                Init();
            }
        }
    }

    //the number of radial segments:
    public int m_RadialSegmentCount = 10;

    void Update()
    {

    }

    // change this variable to change radius
    public int radialSegmentCount
    {
        get
        {
            return m_RadialSegmentCount;
        }
        set
        {
            if (value <= 2)
            {
                Debug.Log("Invalid Radius: " + m_RadialSegmentCount);
                Application.Quit();
            }
            else {
                m_RadialSegmentCount = value;
                Init();
            }
        }
    }

    public Vector3 startPoint = new Vector3(0, 0, 0);
    public Vector3 endPoint = new Vector3(1, 0, 0);

    //the number of radial segments:
    public int m_Points = 10;


    //the number of length segments:
    public int m_LengthSegmentCount = 4;

    Transform reference;

    private void Start()
    {
        Init();
    }

    //Build the mesh:
    public override Mesh BuildMesh()
    {
        //Create a new mesh builder:
        MeshBuilder meshBuilder = new MeshBuilder();

        // creates empty transform
        GameObject go = new GameObject();
        reference = go.transform;


        Vector3[] controlPoints = { startPoint, endPoint };

        // generates the shape for every point
        for (int num = 0; num < controlPoints.Length - 1; ++num)
        {
            // find direction from startpoint to end point
            Vector3 dir = controlPoints[num + 1] - controlPoints[num];
            reference.right = dir.normalized;
            reference.rotation = Quaternion.AngleAxis(startingRoll, reference.right) * reference.rotation;

            //multi-segment cylinder:
            float lengthInc = (dir).magnitude / m_LengthSegmentCount;

            // generate shapes using rings
            for (int i = 0; i <= m_LengthSegmentCount; i++)
            {
                //centre position of this ring:
                Vector3 centrePos = controlPoints[num] + reference.transform.right * lengthInc * i;

                //V coordinate is based on height:
                float v = (float)i / m_LengthSegmentCount;

                BuildShape(meshBuilder, m_RadialSegmentCount, centrePos, radius, v, i > 0 || num > 0);
            }
        }

        // Caps for end of the solid
        reference.right = (controlPoints[1] - controlPoints[0]).normalized;
        reference.rotation = Quaternion.AngleAxis(startingRoll, reference.right) * reference.rotation;
        BuildCap(meshBuilder, controlPoints[0], true); // begin cap
        BuildCap(meshBuilder, controlPoints[controlPoints.Length - 1], false); // end cap

        // destroy reference gameobject
        Destroy(go);

        // set the mesh color
//        mr.material.color = m_RGB;

        return meshBuilder.CreateMesh();
    }

    // builds the shape based upon segmentCount
    protected void BuildShape(MeshBuilder meshBuilder, int segmentCount, Vector3 centre, float radius, float v, bool buildTriangles)
    {
        float angleInc = (Mathf.PI * 2.0f) / segmentCount;

        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = angleInc * i;

            // Finds the radial position wrt direction
            Vector3 right = Mathf.Sin(angle) * -reference.forward;
            Vector3 forward = Mathf.Cos(angle) * reference.up;
            Vector3 unitPosition = right + forward;


            meshBuilder.Vertices.Add(centre + unitPosition * radius);
            meshBuilder.Normals.Add(unitPosition);
            meshBuilder.UVs.Add(new Vector2((float)i / segmentCount, v));

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

    /// <summary>
    /// Adds a cap to the top or bottom of the cylinder.
    /// </summary>
    /// <param name="meshBuilder">The mesh builder currently being added to.</param>
    /// <param name="centre">The postion at the centre of the cap.</param>
    /// <param name="reverseDirection">Should the normal and winding order of the cap be reversed? (Should be true for bottom cap, false for the top)</param>
     protected void BuildCap(MeshBuilder meshBuilder, Vector3 centre, bool reverseDirection)
    {
        //the normal will either be up or down:
        Vector3 normal = reverseDirection ? -reference.right : reference.right;

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
            Vector3 right = Mathf.Sin(angle) * -reference.forward;
            Vector3 forward = Mathf.Cos(angle) * reference.up;
            Vector3 unitPosition = right + forward;

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

    public void ChangeColor(Color c)
    {
        m_RGB = c;
        if(mr != null)
            mr.material.color = c;
    }

    public void Init()
    {
        //Build the mesh:
        Mesh mesh = BuildMesh();

        //Look for a MeshFilter component attached to this GameObject:
        MeshFilter filter = GetComponent<MeshFilter>();

        // set color of mesh renderer
        mr = GetComponent<MeshRenderer>();
        if (mr != null)
            mr.material.color = m_RGB;

        //If the MeshFilter exists, attach the new mesh to it.
        //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }

    }

    public void ResetPoint(Vector3 pos,  float rotation)
    {
        startPoint = pos;
        endPoint = pos;
        startingRoll = rotation;
        Init();
    }

    public void SetEndPoint(Vector3 pos)
    {
        float distance = (endPoint - pos).magnitude;
        if (distance >= 0.005f)
        {
            endPoint = pos;
            Init();
        }
    }

    public void SetMeshCollider()
    {
        MeshCollider mc = GetComponent<MeshCollider>();
        if(mc != null)
        {
            Mesh mesh = BuildMesh();
            MeshFilter mf = GetComponent<MeshFilter>();

            // center the mesh properly in 3D world
            mf.transform.position = mf.transform.TransformPoint(mesh.bounds.center);

            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] -= mesh.bounds.center;
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;

            gameObject.tag = "Trail";
        }
        else
        {
            Debug.Log("Mesh collider is not attached!");
        }
    }

    public void ScaleShapeUp()
    {
        float newRadius = radius + 0.005f;
        if (newRadius <= 0.20f)
        {
            radius = newRadius;
            ToolTracker.net[2] = 1;
            ToolTracker.net[3] = 2;
            ToolTracker.value[0] = "f";
            ToolTracker.value[1] = "" + newRadius;
            ToolTracker.setEmpty(2);
            TrackerScript.AddAction();
        }
    }

    public void ScaleShapeDown()
    {
        float newRadius = radius - 0.005f;
        if (newRadius >= 0.02f)
        {
            radius = newRadius;
            ToolTracker.net[2] = 1;
            ToolTracker.net[3] = 3;
            ToolTracker.value[0] = "f";
            ToolTracker.value[1] = "" + newRadius;
            ToolTracker.setEmpty(2);
            TrackerScript.AddAction();
        }
    }
}
