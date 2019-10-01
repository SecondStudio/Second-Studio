using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// drawing any hairs will call nurbs trail, which will call catmullromspline to create hairs with data
/// </summary>
public class NURBSTrail : MonoBehaviour
{
    // mesh data for rendering and collision
    private MeshCollider meshCollider;
    private MeshBuilder meshBuilder;
    private MeshFilter filter;

    public bool isQuad = false;

    public float bnValue = 0.01f;
    public float nValue = 0f;

    public CatmullRomSpline spline;
    CatmullRomSpline splineSide;
    private float trackLength = 0.05f;

    private Vector3 dir = Vector3.zero;

    Vector3 initPos;
    bool splineSet = false;

    private List<float> knotLengths;

    // Use this for initialization
    void Start()
    {

    }

    public void InitNURBS(CatmullRomSpline spline, float trackLength, Vector3 dir)
    {
        this.spline = spline;
        this.trackLength = trackLength;
        filter = GetComponent<MeshFilter>();

        CreateMeshTrail(dir, 0, Vector3.one);
        BuildMesh();
    }

    public void Init(Vector3 pos)
    {
        // setups the script to track input cursor in 3D space
        transform.position = new Vector3(0, 0, 0);

        //Look for a MeshCollider component attached to this GameObject:
        meshCollider = GetComponent<MeshCollider>();
        if (spline == null) spline = new CatmullRomSpline();
        //Create a new mesh builder:
        meshBuilder = new MeshBuilder();

        //Look for a MeshFilter component attached to this GameObject:
        filter = GetComponent<MeshFilter>();

        // creates the spline
        if (splineSide == null)
        {
            splineSide = new CatmullRomSpline();
            knotLengths = new List<float>();

            // add knots via position
            List<Knot> knots = splineSide.knots;
            Vector3 point = Vector3.zero;

            for (int i = 0; i < 5; ++i)
            {
                knots.Add(new Knot(point));
            }

            for (int i = 0; i < 4; ++i)
            {
                knotLengths.Add(0);
            }
        }
    }

    public void SetInitPos(Vector3 pos)
    {
        initPos = pos;
    }

    // changes the direction of the quad/volume
    public void UpdateQuadDir(Vector3 pos, bool curve)
    {
        if (curve)
        {
            if (!splineSet)
            {
                if (splineSide == null)
                    Init(dir);
                else BuildMeshTrail(pos - initPos);
            }
        }
        else
            CreateMeshTrail(dir + (pos - initPos), nValue, Vector3.one);
    }

    public void SetQuadDir(Vector3 pos, bool curve)
    {
        if (curve)
        {
            splineSet = true;
        }
        else
        {
            dir += (pos - initPos);
            CreateMeshTrail(dir, nValue, Vector3.one);
        }
        BuildMesh();
    }

    // changes the height of the volume
    public void UpdateVolumeDir(Vector3 pos)
    {
        if(splineSet)
            CreateMeshTrail(nValue + Vector3.Distance(pos,initPos));
        else
            CreateMeshTrail(dir, nValue + Vector3.Distance(pos, initPos), Vector3.Scale(Vector3.one, pos - initPos));
    }

    public void SetVolumeDir(Vector3 pos)
    {
        nValue += Vector3.Distance(pos, initPos);
        if (splineSet)
            CreateMeshTrail(nValue);
        else
            CreateMeshTrail(dir, nValue, Vector3.Scale(Vector3.one, pos - initPos));

        BuildMesh();
    }



