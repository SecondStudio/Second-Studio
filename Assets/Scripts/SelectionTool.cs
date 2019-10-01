using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SelectionTool : ToolBase {

    // Use this for initialization

    public ModeSelector.EditMode Mode { get { return ModeSelector.CurrentMode; } }
    public List<MeshEditor.MeshElement> Selection;
    public SelectionEvent OnFinishedSelection;

    protected GameObject CurrentObject;
    protected MeshEditor CurrentEditor { get { return CurrentObject.GetComponent<MeshEditor>(); } }
    public Transform Cursor;
    protected override void Start() {
        base.Start();
        OnFinishedSelection = new SelectionEvent();
        Selection = new List<MeshEditor.MeshElement>();
    }

    public SelectionTool()
    {
        Selection = new List<MeshEditor.MeshElement>();
    }
    protected override void Update() {
        base.Update();

        if (controller.triggerButtonDown)
        {
            photonView.RPC("StartSelection", PhotonTargets.AllBufferedViaServer, Cursor.position);
        } else if (controller.triggerButtonPressed)
        {
            
        }
        else if (controller.triggerButtonUp)
        {
            photonView.RPC("FinishSelection", PhotonTargets.AllBufferedViaServer);
        }


    }

    [PunRPC]
    void StartSelection(Vector3 position)
    {
        Selection.Clear();
    }


    /// <summary>
    /// Updates the selection based on the current active mode.
    /// </summary>
    /// <param name="position">The position of the selection cursor.</param>
    [PunRPC]
    void UpdateSelection(Vector3 position)
    {
        if (!CurrentObject) return;
        bool AlreadyHave = false;
        MeshEditor.MeshElement toSelect = null;
        switch (Mode)
        {
            case ModeSelector.EditMode.EDGE:
                toSelect = CurrentEditor.GetClosestEdge(position);
                foreach (var e in Selection)
                {
                    if ((e as MeshEditor.Edge).EdgeIndex == (toSelect as MeshEditor.Edge).EdgeIndex) AlreadyHave = true;
                }
                break;
            case ModeSelector.EditMode.VERTEX:
                toSelect = CurrentEditor.GetClosestVertex(position);
                if (Selection.Contains(toSelect)) AlreadyHave = true;
                toSelect.Editor = CurrentEditor;
                break;
            case ModeSelector.EditMode.FACE:
                toSelect = CurrentEditor.GetClosestFace(position);
                foreach (var e in Selection)
                {
                    if ((e as MeshEditor.Face).TriIndex == (toSelect as MeshEditor.Face).TriIndex) AlreadyHave = true;
                }
                break;
        }
        if (toSelect != null && !AlreadyHave)
        {
            Selection.Add(toSelect);
        }
    }
    [PunRPC]
    void FinishSelection()
    {
        OnFinishedSelection.Invoke(Selection);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trail") && controller.triggerButtonPressed)
        {
            if (CurrentObject != null)
            {
                if(CurrentObject != other)
                {
                    Selection.Clear();
                    CurrentObject = other.gameObject;
                }
                
            } else
            {
                CurrentObject = other.gameObject;
            }
            photonView.RPC("UpdateSelection", PhotonTargets.AllBufferedViaServer, Cursor.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == CurrentObject) CurrentObject = null;
    }


    //Selection event declaration... for whatever reason unity makes you extend the single param event class
    [System.Serializable]
    public class SelectionEvent : UnityEvent<List<MeshEditor.MeshElement>>{}
}
