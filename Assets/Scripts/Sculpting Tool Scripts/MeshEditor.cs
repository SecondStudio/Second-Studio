using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MeshEditor : MonoBehaviour
{
    public Face[] Faces;
    public List<Edge> Edges;
    public List<GameObject> Tris;
    public MeshFilter Filter;
    List<VertexGroup> UpdatedVerts;
    Dictionary<Vector3, VertexGroup> VertexGroups;
    public GameObject HandlePrefab;
    float minVertDistance = 0.01f;
    bool HasGroups = false;
    bool IsShared = false;
    void Start()
    {
        enabled = false;
        Filter = GetComponent<MeshFilter>();
        UpdatedVerts = new List<VertexGroup>();
        VertexGroups = new Dictionary<Vector3, VertexGroup>();
        Tris = new List<GameObject>();
        UpdateFaces();
    }

    void Update()
    {
        if(UpdatedVerts.Count != 0)
        {
            UpdateMesh();
        }
    }

    private void OnEnable()
    {
        Filter = GetComponent<MeshFilter>();
        //if (!HasGroups) GenerateVertexGroups();
    }
    private void OnDisable()
    {
    }

    /// <summary>
    /// Gets the vertex group closest to the specified (Local Space) position.
    /// </summary>
    /// <param name="inVector">The point to start looking from (Local Position)</param>
    /// <returns></returns>
    public VertexGroup GetVertexGroup(Vector3 inVector)
    {
        VertexGroup g = null;
        float dist = float.MaxValue;

        foreach(var group in VertexGroups.Values)
        {
            if(dist > Vector3.Distance(inVector , group.LocalPosition))
            {
                g = group;
                dist = Vector3.Distance(inVector, group.LocalPosition);
            }
        }
        if(dist < minVertDistance)
        {
            return g;
        }
        return null;
        
    }

    /// <summary>
    /// Wraper method to start Asyc group generation
    /// </summary>
    public void StartGroupGeneration() { StartCoroutine(GenerateVertexGroups()); }

    /// <summary>
    /// Generates group objects to represent groups of unshared verticies. The mesh editing system dosen't
    /// interact with verticies by themselves, but rather with these wrapper objects, so they are nesesarry for all mesh editing operations.
    /// The proccess is relativelty slow (probably could be sped up) so this method is Async so this can happen in the background. If you need the vertex groups
    /// immediately for some reason, just use the GenerateVertexGroupsNow method
    /// </summary>
    /// <returns></returns>
    public IEnumerator GenerateVertexGroups()
    {
        if (!Filter) Start();
        if (VertexGroups == null) VertexGroups = new Dictionary<Vector3, VertexGroup>();
        VertexGroups.Clear();
        for (int i = 0; i < Filter.mesh.vertexCount; i++)
        {
            var v = Filter.mesh.vertices[i];
            Vertex newVertex = new Vertex();
            newVertex.Index = i;
            newVertex.Body = transform;
            newVertex.LocalPosition = v;
            newVertex.Editor = this;
            foreach (var k in VertexGroups.Keys)
            {
                if (Vector3.Distance(k, v) < minVertDistance) v = k;
            }
            var g = GetVertexGroup(v);
            if (g != null)
            {
                g.Verts.Add(newVertex);
            }
            else
            {
                g = new VertexGroup();
                g.Verts = new List<Vertex>();
                g.Verts.Add(newVertex);
                g.Editor = this;
                VertexGroups.Add(v, g);
            }
            if (i % 10 == 0) yield return null;
        }
        HasGroups = true;
        if (!IsShared) ConvertToSharedVerts(false);
        else UpdateFaces();
    }
    public void GenerateVertexGroupsNow()
    {
        if (!Filter) Start();
        if (VertexGroups == null) VertexGroups = new Dictionary<Vector3, VertexGroup>();
        VertexGroups.Clear();
        for (int i = 0; i < Filter.mesh.vertexCount; i++)
        {
            var v = Filter.mesh.vertices[i];
            Vertex newVertex = new Vertex();
            newVertex.Index = i;
            newVertex.Body = transform;
            newVertex.LocalPosition = v;
            newVertex.Editor = this;

            VertexGroup g = null;
            if (VertexGroups.ContainsKey(v)){
                g = VertexGroups[v];
            } else {
               g = GetVertexGroup(v);
            }
            if (g != null)
            {
                g.Verts.Add(newVertex);
            }
            else
            {
                g = new VertexGroup();
                g.Verts = new List<Vertex>();
                g.Editor = this;
                g.Verts.Add(newVertex);
                VertexGroups.Add(v, g);
            }
        }
        HasGroups = true;
        if (!IsShared) ConvertToSharedVerts(false);
        else UpdateFaces();
    }
    /// <summary>
    /// Gets the vertex group closest to the specified (World Space) position.
    /// </summary>
    /// <param name="inVector">The point to start looking from (World Position)</param>
    /// <returns></returns>
    public VertexGroup GetClosestVertex(Vector3 worldPosition)
    {
        if (VertexGroups.Count == 0)
        {
            GenerateVertexGroupsNow();
        }
        float distance = float.MaxValue;
        VertexGroup vert = null;
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        foreach (var g in VertexGroups)
        {
            if (Vector3.Distance(localPosition, g.Value.LocalPosition) < distance)
            {
                vert = g.Value;
                distance = Vector3.Distance(localPosition, g.Value.LocalPosition);
            }
        }
        return vert;
    }
    /// <summary>
    /// Gets the closest face.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns></returns>
    public Face GetClosestFace(Vector3 position)
    {
        if (!HasGroups) Debug.LogError("Groups haven't generated yet!");
        UpdateFaces();
        var Filter = GetComponent<MeshFilter>();
        VertexGroup firstVert = GetClosestVertex(position);
        float dist = float.MaxValue;
        Vector3 v1, v2, v3;
        Face closestFace = Faces[0];
        var mesh = Filter.mesh;
        foreach (var f in Faces)
        {

            if (f.Contains(firstVert.LocalPosition) && Vector3.Distance(transform.TransformPoint(f.center), position) < dist)
            {
                dist = Vector3.Distance(transform.TransformPoint(f.center), position);
                closestFace = f;
            }
        }
        return closestFace;
    }
    /// <summary>
    /// Extrudes the closest face.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>The newly formed face from the extrude</returns>
    public VertexGroup[] ExtrudeClosestFace(Vector3 position)
    {
        if (!HasGroups) Debug.LogError("Groups haven't generated yet!");
        UpdateFaces();
        var Filter = GetComponent<MeshFilter>();
        VertexGroup firstVert = GetClosestVertex(position);
        float dist = float.MaxValue;
        Vector3 v1, v2, v3;
        Face closestFace = Faces[0];
        var mesh = Filter.mesh;
        foreach (var f in Faces)
        {

            if (f.Contains(firstVert.LocalPosition) && Vector3.Distance(transform.TransformPoint(f.center), position) < dist)
            {
                dist = Vector3.Distance(transform.TransformPoint(f.center), position);
                closestFace = f;
            }
        }

        v1 = closestFace.points[0];
        v2 = closestFace.points[1];
        v3 = closestFace.points[2];
        VertexGroup[] Verts = new VertexGroup[3];
        Verts[0] = GetVertexGroup(v1);
        Verts[1] = GetVertexGroup(v2);
        Verts[2] = GetVertexGroup(v3);



        VertexGroup[] NewVerts = new VertexGroup[3];
        NewVerts[0] = new VertexGroup(Verts[0]);
        NewVerts[1] = new VertexGroup(Verts[1]);
        NewVerts[2] = new VertexGroup(Verts[2]);

        var tris = new int[mesh.triangles.Length + 21];
        var verts = new Vector3[mesh.vertices.Length + 3];
        int count = 0;
        for (int i = 0; i < mesh.triangles.Length; i++) tris[i] = mesh.triangles[i];
        for (int i = 0; i < mesh.vertices.Length; i++) verts[i] = mesh.vertices[i];
        foreach (var g in NewVerts)
        {
            foreach (var v in g.Verts)
            {

            }
            g.Verts[0].Index = mesh.vertices.Length + count;
            count++;
            verts[g.Verts[0].Index] = g.Verts[0].LocalPosition;
        }


        for (int i = 0; i < 3; i++)
        {
            tris[mesh.triangles.Length + 6 * i] = Verts[i].Verts[0].Index;
            tris[mesh.triangles.Length + 1 + 6 * i] = Verts[(i + 1) % 3].Verts[0].Index;
            tris[mesh.triangles.Length + 2 + 6 * i] = NewVerts[i].Verts[0].Index;
            tris[mesh.triangles.Length + 3 + 6 * i] = NewVerts[i].Verts[0].Index;
            tris[mesh.triangles.Length + 4 + 6 * i] = Verts[(i + 1) % 3].Verts[0].Index;
            tris[mesh.triangles.Length + 5 + 6 * i] = NewVerts[(i + 1) % 3].Verts[0].Index;
        }


        tris[mesh.triangles.Length + 18] = NewVerts[0].Verts[0].Index;
        tris[mesh.triangles.Length + 19] = NewVerts[1].Verts[0].Index;
        tris[mesh.triangles.Length + 20] = NewVerts[2].Verts[0].Index;

        var uvs = new Vector2[mesh.uv.Length + 3];
        for (int i = 0; i < mesh.uv.Length + 3; i++) { uvs[i] = new Vector2(0, 0); } //This is hacky AF you should build new UVs

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return NewVerts;

    }
    /// <summary>
    /// Extrudes the face.
    /// </summary>
    /// <param name="toExtrude">To extrude.</param>
    /// <returns></returns>
    public VertexGroup[] ExtrudeFace(Face toExtrude)
    {
        if (!HasGroups) Debug.LogError("Groups haven't generated yet!");
        UpdateFaces();
        var Filter = GetComponent<MeshFilter>();
        var mesh = Filter.mesh;

        VertexGroup[] Verts = new VertexGroup[3];
        Verts[0] = toExtrude.Groups[0];
        Verts[1] = toExtrude.Groups[1];
        Verts[2] = toExtrude.Groups[2];



        VertexGroup[] NewVerts = new VertexGroup[3];
        NewVerts[0] = new VertexGroup(Verts[0]);
        NewVerts[1] = new VertexGroup(Verts[1]);
        NewVerts[2] = new VertexGroup(Verts[2]);

        var tris = new int[mesh.triangles.Length + 21];
        var verts = new Vector3[mesh.vertices.Length + 3];
        int count = 0;
        for (int i = 0; i < mesh.triangles.Length; i++) tris[i] = mesh.triangles[i];
        for (int i = 0; i < mesh.vertices.Length; i++) verts[i] = mesh.vertices[i];
        foreach (var g in NewVerts)
        {
            foreach (var v in g.Verts)
            {

            }
            g.Verts[0].Index = mesh.vertices.Length + count;
            count++;
            verts[g.Verts[0].Index] = g.Verts[0].LocalPosition;
        }


        for (int i = 0; i < 3; i++)
        {
            tris[mesh.triangles.Length + 6 * i] = Verts[i].Verts[0].Index;
            tris[mesh.triangles.Length + 1 + 6 * i] = Verts[(i + 1) % 3].Verts[0].Index;
            tris[mesh.triangles.Length + 2 + 6 * i] = NewVerts[i].Verts[0].Index;
            tris[mesh.triangles.Length + 3 + 6 * i] = NewVerts[i].Verts[0].Index;
            tris[mesh.triangles.Length + 4 + 6 * i] = Verts[(i + 1) % 3].Verts[0].Index;
            tris[mesh.triangles.Length + 5 + 6 * i] = NewVerts[(i + 1) % 3].Verts[0].Index;
        }


        tris[mesh.triangles.Length + 18] = NewVerts[0].Verts[0].Index;
        tris[mesh.triangles.Length + 19] = NewVerts[1].Verts[0].Index;
        tris[mesh.triangles.Length + 20] = NewVerts[2].Verts[0].Index;

        var uvs = new Vector2[mesh.uv.Length + 3];
        for (int i = 0; i < mesh.uv.Length + 3; i++) { uvs[i] = new Vector2(0, 0); } //This is hacky AF you should build new UVs

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return NewVerts;

    }
    /// <summary>
    /// Gets the closest edge.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns></returns>
    public Edge GetClosestEdge(Vector3 position)
    {
        if (!HasGroups) Debug.LogError("Groups haven't generated yet!");
        UpdateFaces();
        var Filter = GetComponent<MeshFilter>();
        VertexGroup firstHandle = GetClosestVertex(position);
        float dist = float.MaxValue;
        Vector3 v1, v2;
        Edge closestEdge = Edges[0];
        var mesh = Filter.mesh;
        foreach (var e in Edges)
        {

            if (e.Contains(firstHandle.LocalPosition) && Vector3.Distance(transform.TransformPoint(e.center), position) < dist)
            {
                dist = Vector3.Distance(transform.TransformPoint(e.center), position);
                closestEdge = e;
            }
        }
        return closestEdge;
    }
    /// <summary>
    /// Updates the mesh renderer and mesh filter. Needs to be called after verts are edited so that the changes will show.
    /// </summary>
    public void UpdateMesh()
    {

        var mesh = GetComponent<MeshFilter>();
        var meshData = mesh.sharedMesh;

        var positions = meshData.vertices;
        foreach (var g in UpdatedVerts)
        {
            VertexGroups[g.LocalPosition] =  g;
            foreach (var v in g.Verts)
            {
                positions[v.Index] = v.LocalPosition;
            }
        }
        UpdatedVerts.Clear();
        //       Debug.Log(positions.Length);
        var col = GetComponent<MeshCollider>();
        mesh.sharedMesh = meshData;
        col.sharedMesh = meshData;
        GetComponent<ObjectID>().OutlineRenderer.gameObject.GetComponent<MeshFilter>().mesh = meshData;
        meshData.vertices = positions;
        meshData.RecalculateNormals();
    }
    /// <summary>
    /// Adds the vertex to the list of verts to be updated next time UpdateMesh is called
    /// </summary>
    /// <param name="v">The v.</param>
    public void UpdateVertex(VertexGroup v)
    {
        UpdatedVerts.Add(v);
    }
    /// <summary>
    /// Updates the face struct data for this mesh -> Automatically calls update edges
    /// </summary>
    public void UpdateFaces()
    {
        var Filter = GetComponent<MeshFilter>();
        Faces = new Face[Filter.mesh.triangles.Length / 3];
        for (int i = 0; i < GetComponent<MeshFilter>().mesh.triangles.Length; i += 3)
        {
            Faces[i / 3] = new Face(new Vector3[] { Filter.mesh.vertices[Filter.mesh.triangles[i]], Filter.mesh.vertices[Filter.mesh.triangles[i + 1]], Filter.mesh.vertices[Filter.mesh.triangles[i + 2]] }, i / 3, this);
        }
        UpdateEdges();
    }

    public class MeshElement
    {
        public Vector3[] points;
        public Vector3 center;
        public MeshEditor Editor;
    }
    public class Face : MeshElement
    {
        public Face(Vector3[] inPoints, int Index , MeshEditor editor) {
            points = inPoints;
            TriIndex = Index;
            Editor = editor;
            center = (points[0] + points[1] + points[2]) / 3; 
            distances = new Vector3[] { points[0] - center, points[1] - center, points[2] - center };
            Groups = new VertexGroup[] { Editor.GetVertexGroup(points[0]), Editor.GetVertexGroup(points[1]), Editor.GetVertexGroup(points[2])};
        }
        public Vector3[] distances;
        public VertexGroup[] Groups;
        public int TriIndex;
        public GameObject Handle;
        public bool Contains(Vector3 vertex)
        {
            foreach(var p in points)
            {
                if (p == vertex) return true;
            }
            return false;
        }
        public void Scale(float amt)
        {
            for(int i = 0; i < 3; i++)
            {
                Groups[i].LocalPosition = center + distances[i] * amt;
                distances[i] = points[i] - center;
                Editor.UpdatedVerts.Add(Groups[i]);
            }
            Editor.UpdateMesh();
        }
        public static bool operator ==(Face lhs , Face rhs)
        {
            if (Object.ReferenceEquals(lhs,null) || Object.ReferenceEquals(rhs, null)) return false;
            
            return lhs.TriIndex == rhs.TriIndex && lhs.Editor == rhs.Editor;
        }
        public static bool operator !=(Face lhs, Face rhs)
        {
            return !(lhs == rhs);
        }
    }
    public class Edge : MeshElement
    {
        public Vector3[] distances;
        public int EdgeIndex;
        public VertexGroup[] Groups;

        public Edge(Vector3[] inPoints, int Index, MeshEditor editor)
        {
            points = inPoints;
            Editor = editor;
            EdgeIndex = Index;
            center = (points[0] + points[1]) / 2; 
            distances = new Vector3[] { points[0] - center, points[1] - center};
            Groups = new VertexGroup[] { Editor.GetVertexGroup(points[0]), Editor.GetVertexGroup(points[1]) };
        }

        public bool Contains(Vector3 vertex)
        {
            foreach (var p in points)
            {
                if (p == vertex) return true;
            }
            return false;
        }
        public void Scale(float amt)
        {
            for (int i = 0; i < 2; i++)
            {
                Groups[i].LocalPosition = center + distances[i] * amt;
                distances[i] = points[i] - center;
                Editor.UpdatedVerts.Add(Groups[i]);
            }
            Editor.UpdateMesh();
        }
        public static bool operator ==(Edge lhs, Edge rhs)
        {
            if (Object.ReferenceEquals(lhs, null) || Object.ReferenceEquals(rhs, null)) return false;

            return lhs.EdgeIndex == rhs.EdgeIndex && lhs.Editor == rhs.Editor;
        }
        public static bool operator !=(Edge lhs, Edge rhs)
        {
            return !(lhs == rhs);
        }
    }
    public class Vertex
    {
        public int Index;
        public Transform Body;
        public Vector3 LocalPosition;
        public Vector3 WorldPosition { get { return Body.TransformPoint(LocalPosition); } set { LocalPosition = Body.InverseTransformPoint(value); } }
        public MeshEditor Editor;
        List<Vertex> SharedVerts;
    }
    public class VertexGroup : MeshElement
    {
        public VertexGroup() { Verts = new List<Vertex>(); }
        public VertexGroup(VertexGroup other)
        {
            Verts = new List<Vertex>();

            foreach(var v in other.Verts)
            {
                var nv = new Vertex();
                nv.Body = v.Body;
                nv.Editor = v.Editor;
                nv.WorldPosition = v.WorldPosition;
                Verts.Add(nv);
            }
            WorldPosition = other.WorldPosition;
            center = WorldPosition;
            Editor = other.Editor;
        }
        public List<Vertex> Verts;
        public Vector3 WorldPosition { get { return Verts[0].WorldPosition; } set { foreach (var v in Verts) { v.WorldPosition = value; } } }
        public Vector3 LocalPosition { get { return Verts[0].LocalPosition; } set { foreach (var v in Verts) { v.LocalPosition = value; } } }
        }
    private void UpdateEdges()
    {
        if (Edges != null)
        {
            Edges.Clear();
        }
        else
        {
            Edges = new List<Edge>();
        }
        var Filter = GetComponent<MeshFilter>();
        int i = 0;
        foreach (Face f in Faces)
        {
            Edges.Add(new Edge(new Vector3[] { f.points[0], f.points[1] } , i++ , this));
            Edges.Add(new Edge(new Vector3[] { f.points[0], f.points[2] }, i++, this));
            Edges.Add(new Edge(new Vector3[] { f.points[1], f.points[2] }, i++, this));
        }
    }
    private int GetTriToExtrude(VertexGroup[] Verts)
    {
        Dictionary<int, int> TriCount = new Dictionary<int, int>();
        int TriIndex = 0;
        foreach( var g in Verts)
        {
            foreach(var v in g.Verts)
            {
                int triIndex = v.Index / 3;
                if (TriCount.ContainsKey(triIndex))
                {
                    TriCount[triIndex]++;
                } else
                {
                    TriCount.Add(triIndex , 1);
                }
            }
        }
        foreach(var i in TriCount)
        {
            if (i.Value == 3) TriIndex = i.Key;
        }

        return TriIndex;
        
    }
    /// <summary>
    /// Converts the mesh to a shared vertex mesh by combining verts that are closer than minVertDistance apart. Also can combine resultant verts
    /// from a multi extrude into one vert
    /// </summary>
    /// <param name="GenerateVerts">if set to <c>true</c> [generate verts].</param>
    public void ConvertToSharedVerts(bool GenerateVerts)
    {
        if (GenerateVerts) GenerateVertexGroupsNow();
        var Mesh = GetComponent<MeshFilter>().mesh;
        int lastCount = Mesh.vertexCount;
        Dictionary<int, int> NewIndicies = new Dictionary<int, int>();
        Dictionary<int, int> Aliases = new Dictionary<int, int>();

        for(int i = 0; i < Mesh.vertexCount; i++)
        {
            NewIndicies[i] = i;
            Aliases[i] = i;
        }

        var vals = VertexGroups.Values;
        foreach(var g in vals)
        {
            for(int i = 1; i < g.Verts.Count; i++)
            {
                Aliases[g.Verts[i].Index] = g.Verts[0].Index;
                NewIndicies[g.Verts[i].Index] = -1;
                var keys = NewIndicies.Keys;
                for (int j = 0; j < NewIndicies.Count; j++)
                {
                    if (j >= g.Verts[i].Index) NewIndicies[j]--;
                }
            }
        }


        var verts = new Vector3[VertexGroups.Count];

        for(int i = 0; i < Mesh.vertices.Length; i++)
        {
            if(NewIndicies[i] >=0)
            {
                if (NewIndicies[i] == 8 && verts.Length == 8) NewIndicies[i] = 0; // this is digusting and I'm sorry
                verts[NewIndicies[i]] = Mesh.vertices[i];
            }
        }

        var tris = Mesh.triangles;
        for(int i = 0; i < Mesh.triangles.Length; i++)
        {
            tris[i] = NewIndicies[Aliases[tris[i]]];
        }

        Mesh.triangles = tris;
        Mesh.vertices = verts;
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();
        Mesh.RecalculateTangents();
        GetComponent<MeshFilter>().mesh = Mesh;
        UpdateFaces();
        IsShared = true;
        print("Reduced mesh from " + lastCount + " --> " + Mesh.vertexCount + " verts!");
        StartGroupGeneration();
    }
    
    protected void DrawFace(VertexGroup[] handles , int TriIndex)
    {
        var points = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            points[i] = handles[i].WorldPosition;
        }
        if (Tris == null) Tris = new List<GameObject>();
        var tri = new GameObject();
        var renderer = tri.AddComponent<MeshRenderer>();
        var filter = tri.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        mesh.vertices = points;
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new int[] { 0, 1, 2 };
        filter.mesh = mesh;
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.cyan;
        tri.transform.parent = transform;
        Tris.Add(tri);
        
        

    }
}
