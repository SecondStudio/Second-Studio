using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoftModel : MonoBehaviour {
    // mesh data for rendering and collision
    private MeshCollider meshCollider;
    private MeshBuilder meshBuilder;
    private MeshFilter filter;

    private List<List<Vector3>> lineList;
    private List<CatmullRomSpline> splineList;

    bool smooth = true;
    int numSections = 50; // num of division for linear interpolation
    float trackLength = 0.01f; // spline interpolation

    // Use this for initialization
    void Start () {
        
    }

    // Update is called once per frame
    void Update () {
        // instantiates and records a new mesh
        if (Input.GetKeyDown(KeyCode.Space))
        {
            smooth = !smooth;
            CreateMeshTrail();
        }
    }

    public void Loft(GameObject line1 , GameObject line2, GameObject line3)
    {
        lineList = new List<List<Vector3>>();
        var list1 = new List<Vector3>();
        var list2 = new List<Vector3>();
        var list3 = new List<Vector3>();
        var hair1 = line1.GetComponent<NURBSTrail>();
        var hair2 = line2.GetComponent<NURBSTrail>();
        var hair3 = line3.GetComponent<NURBSTrail>();

        if(hair1 == null || hair2 == null || hair3 == null)
        {
            print("One or more objects are not nurbs");
            return;
        }
        for (int i = 0; i < hair1.spline.knots.Count; i++)
        {
            list1.Add(hair1.spline.knots[i].position);
        }
        for (int i = 0; i < hair2.spline.knots.Count; i++)
        {
            list2.Add(hair2.spline.knots[i].position);
        }
        for (int i = 0; i < hair3.spline.knots.Count; i++)
        {
            list3.Add(hair3.spline.knots[i].position);
        }
        lineList.Add(list1);
        lineList.Add(list2);
        lineList.Add(list3);
        CreateMeshTrail();
        ObjectManager.instance.DestroyObject(line1);
        ObjectManager.instance.DestroyObject(line2);
        ObjectManager.instance.DestroyObject(line3);
        ObjectManager.instance.AddObject(gameObject);
    }
    public void Loft(List<Vector3> line1, List<Vector3> line2, List<Vector3> line3)
    {
        lineList = new List<List<Vector3>>();
        
        lineList.Add(line1);
        lineList.Add(line2);
        lineList.Add(line3);
        CreateMeshTrail();
        ObjectManager.instance.AddObject(gameObject);
    }
    public void CreateMeshTrail()
    {
        // creates a new mesh builder for generating mesh
        meshBuilder = new MeshBuilder();

		// find the max count of control points
		int max = 0;
		for(int i=0; i<lineList.Count; ++i)
		{
			if(max < lineList[i].Count)
				max = lineList[i].Count;
		}
		
		// redistribute points for lines with smaller number of control points
		for(int i=0; i<lineList.Count; ++i)
		{
			// if line does not have enough control points
			if(max > lineList[i].Count)
			{
                List<Vector3> l = new List<Vector3>();
				float frac1 = 0;
				float frac2 = 0;
                int index = 1;
				
				// add beginning point
				l.Add(lineList[i][0]);
				
				// interpolate between two points
                for (int j = 1; j < lineList[i].Count; ++j)
                {
					frac1 = (float) index / (max - 1);
					frac2 = (float) j / (lineList[i].Count - 1);
					
					// if the point is distributed between the 2 control point
					while(frac1 < frac2)
					{
						Vector3 diff = (lineList[i][j] - lineList[i][j - 1]);
						l.Add(lineList[i][j - 1] + diff * frac1);
						++index;
						frac1 = (float) index / (max - 1);
					}
					// endpoint of the 2 control point
					l.Add(lineList[i][j]);
					++index;						
                }
				lineList[i] = l;
            }
		}

		
        if (smooth) // create smooth sections
        {
            List<Vector3> listA;

            CatmullRomSpline spline;
            splineList = new List<CatmullRomSpline>();
            // generate the splines
            for (int j = 0; j < lineList[0].Count; ++j) // cycle the jth point of all lines
            {
                spline = new CatmullRomSpline();
                for (int i = 0; i < lineList.Count; ++i) // cycle through all the lines
                {
                    listA = lineList[i];

                    if(i == 0 || i == lineList.Count - 1)
                        for (int n = 0; n < 3; ++n)
                            spline.knots.Add(new Knot(listA[j]));
                    else spline.knots.Add(new Knot(listA[j]));
                }
                spline.Parametrize();
                splineList.Add(spline);
            }

            // find the maxlength for division
            float maxLength = -1;
            for (int i = 0; i < splineList.Count; ++i)
            {
                if(maxLength < splineList[i].Length())
                {
                    maxLength = splineList[i].Length();
                }
            }


            float trackLength = 0.01f;
            int maxCount = (int)(maxLength / trackLength);
            int total = 0;
            int offset = 1;
            CatmullRomSpline.Marker marker = new CatmullRomSpline.Marker();

            // Render the trail, spline by spline
            Vector3 posA;
            Vector3 posB;
            Vector3 tangentA;
            Vector3 tangentB;

            // NOTE: Creates SEPARATE quad spline
            // Quad 0: spline 0 and spline 1
            // Quad 1: spline 1 and spline 2
            // Quad 2: spline 2 and spline 3 .. etc
            for (int i=0; i<splineList.Count - 1; ++i)
            {
                CatmullRomSpline splineA = splineList[i];
                CatmullRomSpline splineB = splineList[i + 1];
                for (int j = offset; j < maxCount; ++j)
                {
                    // grab positions and tangents
                    splineA.PlaceMarker(marker, j * splineA.Length() / maxCount);
                    posA = splineA.GetPosition(marker);
                    tangentA = splineA.GetTangent(marker);

                    splineB.PlaceMarker(marker, j * splineB.Length() / maxCount);
                    posB = splineB.GetPosition(marker);
                    tangentB = splineB.GetTangent(marker);

                    // calculate the normals
                    Vector3 normalA = Vector3.Cross((posA - posB), tangentA).normalized;
                    Vector3 normalB = Vector3.Cross(tangentB, (posB - posA)).normalized;

                    // build the splines
                    BuildQuadSpline(meshBuilder, posA, posB, normalA, normalB, total, false);
                }

                // update offset for rendering separate quad splines
                total = meshBuilder.Vertices.Count;
            }
        }
        else // create smooth sections
        {
            List<Vector3> listA;
            List<Vector3> listB;

            int total = 0;
            for (int j = 0; j < lineList[0].Count - 1; ++j)
            {
                for (int i = 0; i < lineList.Count - 1; ++i)
                {
                    listA = lineList[i];
                    listB = lineList[i + 1];
                    BuildQuad(meshBuilder, listA[j], listB[j], listA[j + 1], listB[j + 1], total);
                }
                total = meshBuilder.Vertices.Count;
            }
        }

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
    // builds the shape
    // build quads for spline
    protected void BuildQuadSpline(MeshBuilder meshBuilder, Vector3 pointA, Vector3 pointB, Vector3 normalA, Vector3 normalB, int total, bool reverse)
    {
        // vertices are arranged in this order by point #
        // (0, 2) O---O (1, 3)

//        if (reverse)
//            normal *= -1;

        meshBuilder.Vertices.Add(pointA);
        meshBuilder.Normals.Add(normalA);

        meshBuilder.Vertices.Add(pointB);
        meshBuilder.Normals.Add(normalB);

        meshBuilder.Vertices.Add(pointA);
        meshBuilder.Normals.Add(-normalA);

        meshBuilder.Vertices.Add(pointB);
        meshBuilder.Normals.Add(-normalB);


        int baseIndex = meshBuilder.Vertices.Count - 8;

        // check if it has enough data to render the quads
        if(baseIndex - total >= 0)
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
    // builds the shape using the 4 points
    protected void BuildQuad(MeshBuilder meshBuilder, Vector3 point0, Vector3 point1, Vector3 point2, Vector3 point3, int total)
    {
        Vector3 pointA;
        Vector3 pointB;
        Vector3 tangentA = (point1 - point0).normalized;
        Vector3 tangentB = (point3 - point2).normalized;

        bool reverse = false;
        for (int i = 0; i <= numSections; ++i)
        {
            pointA = Vector3.Lerp(point0, point1, i / (float)numSections);
            pointB = Vector3.Lerp(point2, point3, i / (float)numSections);

            Vector3 normalA = Vector3.Cross(tangentA, pointB - pointA);
            Vector3 normalB = Vector3.Cross(tangentB, pointB - pointA);

            meshBuilder.Vertices.Add(pointA);
            meshBuilder.Normals.Add(normalA);

            meshBuilder.Vertices.Add(pointB);
            meshBuilder.Normals.Add(normalB);

            meshBuilder.Vertices.Add(pointA);
            meshBuilder.Normals.Add(-normalA);

            meshBuilder.Vertices.Add(pointB);
            meshBuilder.Normals.Add(-normalB);

            int baseIndex = meshBuilder.Vertices.Count - 8;

            if (baseIndex - total >= 0)
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
    }

    
}
