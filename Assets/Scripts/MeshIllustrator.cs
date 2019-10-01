using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshIllustrator : MonoBehaviour {

    [SerializeField]
    public List<EdgeTool> EdgeTools;
    public Material EdgeMaterial;
    Dictionary<GameObject , Material> PrevMats;
    public static MeshIllustrator Instance;
    public bool DrawSelection = true;
    public void Start()
    {
        PrevMats = new Dictionary<GameObject, Material>();
        if (!Instance) Instance = this;
    }
    private void OnPostRender()
    {
        List<MeshEditor> toDraw = new List<MeshEditor>();
        foreach(var controller in FindObjectsOfType<WandController>())
        {
            MeshEditor closest = null;
            float dist = float.MaxValue;
            foreach (var m in FindObjectsOfType<MeshEditor>())
            {
                if (m.gameObject.GetComponent<ObjectID>())
                    m.gameObject.GetComponent<ObjectID>().OutlineRenderer.enabled = false;
                var d = Vector3.Distance(controller.transform.position, m.transform.position);
                if(d < dist)
                {
                    dist = d;
                    closest = m;
                }
            }
            if(closest != null && !toDraw.Contains(closest))
            {
                toDraw.Add(closest);
            }
        }
        
            foreach (var m in toDraw)
        {
            try
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.green;
                Transform trans = m.transform;
                if(m.GetComponent<ObjectID>())
                m.GetComponent<ObjectID>().OutlineRenderer.enabled = true;
                GL.PushMatrix();
                mat.SetPass(0);
                GL.Begin(GL.TRIANGLES);
                switch (ModeSelector.CurrentMode)
                {
                    case ModeSelector.EditMode.VERTEX:
                        // Set your materials

                        foreach (var v in m.GetComponent<MeshFilter>().mesh.vertices)
                        {
                            GL.Vertex(trans.TransformPoint(v + (1 / trans.lossyScale.x) * new Vector3(0, 0.015f, 0)));
                            GL.Vertex(trans.TransformPoint(v + (1 / trans.lossyScale.x) * new Vector3(-.006f, -0.004f, 0)));
                            GL.Vertex(trans.TransformPoint(v + (1 / trans.lossyScale.x) * new Vector3(.006f, -0.004f, 0)));
                        }
                        break;
                    case ModeSelector.EditMode.EDGE:
                        // Set your materials
                        if (m.Edges == null || m.Edges.Count == 0 || trans == null) continue;
                        foreach (var e in m.Edges)
                        {
                            GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(0, 0.015f, 0)));
                            GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(-.006f, -0.004f, 0)));
                            GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(.006f, -0.004f, 0)));
                        }
                        break;
                    case ModeSelector.EditMode.FACE:
                        if(m.GetComponent<ObjectID>())
                        m.GetComponent<ObjectID>().OutlineRenderer.material = FindObjectOfType<FaceTool>().OutlineMaterial;
                        if (m.Faces == null || m.Faces.Length == 0 || trans == null) continue;
                        foreach (var f in m.Faces)
                        {
                            GL.Vertex(trans.TransformPoint(f.center + (1 / trans.lossyScale.x) * new Vector3(0, 0.015f, 0)));
                            GL.Vertex(trans.TransformPoint(f.center + (1 / trans.lossyScale.x) * new Vector3(-.006f, -0.004f, 0)));
                            GL.Vertex(trans.TransformPoint(f.center + (1 / trans.lossyScale.x) * new Vector3(.006f, -0.004f, 0)));
                        }
                        break;
                    case ModeSelector.EditMode.OBJECT:

                        break;
                }
            }
            
            finally {
                GL.End();
                GL.PopMatrix();
            }
        }

        if (!DrawSelection) return;
        foreach (var e in FindObjectOfType<SelectionTool>().Selection)
        {
            try
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.red;
                GL.PushMatrix();
                mat.SetPass(0);
                GL.Begin(GL.TRIANGLES);
                Transform trans = e.Editor.transform;
                if (e is MeshEditor.Edge)
                {                    
                    GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(0, 0.015f, 0)));
                    GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(-.009f, -0.006f, 0)));
                    GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(.009f, -0.006f, 0)));
                }
                else if (e is MeshEditor.VertexGroup)
                {
                    GL.Vertex((e as MeshEditor.VertexGroup).WorldPosition + (1 / 1.5f * trans.lossyScale.x) * new Vector3(0, 0.015f, 0));
                    GL.Vertex((e as MeshEditor.VertexGroup).WorldPosition + (1 / 1.5f * trans.lossyScale.x) * new Vector3(-.009f, -0.006f, 0));
                    GL.Vertex((e as MeshEditor.VertexGroup).WorldPosition + (1 / 1.5f * trans.lossyScale.x) * new Vector3(.009f, -0.006f, 0));
                }
                else if (e is MeshEditor.Face)
                {
                    GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(0, 0.015f, 0)));
                    GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(-.009f, -0.006f, 0)));
                    GL.Vertex(trans.TransformPoint(e.center + (1 / trans.lossyScale.x) * new Vector3(.009f, -0.006f, 0)));
                }
            }

            finally
            {
                GL.End();
                GL.PopMatrix();
            }
        }
    }
}
