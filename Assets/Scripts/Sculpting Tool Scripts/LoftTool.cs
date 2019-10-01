using UnityEngine;
using System.Collections.Generic;

public class LoftTool : ToolBase
{
    Collider otherCollider;
    GameObject firstObject , secondObject , thirdObject;
    List<Vector3> firstCurve, secondCurve, thirdCurve;
    Mesh firstMesh;
    public GameObject ModelPrefab;
    MeshRenderer mr;
    Color color;
    bool AwaitingSelection = false;
    Vector3[] LineVertices1;
    Vector3[] LineVertices2;

    private float timeUpdate = 0.03f;
    float timeLeft = 0.0f;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        trackerLetter = "F";
        mr = GetComponent<MeshRenderer>();
        firstCurve = secondCurve = thirdCurve = null;
        color = mr.material.color;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!photonView.isMine || AwaitingSelection)
            return;

        if (timeLeft > 0)
            timeLeft -= Time.deltaTime;

        if ((controller.triggerButtonDown || Input.GetKeyDown(KeyCode.A)))
        {
            photonView.RPC("StartLoft", PhotonTargets.AllBufferedViaServer);
        }

        if (controller.dpadAxis.x < -0.2 && timeLeft <= 0)
        {
            if (transform.localScale.x > 0.05)
            {
                photonView.RPC("ScaleDown", PhotonTargets.AllBufferedViaServer);
                timeLeft = timeUpdate;
            }
        }

        else if (controller.dpadAxis.x > 0.2 && timeLeft <= 0)
        {
            if (transform.localScale.x < 0.2)
            {
                photonView.RPC("ScaleUp", PhotonTargets.AllBufferedViaServer);
                timeLeft = timeUpdate;
            }
        }

    }

    [PunRPC]
    void StartLoft()
    {
        print("Starting Loft");
        AwaitingSelection = true;
        foreach (var s in FindObjectsOfType<SelectionTool>())
        {
            s.OnFinishedSelection.AddListener(GetSelection);
        }
    }

    void DoLoft()
    {
        GameObject model = Instantiate(ModelPrefab);
        model.GetComponent<LoftModel>().Loft(firstObject, secondObject, thirdObject);
    }
    void DoLoft(List<Vector3> set1 , List<Vector3> set2, List<Vector3> set3)
    {
        GameObject model = Instantiate(ModelPrefab);
        model.GetComponent<LoftModel>().Loft(set1, set2, set3);
        Reset();
    }

    void GetSelection(List<MeshEditor.MeshElement> Selection)
    {
        try
        {
            if (firstCurve == null)
            {
                try
                {
                    print("Adding first edge set");
                    var firstSet = new List<MeshEditor.Edge>();
                    foreach (var e in Selection)
                    {
                        firstSet.Add(e as MeshEditor.Edge);
                    }
                    firstCurve = MakeLine(firstSet);
                } catch(System.Exception e)
                {
                    print(e.Message);
                }
            }
            else if (secondCurve == null)
            {
                try
                {
                    print("Adding first edge set");
                    var secondSet = new List<MeshEditor.Edge>();
                    foreach (var e in Selection)
                    {
                        secondSet.Add(e as MeshEditor.Edge);
                    }
                    secondCurve = MakeLine(secondSet);
                }
                catch (System.Exception e)
                {
                    print(e.Message);
                }
            }
            else if (thirdCurve == null)
            {
                try
                {
                    print("Adding first edge set");
                    var thirdSet = new List<MeshEditor.Edge>();
                    foreach (var e in Selection)
                    {
                        thirdSet.Add(e as MeshEditor.Edge);
                    }
                    thirdCurve = MakeLine(thirdSet);
                }
                catch (System.Exception e)
                {
                    print(e.Message);
                }
                DoLoft(firstCurve, secondCurve, thirdCurve);
            }
        }
        catch (System.Exception e)
        {
            print(e.Message);
            Reset();
        }
        
    }


    private void Reset()
    {
        firstCurve = secondCurve = thirdCurve = null;
        foreach (var s in FindObjectsOfType<SelectionTool>())
        {
            s.OnFinishedSelection.RemoveListener(GetSelection);
        }
        AwaitingSelection = false;
    }
    [PunRPC]
    public void ScaleUp()
    {
        transform.localScale += new Vector3(0.002f, 0.002f, 0.002f);
    }

    [PunRPC]
    public void ScaleDown()
    {
        transform.localScale -= new Vector3(0.002f, 0.002f, 0.002f);
    }

    // detects the object and distort it when button is inputted
    void OnTriggerEnter(Collider other)
    {
        if (!photonView.isMine)
            return;

        if (other.tag == "Trail")
        {
            Debug.Log("Entered");
            otherCollider = other;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!photonView.isMine)
            return;

        if (other.tag == "Trail")
        {
            Debug.Log("Exited");
            otherCollider = null;
        }
    }

    // finds the line vertices for a mesh
    Vector3[] findNearestLineVertices(Vector3[] vertices, Vector3 otherPosition)
    {
        Vector3[] lineVertices = new Vector3[2];

        float shortestDistance = 1000;
        float shortestDistance2 = shortestDistance;

        Debug.Log(otherPosition);


        // grabs the nearest two vertices: aka nearest line of mesh
        for (int i = 0; i < vertices.Length; ++i)
        {
            // gets mesh vertices position
            Vector3 position = vertices[i] + otherPosition;

            // gets points based shortest distance
            float distance = (position - transform.position).magnitude;
            if (distance < shortestDistance)
            {
                shortestDistance2 = shortestDistance;
                shortestDistance = distance;
                lineVertices[1] = lineVertices[0];
                lineVertices[0] = position;
            }
            else if (distance < shortestDistance2 && lineVertices[0] != position)
            {
                lineVertices[1] = position;
                shortestDistance2 = distance;
            }
        }

        return lineVertices;
    }

    // adds the connecting surface
    void addQuad(MeshBuilder meshBuilder)
    {
        Vector3 pointA = LineVertices1[0];
        Vector3 pointB = LineVertices1[1];
        Vector3 pointC = LineVertices2[1];
        Vector3 pointD = LineVertices2[0];

        // corrects points if flipped
        if (Vector3.Dot(pointB - pointA, pointC - pointD) < 0)
        {
            Vector3 sub = pointD;
            pointD = pointC;
            pointC = sub;
        }

        Vector3 normal = Vector3.Cross(pointA - pointB, pointA - pointC).normalized;

        // adds the connecting vertices
        for (int i = 0; i < 2; ++i)
        {
            meshBuilder.Vertices.Add(pointA);
            meshBuilder.Normals.Add(normal);
            meshBuilder.UVs.Add(new Vector2(0, 0));

            meshBuilder.Vertices.Add(pointB);
            meshBuilder.Normals.Add(normal);
            meshBuilder.UVs.Add(new Vector2(0, 0));

            meshBuilder.Vertices.Add(pointC);
            meshBuilder.Normals.Add(normal);
            meshBuilder.UVs.Add(new Vector2(0, 0));

            meshBuilder.Vertices.Add(pointD);
            meshBuilder.Normals.Add(normal);
            meshBuilder.UVs.Add(new Vector2(0, 0));

            normal = -normal;
        }

        int baseVertex = meshBuilder.Vertices.Count - 4;
        meshBuilder.AddTriangle(baseVertex + 2, baseVertex + 1, baseVertex);
        meshBuilder.AddTriangle(baseVertex, baseVertex + 3, baseVertex + 2);

        baseVertex = meshBuilder.Vertices.Count - 8;

        meshBuilder.AddTriangle(baseVertex, baseVertex + 1, baseVertex + 2);
        meshBuilder.AddTriangle(baseVertex + 2, baseVertex + 3, baseVertex);

        // creates the mesh and recalculates bounds and normals
        AssignMesh(meshBuilder.CreateMesh());
    }

    void AssignMesh(Mesh newMesh)
    {

        // gets the mesh from the object to alter
        MeshFilter mf = firstObject.GetComponent<MeshFilter>();
        MeshCollider mc = firstObject.GetComponent<MeshCollider>();

        mf.sharedMesh = newMesh;
        mc.sharedMesh = newMesh;

    }
    List<Vector3> MakeLine(List<MeshEditor.Edge> Edges)
    {
        List<Vector3> LinePoints = new List<Vector3>();
        Dictionary<Vector3, int> Appearances = new Dictionary<Vector3, int>();

        foreach (var e in Edges)
        {
            foreach (var g in e.Groups)
            {
                if (g == null) continue;
                if (Appearances.ContainsKey(g.center))
                {
                    Appearances[g.center]++;
                }
                else
                {
                    Appearances[g.center] = 1;
                }
            }
        }

        Vector3 StartPoint = Vector3.zero, Endpoint = Vector3.zero, CurrentPoint = Vector3.zero, LastPoint = Vector3.zero;
        bool HasStart = false, HasEnd = false;
        foreach (var a in Appearances)
        {
            if (a.Value == 1)
            {
                if (!HasStart)
                {
                    StartPoint = a.Key;
                    CurrentPoint = a.Key;
                    LinePoints.Add(CurrentPoint);
                    HasStart = true;
                }
                else if (!HasEnd)
                {
                    Endpoint = a.Key;
                    HasEnd = true;
                }
                else
                {
                    throw new System.Exception("Provided selection has more than two endpoints, not a continuous line");
                }
            }
        }
        if (!(HasStart && HasEnd)) throw new System.Exception("Provided selection dosen't have two endpoints , something probably went wrong, or the selection is empty");

        while (CurrentPoint != Endpoint)
        {
            bool found = false;
            foreach (var e in Edges)
            {
                if (e.Groups[0].center == CurrentPoint && e.Groups[1].center != LastPoint)
                {
                    LastPoint = CurrentPoint;
                    CurrentPoint = e.Groups[1].center;
                    LinePoints.Add(CurrentPoint);
                    found = true;
                    break;
                }
                else if (e.Groups[1].center == CurrentPoint && e.Groups[0].center != LastPoint)
                {
                    LastPoint = CurrentPoint;
                    CurrentPoint = e.Groups[0].center;
                    LinePoints.Add(CurrentPoint);
                    found = true;
                    break;
                }
            }
            if (!found) throw new System.Exception("Could not find matching vert for a non endpoint.");
        }
        return LinePoints;
    }
}
