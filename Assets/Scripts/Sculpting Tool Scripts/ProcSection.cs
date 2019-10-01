using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProcSection : ProcBase
{
    public enum ShapeType {T, I, BRACKET, LEFTCORNER, RIGHTCORNER};
    public ShapeType m_ShapeType = ShapeType.T;

    public Color32 m_RGB = new Color32(255, 255, 255, 255);
    MeshRenderer mr;
    float startingRoll;
    //the radius and height of the cylinder:
    public float m_Width = 0.5f;
    public float m_Height = 0.5f;
    public float m_Thickness = 0.1f;

    // change this variable to change width
    public float height
    {
        get
        {
            return m_Height;
        }
        set
        {
            if (value <= 0)
            {
                Debug.Log("Invalid Height: " + value);
                Application.Quit();
            }
            else {
                m_Height = value;
                Init();
            }
        }
    }

    // change this variable to change width
    public float width
    {
        get
        {
            return m_Width;
        }
        set
        {
            if (value <= 0)
            {
                Debug.Log("Invalid Width: " + value);
                Application.Quit();
            }
            else {
                m_Width = value;
                Init();
            }
        }
    }

    // change this variable to change thickness
    public float thickness
    {
        get
        {
            return m_Thickness;
        }
        set
        {
            if (value <= 0)
            {
                Debug.Log("Invalid Width: " + value);
                Application.Quit();
            }
            else {
                m_Thickness = value;
                Init();
            }
        }
    }

    public ShapeType shapeType
    {
        get
        {
            return m_ShapeType;
        }
        set
        {
            m_ShapeType = value;
            Init();
        }
    }


    public Vector3 startPoint = new Vector3(0, 0, 0);
    public Vector3 endPoint = new Vector3(1, 0, 0);

    //the number of length segments:
    public int m_LengthSegmentCount = 4;

    Transform reference;
    private Vector3[] cornerNormals;
    private Vector3[] shapePoints;
    private int[] normalsIndices;

    private void Start()
    {
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
            reference.forward = dir.normalized;
            reference.rotation = Quaternion.AngleAxis(startingRoll, reference.forward) * reference.rotation;
            // setup shape for specified section
            SetupShape();

            //multi-segment cylinder:
            float lengthInc = (dir).magnitude / m_LengthSegmentCount;

            // generate shapes using rings
            for (int i = 0; i <= m_LengthSegmentCount; i++)
            {
                //centre position of this ring:
                Vector3 centrePos = controlPoints[num] + reference.transform.forward * lengthInc * i;

                //V coordinate is based on height:
                float v = (float)i / m_LengthSegmentCount;

                BuildShape(meshBuilder, centrePos, v, i > 0 || num > 0);
            }
        }

        // Caps for end of the solid
        BuildCap(meshBuilder, controlPoints[0], false); // begin cap
        BuildCap(meshBuilder, controlPoints[1], true); // end cap

        // destroy reference gameobject
        Destroy(go);

        // sets the color of the object
//        mr.material.color = m_RGB;

        return meshBuilder.CreateMesh();
    }

    // setup shape based on "ShapeType"
    protected void SetupShape()
    {
        // corner normals
        Vector3[] normals =
        {
            -reference.right + reference.up, // 0: upper-left
            reference.right + reference.up, // 1: upper-right
            reference.right - reference.up, // 2: lower-right
            -reference.right - reference.up, // 3: lower-left
        };

        // normalize the vectors
        for (int i = 0; i < normals.Length; ++i)
            normals[i].Normalize();

        cornerNormals = normals;

        // Creates variables for getting points of letter
        Vector3 up = reference.up * m_Height / 2.0f;
        Vector3 h_thickness = reference.up * m_Thickness / 2.0f;

        Vector3 right = reference.right * m_Width / 2.0f;
        Vector3 w_thickness = reference.right * m_Thickness / 2.0f;

        if (m_ShapeType == ShapeType.I)
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
        else if (m_ShapeType == ShapeType.T)
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
        else if(m_ShapeType == ShapeType.BRACKET)
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
        else if(m_ShapeType == ShapeType.LEFTCORNER)
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
        else if (m_ShapeType == ShapeType.RIGHTCORNER)
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

    // builds the shape based upon segmentCount
    protected void BuildShape(MeshBuilder meshBuilder, Vector3 centre, float v, bool buildTriangles)
    {
        int segmentCount = shapePoints.Length - 1; // gets number of points
        for (int i = 0; i <= segmentCount; i++)
        {
            // Adds the vertices based upon shape wrt centre
            meshBuilder.Vertices.Add(centre + shapePoints[i]);

            // Adds normals at corners of shape
            meshBuilder.Normals.Add(cornerNormals[normalsIndices[i]]);

            // UV texture
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
    /// Adds a cap to the top or bottom of the section.
    /// </summary>
    /// <param name="meshBuilder">The mesh builder currently being added to.</param>
    /// <param name="centre">The postion at the centre of the cap.</param>
    /// <param name="reverseDirection">Should the normal and winding order of the cap be reversed? (Should be true for bottom cap, false for the top)</param>
    private void BuildCap(MeshBuilder meshBuilder, Vector3 centre, bool reverseDirection)
    {

        // determines how the cap will be formed based upon vertices
        if (m_ShapeType == ShapeType.T)
        {
            int[] quad = { 0, 1, 2, 7 };
            addQuad(meshBuilder, reverseDirection, centre, quad);

            int[] quad2 = { 3, 4, 5, 6 };
            addQuad(meshBuilder, reverseDirection, centre, quad2);
        }
        else if (m_ShapeType == ShapeType.I)
        {
            int[] quad = { 0, 1, 2, 11 };
            addQuad(meshBuilder, reverseDirection, centre, quad);

            int[] quad2 = { 3, 4, 9, 10 };
            addQuad(meshBuilder, reverseDirection, centre, quad2);

            int[] quad3 = { 5, 6, 7, 8 };
            addQuad(meshBuilder, reverseDirection, centre, quad3);
        }
        else if (m_ShapeType == ShapeType.BRACKET)
        {
            int[] quad = { 0, 1, 2, 3 };
            addQuad(meshBuilder, reverseDirection, centre, quad);

            int[] quad2 = { 0, 3, 4, 7 };
            addQuad(meshBuilder, reverseDirection, centre, quad2);

            int[] quad3 = { 4, 5, 6, 7 };
            addQuad(meshBuilder, reverseDirection, centre, quad3);
        }
        else if (m_ShapeType == ShapeType.LEFTCORNER)
        {
            int[] quad = { 0, 1, 2, 3 };
            addQuad(meshBuilder, reverseDirection, centre, quad);

            int[] quad2 = { 0, 3, 4, 5 };
            addQuad(meshBuilder, reverseDirection, centre, quad2);
        }
        else if (m_ShapeType == ShapeType.RIGHTCORNER)
        {
            int[] quad = { 0, 1, 4, 5 };
            addQuad(meshBuilder, reverseDirection, centre, quad);

            int[] quad2 = { 4, 1, 2, 3 };
            addQuad(meshBuilder, reverseDirection, centre, quad2);
        }

    }

    // adds quads for the vertices
    private void addQuad(MeshBuilder meshBuilder, bool reverseDirection, Vector3 centre, int[] vertices)
    {
        //the normal will either be up or down:
        Vector3 normal = reverseDirection ? reference.forward : -reference.forward;

        int num;

        for (int i = 0; i < 4; ++i)
        {
            if (reverseDirection)
                num = 3 - i;
            else num = i;

            meshBuilder.Vertices.Add(centre + shapePoints[vertices[num] ]);
            meshBuilder.Normals.Add(normal);
            Vector2 uv = new Vector2(shapePoints[vertices[num] ].x + 1.0f, shapePoints[vertices[num] ].y + 1.0f) * 0.5f;
            meshBuilder.UVs.Add(uv);
        }

        int baseIndex = meshBuilder.Vertices.Count - 4;

        meshBuilder.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
        meshBuilder.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
    }

    public void ChangeColor(Color c)
    {
        m_RGB = c;
        if (mr != null)
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

    public void ScaleShapeUp()
    {
        float newThickness = thickness + 0.005f / 3f;

        if (newThickness < 0.20f)
        {
            m_Width += 0.005f;
            m_Height += 0.005f;
            thickness = newThickness;
        }
    }

    public void ScaleShapeDown()
    {
        float newThickness = thickness - 0.005f / 3f;
        if (newThickness > 0.01f)
        {
            m_Width -= 0.005f;
            m_Height -= 0.005f;
            thickness = newThickness;
        }
    }


    public void setWidth(float width)
    {
        if (width <= 0)
        {
            Debug.Log("Invalid Radius: " + width);
            Application.Quit();
        }
        else {
            m_Width = width;
            Init();
        }
    }

    public void setHeight(float height)
    {
        if (height <= 0)
        {
            Debug.Log("Invalid Radius: " + height);
            Application.Quit();
        }
        else {
            m_Height = height;
            Init();
        }
    }


    public void ResetPoint(Vector3  pos, float rotation)
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
        if (mc != null)
        {
            Mesh mesh = BuildMesh();
            MeshFilter mf = GetComponent<MeshFilter>();

            // center the mesh properly in 3D world
            mr.transform.position = mr.transform.TransformPoint(mesh.bounds.center);
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
    }
}
