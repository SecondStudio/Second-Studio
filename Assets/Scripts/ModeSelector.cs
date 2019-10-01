using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is here to switch the global "mode" of the scene between objects, verts, edges, and faces.
/// This mode determines how the selection tool, and move tool interact with objects
/// </summary>

public class ModeSelector : Photon.MonoBehaviour {



    int currentIndex;
    public GrabObjects objectTool;
    public FaceTool faceTool;
    public VertexTool vertexTool;
    public EdgeTool edgeTool;
    public ModeSelector other;
    MonoBehaviour[] Tools;
    MonoBehaviour ActiveTool;
    public enum EditMode { OBJECT, FACE, VERTEX, EDGE }
    public static EditMode CurrentMode{ get { return (EditMode)FindObjectOfType<ModeSelector>().currentIndex; } } 
    private void Start()
    {
        Tools = new MonoBehaviour[] { objectTool , faceTool , vertexTool, edgeTool };
        SetActiveTool(0);
        
    }

    public void Update()
    {
        if (faceTool.controller.appButtonDown) NextMode();
    }

    public void NextMode()
    {
        currentIndex++;
        if (currentIndex > 3) currentIndex = 0;
        SetActiveTool(currentIndex);
        if (FindObjectOfType<SelectionTool>())
            FindObjectOfType<SelectionTool>().Selection = new List<MeshEditor.MeshElement>();
        other.SetActiveTool(currentIndex);
    }

    public void PrevMode()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = 3;
        SetActiveTool(currentIndex);
        FindObjectOfType<SelectionTool>().Selection.Clear();
        other.SetActiveTool(currentIndex);
    }


    void SetActiveTool(int index)
    {
        if (ActiveTool) ActiveTool.enabled = false;
        currentIndex = index;
        if (Tools == null || Tools.Length == 0) return;
        Tools[index].enabled = true;
        ActiveTool = Tools[index];
    }
}