    void BuildMesh()
    {
        //If the MeshFilter exists, attach the new mesh to it.
        //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
        Mesh mesh = meshBuilder.CreateMesh();

        filter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        filter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void BuildMeshTrail(Vector3 pos)
    {
        // Checks and update the spline path for cursor
        List<Knot> knots = splineSide.knots;
        Vector3 point = pos;

        knots[knots.Count - 1].position = point;
        knots[knots.Count - 2].position = point;

        if (Vector3.Distance(knots[knots.Count - 3].position, point) > trackLength &&
            Vector3.Distance(knots[knots.Count - 4].position, point) > trackLength) // point exceeds length path travelled
        {

            // add knot and the normals at the point
            knots.Add(new Knot(point));
            splineSide.Parametrize();
            knotLengths.Add(splineSide.Length());
            knotLengths.RemoveAt(0);

            CreateMeshTrail(0); // update mesh
        }
    }

    // create mesh for LINEAR extrusion
    public void CreateMeshTrail(Vector3 bn, float nValue, Vector3 normPos)
    {
        // throws out previous mesh if nValue is set
        List<Knot> knots = spline.knots;

        CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();

        meshBuilder = new MeshBuilder();
        Vector3 position = new Vector3(0, 0, 0);

        // makes points based on the distance segments
        int begin = 0;
        int end = (int)(spline.Length() / trackLength);

        int offset = 5;
        int totalPoints = end - begin - offset;

        totalPoints *= 2;

        // IMPORTANT!!! : DEPENDS UPON THE CONSTRAINTS
        // calculates the normal in order to determine the render direction
        // place the marker at spline to get distance

        bool reverse = true;

        // Restarts rendering if a volume is being generated
        
        if (nValue <= -0.01f || nValue >= 0.01f)
        {
            meshBuilder = new MeshBuilder();

            spline.PlaceMarker(marker, trackLength * offset);

            Vector3 diffSide = new Vector3(bn.x, 0, 0);

            print("Normal: " + normPos);
            print("Tangent: " + spline.GetTangent(marker));
            print("Side: " + diffSide);
            print("Cross: " + Vector3.Cross(spline.GetTangent(marker), normPos));

            reverse = !(Vector3.Dot(Vector3.Cross(spline.GetTangent(marker), normPos), diffSide) > 0);

            if (reverse)
                print("REVERSE!");
            else print("FORWARD!");
        }
        
        Vector3 sidePos = Vector3.zero;

        // iterate through the entire path of the spline
        for (int j = 0; j < 2; ++j)
        {
            if (j == 1)
                sidePos = bn;

            for (int i = begin + offset; i < end; ++i)
            {
                // place the marker at spline to get distance
                spline.PlaceMarker(marker, trackLength * i);

                // get the variable needed for the point on spline
                position = spline.GetPosition(marker) + sidePos;

                Vector3 tangent = spline.GetTangent(marker);

                Vector3 normal = normPos;

                // build or update mesh
                if (nValue > -0.01f && nValue < 0.01f)
                {
                    if (j >= meshBuilder.Vertices.Count / totalPoints)
                        BuildQuadSpline(meshBuilder, position, normal, totalPoints, Vector3.zero, Vector3.zero, 0, reverse);
                    else
                    {
                        UpdateQuadSpline(meshBuilder, position, normal, totalPoints, Vector3.zero, Vector3.zero, i - offset, 0, reverse);
                    }
                }
                else
                {
                    Vector3 startPos = Vector3.zero;
                    Vector3 endPos = normPos;

                    if (j >= meshBuilder.Vertices.Count / totalPoints)
                        BuildQuadSpline(meshBuilder, position, normal, totalPoints, startPos, endPos, 0, reverse);
                    else
                    {
                        UpdateQuadSpline(meshBuilder, position, normal, totalPoints, startPos, endPos, i - offset, 0, reverse);
                    }
                }
            }
        }
        
        // detect if there is a structure or not
        if (nValue <= -0.01f || nValue >= 0.01f)
        {
            int p = meshBuilder.Vertices.Count;

            // build sides out of the spline
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = bn;

            for (int j = 0; j < 2; ++j)
            {
                for (int i = begin + offset; i < end; ++i)
                {
                    // place the marker at spline to get distance
                    spline.PlaceMarker(marker, trackLength * i);

                    // get the variable needed for the point on spline
                    position = spline.GetPosition(marker);
                    if (j == 1)
                        position += normPos;

                    Vector3 tangent = spline.GetTangent(marker);

                    Vector3 normal = Vector3.Cross(tangent, normPos).normalized;

                    BuildQuadSpline(meshBuilder, position, normal, totalPoints, endPos, startPos, p, reverse);
                }
            }
            
            p = meshBuilder.Vertices.Count;

            // build caps out of the side spline
            spline.PlaceMarker(marker, trackLength * offset);
            startPos = spline.GetPosition(marker);
            Vector3 startTangent = spline.GetTangent(marker);

            if (nValue < 0)
                startTangent *= -1;

            spline.PlaceMarker(marker, trackLength * (end - 1));
            endPos = spline.GetPosition(marker);

            BuildQuadSpline(meshBuilder, Vector3.zero, startTangent, 4, startPos, endPos, p, reverse);
            BuildQuadSpline(meshBuilder, bn, startTangent, 4, startPos, endPos, p, reverse);

            BuildQuadSpline(meshBuilder, normPos, startTangent, 4, startPos, endPos, p, reverse);
            BuildQuadSpline(meshBuilder, normPos + bn, startTangent, 4, startPos, endPos, p, reverse);            
        }
        
        filter.sharedMesh = meshBuilder.CreateMesh();
    }


    // create mesh for curved extrusion
    public void CreateMeshTrail(float nValue)
    {
        // throws out previous mesh if nValue is set
        List<Knot> knots = spline.knots;

        CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();
        CatmullRomSpline.Marker markerSide = new CatmullRomSpline.Marker();

        int maxPoint = meshBuilder.Vertices.Count;
        Vector3 position = new Vector3(0, 0, 0);
        Vector3 sidePos = new Vector3(0, 0, 0);
        Vector3 normPos = new Vector3(0, 0, 1f) * nValue;

        // makes points based on the distance segments
        int begin = 0;
        int end = (int)(spline.Length() / trackLength);

        int beginSide = (int)(knotLengths[0] / trackLength);
        int endSide = (int)(splineSide.Length() / trackLength);

        int offset = 5;
        int totalPoints = end - begin - offset;
        int totalPointsSide = (int)(splineSide.Length() / trackLength) - begin - offset;

        totalPoints *= 2;
        totalPointsSide *= 2;

        // IMPORTANT!!! : DEPENDS UPON THE CONSTRAINTS
        // calculates the normal in order to determine the render direction
        // place the marker at spline to get distance

        bool reverse = true;

        // Restarts rendering if a volume is being generated
        if (nValue <= -0.01f || nValue >= 0.01f)
        {
            beginSide = 0;
            meshBuilder = new MeshBuilder();

            spline.PlaceMarker(marker, trackLength * offset);

            splineSide.PlaceMarker(markerSide, 0);
            Vector3 diffSide = splineSide.GetPosition(markerSide);
            splineSide.PlaceMarker(markerSide, trackLength * (endSide - 1));
            diffSide -= splineSide.GetPosition(markerSide);
            diffSide = new Vector3(diffSide.x, 0, 0);

            print("Normal: " + normPos);
            print("Tangent: " + spline.GetTangent(marker));
            print("Side: " + diffSide);
            print("Cross: " + Vector3.Cross(spline.GetTangent(marker), normPos));

            reverse = (Vector3.Dot(Vector3.Cross(spline.GetTangent(marker), normPos), diffSide) > 0);

            if (reverse)
                print("REVERSE!");
            else print("FORWARD!");
        }

        beginSide = (offset > beginSide) ? offset : beginSide;

        // iterate through the entire path of the spline
        for (int j = beginSide; j < endSide; ++j)
        {
            // place the marker at spline to get distance
            splineSide.PlaceMarker(marker, trackLength * j);

            sidePos = splineSide.GetPosition(marker);

            Vector3 sideTangent = splineSide.GetTangent(marker);

            for (int i = begin + offset; i < end; ++i)
            {
                // place the marker at spline to get distance
                spline.PlaceMarker(marker, trackLength * i);

                // get the variable needed for the point on spline
                position = spline.GetPosition(marker) + sidePos;

                Vector3 tangent = spline.GetTangent(marker);

                Vector3 normal = normPos.normalized;

                // build or update mesh
                if (nValue > -0.01f && nValue < 0.01f)
                {
                    if ((j - offset) >= meshBuilder.Vertices.Count / totalPoints)
                        BuildQuadSpline(meshBuilder, position, normal, totalPoints, Vector3.zero, Vector3.zero, 0, reverse);
                    else
                    {
                        UpdateQuadSpline(meshBuilder, position, normal, totalPoints, Vector3.zero, Vector3.zero, i - offset, j - offset, reverse);
                    }
                }
                else
                {
                    Vector3 startPos = Vector3.zero;
                    Vector3 endPos = normPos;

                    if ((j - offset) >= meshBuilder.Vertices.Count / totalPoints)
                        BuildQuadSpline(meshBuilder, position, normal, totalPoints, startPos, endPos, 0, reverse);
                    else
                    {
                        UpdateQuadSpline(meshBuilder, position, normal, totalPoints, startPos, endPos, i - offset, j - offset, reverse);
                    }
                }
            }
        }

        // detect if there is a structure or not
        if (nValue <= -0.01f || nValue >= 0.01f)
        {
            int p = meshBuilder.Vertices.Count;

            // build sides out of the spline
            splineSide.PlaceMarker(marker, trackLength * offset);
            Vector3 startPos = splineSide.GetPosition(marker);
            splineSide.PlaceMarker(marker, trackLength * (endSide - 1));
            Vector3 endPos = splineSide.GetPosition(marker);
            
            for (int j = 0; j < 2; ++j)
            {
                for (int i = begin + offset; i < end; ++i)
                {
                    // place the marker at spline to get distance
                    spline.PlaceMarker(marker, trackLength * i);

                    // get the variable needed for the point on spline
                    position = spline.GetPosition(marker);
                    if (j == 1)
                        position += normPos;

                    Vector3 tangent = spline.GetTangent(marker);

                    Vector3 normal = Vector3.Cross(tangent, normPos).normalized;

                    BuildQuadSpline(meshBuilder, position, normal, totalPoints, endPos, startPos,  p, reverse);
                }
            }

            p = meshBuilder.Vertices.Count;

            // build caps out of the side spline
            spline.PlaceMarker(marker, trackLength * offset);
            startPos = spline.GetPosition(marker);
            spline.PlaceMarker(marker, trackLength * (end - 1));
            endPos = spline.GetPosition(marker); 

            for (int j = 0; j < 2; ++j)
            {
                for (int i = begin + offset; i < endSide; ++i)
                {
                    // place the marker at spline to get distance
                    splineSide.PlaceMarker(marker, trackLength * i);

                    // get the variable needed for the point on spline
                    position = splineSide.GetPosition(marker);
                    if (j == 1)
                        position += normPos;

                    Vector3 tangent = splineSide.GetTangent(marker);

                    Vector3 normal = Vector3.Cross(tangent, normPos).normalized;

                    BuildQuadSpline(meshBuilder, position, normal, totalPointsSide, startPos, endPos,  p, reverse);
                }
            }
        }

        filter.sharedMesh = meshBuilder.CreateMesh();
    }

    // build quads for spline
    protected void BuildQuadSpline(MeshBuilder meshBuilder, Vector3 point, Vector3 normal, int total, Vector3 startOffset, Vector3 endOffset, int pointOffset, bool reverse)
    {
        // vertices are arranged in this order by point #
        //   (0, 1) O---O (2, 3)
        // (n, n+1) O---O (n+2, n+3)
        if (reverse)
            normal *= -1;

        meshBuilder.Vertices.Add(point + startOffset);
        meshBuilder.Normals.Add(normal);

        meshBuilder.Vertices.Add(point + endOffset);
        meshBuilder.Normals.Add(-normal);

        int baseIndex = meshBuilder.Vertices.Count - total - 4;

        if (baseIndex - pointOffset >= 0 && ((baseIndex - pointOffset + 2) % total) != 0)
        {
            if (!reverse)
            {
                meshBuilder.AddTriangle(baseIndex, baseIndex + 2, baseIndex + total);
                meshBuilder.AddTriangle(baseIndex + total + 2, baseIndex + total, baseIndex + 2);
                meshBuilder.AddTriangle(baseIndex + total + 1, baseIndex + 3, baseIndex + 1);
                meshBuilder.AddTriangle(baseIndex + 3, baseIndex + total + 1, baseIndex + total + 3);
            }
            else
            {
                meshBuilder.AddTriangle(baseIndex + total, baseIndex + 2, baseIndex);
                meshBuilder.AddTriangle(baseIndex + 2, baseIndex + total, baseIndex + total + 2);
                meshBuilder.AddTriangle(baseIndex + +1, baseIndex + 3, baseIndex + total + 1);
                meshBuilder.AddTriangle(baseIndex + total + 3, baseIndex + total + 1, baseIndex + 3);
            }
        }
    }

    protected void UpdateQuadSpline(MeshBuilder meshBuilder, Vector3 point, Vector3 normal, int total, Vector3 startOffset, Vector3 endOffset, int i, int j, bool reverse)
    {
        // vertices are arranged in this order by point #
        //   (0, 1) O---O (2, 3)
        // (n, n+1) O---O (n+2, n+3)
        if (reverse)
            normal *= -1;

        meshBuilder.Vertices[(2 * i) + j * total] = (point + startOffset);
        meshBuilder.Normals[(2 * i) + j * total] = normal;

        meshBuilder.Vertices[(2 * i) + 1 + j * total] = (point + endOffset);
        meshBuilder.Normals[(2 * i) + 1 + j * total] = -normal;
    }

    // builds the shape
    protected void BuildStructure(MeshBuilder meshBuilder, Vector3 point, Vector3 binormal, Vector3 normal, float nValue, bool reverse)
    {
        // vertices are arranged in this order by point #
        // (7, 0) O---O (1, 2)
        //        |   |
        // (6, 5) O---O (3, 4)
        Vector2 uv = new Vector2(0f, 0f);
        Vector3 offsetXZ = bnValue * binormal.normalized;
        Vector3 offsetY = nValue * normal.normalized;

        if (nValue < 0)
            normal *= -1;

        meshBuilder.Vertices.Add(point + offsetY);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point + offsetXZ + offsetY);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point + offsetXZ + offsetY);
        meshBuilder.Normals.Add(binormal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point + offsetXZ);
        meshBuilder.Normals.Add(binormal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point + offsetXZ);
        meshBuilder.Normals.Add(-normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point);
        meshBuilder.Normals.Add(-normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point);
        meshBuilder.Normals.Add(-binormal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point + offsetY);
        meshBuilder.Normals.Add(-binormal);
        meshBuilder.UVs.Add(uv);

        int baseIndex = meshBuilder.Vertices.Count - 16;

        // if there are at least two set of points, generate the mesh
        if (baseIndex >= 0)
        {
            for (int i = 0; i < 4; ++i)
            {
                int index = baseIndex + 2 * i;
                if (nValue > 0)
                {
                    meshBuilder.AddTriangle(index, index + 8, index + 1);
                    meshBuilder.AddTriangle(index + 8, index + 9, index + 1);
                }
                else
                {
                    meshBuilder.AddTriangle(index + 1, index + 8, index);
                    meshBuilder.AddTriangle(index + 1, index + 9, index + 8);
                }
            }
        }

    }

    // builds the shape
    protected void UpdateStructure(MeshBuilder meshBuilder, Vector3 point, Vector3 binormal, Vector3 normal, bool reverse, int i)
    {
        // vertices are arranged in this order by point #
        // (7, 0) O---O (1, 2)
        //        |   |
        // (6, 5) O---O (3, 4)
        Vector3 offsetXZ = bnValue * binormal.normalized;
        Vector3 offsetY = nValue * normal.normalized;

        int baseIndex = i * 8;

        meshBuilder.Vertices[baseIndex] = point + offsetY;
        meshBuilder.Normals[baseIndex] = normal;

        meshBuilder.Vertices[baseIndex + 1] = point + offsetXZ + offsetY;
        meshBuilder.Normals[baseIndex + 1] = normal;

        meshBuilder.Vertices[baseIndex + 2] = point + offsetXZ + offsetY;
        meshBuilder.Normals[baseIndex + 2] = binormal;

        meshBuilder.Vertices[baseIndex + 3] = point + offsetXZ;
        meshBuilder.Normals[baseIndex + 3] = binormal;

        meshBuilder.Vertices[baseIndex + 4] = point + offsetXZ;
        meshBuilder.Normals[baseIndex + 4] = -normal;

        meshBuilder.Vertices[baseIndex + 5] = point;
        meshBuilder.Normals[baseIndex + 5] = -normal;

        meshBuilder.Vertices[baseIndex + 6] = point;
        meshBuilder.Normals[baseIndex + 6] = -binormal;

        meshBuilder.Vertices[baseIndex + 7] = point + offsetY;
        meshBuilder.Normals[baseIndex + 7] = -binormal;
    }

    // builds the shape
    protected void BuildQuad(MeshBuilder meshBuilder, Vector3 point, Vector3 binormal, Vector3 normal, bool reverse)
    {
        // vertices are arranged in this order by point #
        // (0, 2) O---O (1, 3)
        if (reverse)
            normal *= -1;

        Vector2 uv = new Vector2(0f, 0f);
        Vector3 offset = bnValue * binormal.normalized;

        meshBuilder.Vertices.Add(point + offset);
        meshBuilder.Normals.Add(-normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point);
        meshBuilder.Normals.Add(-normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point + offset);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(uv);

        meshBuilder.Vertices.Add(point);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(uv);

        int baseIndex = meshBuilder.Vertices.Count - 8;

        if (baseIndex >= 0)
        {
            if (!reverse)
            {
                meshBuilder.AddTriangle(baseIndex + 4, baseIndex + 5, baseIndex);
                meshBuilder.AddTriangle(baseIndex + 5, baseIndex + 1, baseIndex);
                meshBuilder.AddTriangle(baseIndex + 2, baseIndex + 7, baseIndex + 6);
                meshBuilder.AddTriangle(baseIndex + 2, baseIndex + 3, baseIndex + 7);
            }
            else
            {
                meshBuilder.AddTriangle(baseIndex, baseIndex + 5, baseIndex + 4);
                meshBuilder.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 5);
                meshBuilder.AddTriangle(baseIndex + 6, baseIndex + 7, baseIndex + 2);
                meshBuilder.AddTriangle(baseIndex + 7, baseIndex + 3, baseIndex + 2);
            }
        }
    }

    // update previous quads
    protected void UpdateQuad(MeshBuilder meshBuilder, Vector3 point, Vector3 binormal, Vector3 normal, bool reverse, int i)
    {
        if (reverse)
            normal *= -1;

        // vertices are arranged in this order by point #
        // (0, 2) O---O (1, 3)

        Vector2 uv = new Vector2(0f, 0f);
        Vector3 offset = bnValue * binormal.normalized;

        int baseIndex = i * 4;

        meshBuilder.Vertices[baseIndex] = point + offset;
        meshBuilder.Normals[baseIndex] = -normal;

        meshBuilder.Vertices[baseIndex + 1] = point;
        meshBuilder.Normals[baseIndex + 1] = -normal;

        meshBuilder.Vertices[baseIndex + 2] = point + offset;
        meshBuilder.Normals[baseIndex + 2] = normal;

        meshBuilder.Vertices[baseIndex + 3] = point;
        meshBuilder.Normals[baseIndex + 3] = normal;
    }
}