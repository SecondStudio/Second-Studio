using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(PhotonView))]
public class GridSnapTool : Photon.MonoBehaviour {

    public static GridSnapTool instance;
    public static float SizeDelta = 0.05f;
    public static float SnapDistance = 0.25f;
    static bool GridSnappingEnabled = false;
    static bool ElementSnappingEnabled = false;
    public Text SizeIndicatorText;
    public GameObject GridlinePrefab;
    private List<GameObject> Gridlines;
    public GameObject leftController, rightController;
    public enum ElementSnapType { VERTEX , EDGE , FACE}
    public static float MinSnapDistance = 0.25f;
    public static int ElementSnapIndex = 0;
    // Use this for initialization
    void Start () {
        Gridlines = new List<GameObject>();
        if (instance == null)
        {
            instance = this;
            Gridlines = new List<GameObject>();
        } else
        {
            //Destroy(gameObject);
        }
	}

    private void Update()
    {
        for(int i=0; i<Gridlines.Count; i++)
        {
            if(Vector3.Distance(Gridlines[i].transform.position, leftController.transform.position) > 1.0f && Vector3.Distance(Gridlines[i].transform.position, rightController.transform.position) > 1.0f)
            {
                Gridlines[i].SetActive(false);
            }
            else
                Gridlines[i].SetActive(true);
        }
    }


    public void IncreaseGrid()
    {
        photonView.RPC("IncreaseGridSize", PhotonTargets.AllBufferedViaServer);
    }

    public void DecreaseGrid()
    {
        photonView.RPC("DecreaseGridSize", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    public void EnableGridSnapping()
    {
        GridSnappingEnabled = true;
        DisableElementSnapping();
        DrawGridlines();
    }

    [PunRPC]
    public void DisableGridSnapping()
    {
        GridSnappingEnabled = false;
        ClearGridlines();
    }

    [PunRPC]
    public void EnableElementSnapping()
    {
        ElementSnappingEnabled = true;
        DisableGridSnapping();
    }

    [PunRPC]
    public void DisableElementSnapping()
    {
        ElementSnappingEnabled = false;
        ClearGridlines();
    }

    [PunRPC]
    public void IncreaseGridSize()
    {
        SnapDistance += SizeDelta;
        if (SnapDistance < 0.25) SnapDistance = 0.25f;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("GridSizeText"))
        {
            go.GetComponent<Text>().text = SnapDistance.ToString("0.00");
        }

        SizeIndicatorText.text = "" + SnapDistance;
        if (GridSnappingEnabled)
        {
            ClearGridlines();
            DrawGridlines();
        }
    }
    [PunRPC]
    public void DecreaseGridSize()
    {
        SnapDistance -= SizeDelta;
        if (SnapDistance < 0.25) SnapDistance = 0.25f;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("GridSizeText"))
        {
            go.GetComponent<Text>().text = SnapDistance.ToString("0.00");
        }
        if (GridSnappingEnabled)
        {
            ClearGridlines();
            DrawGridlines();
        }
        
    }

    public void ToggleGridSnap()
    {
        GridSnappingEnabled = !GridSnappingEnabled;
        if (GridSnappingEnabled)
        {
            photonView.RPC("EnableGridSnapping", PhotonTargets.AllBufferedViaServer);
        } else
        {
            photonView.RPC("DisableGridSnapping", PhotonTargets.AllBufferedViaServer);
        }

        //TrackerScript.AddAction("R");
    }
    public void ToggleElementSnap()
    {
        ElementSnappingEnabled = !ElementSnappingEnabled;
        if (ElementSnappingEnabled)
        {
            photonView.RPC("EnableElementSnapping", PhotonTargets.AllBufferedViaServer);
        }
        else
        {
            photonView.RPC("DisableElementSnapping", PhotonTargets.AllBufferedViaServer);
        }

        //TrackerScript.AddAction("R");
    }

    public void NextElementType()
    {
        photonView.RPC("NextElement", PhotonTargets.AllBufferedViaServer);
    }
    public void PrevElementType()
    {
        photonView.RPC("PrevElement", PhotonTargets.AllBufferedViaServer);
    }

    [PunRPC]
    void NextElement()
    {
        ElementSnapIndex++;
        if (ElementSnapIndex > 2) ElementSnapIndex = 0;
    }
    [PunRPC]
    void PrevElement()
    {
        ElementSnapIndex--;
        if (ElementSnapIndex < 0) ElementSnapIndex = 2;
    }


    /// <summary>
    /// Snaps the specified position to the grid, or the closest element of the active type (Defined by ModeSelector)
    /// You should always call this method rather than calling Element or Grid methods directly
    /// </summary>
    /// <param name="inPosition">The in position.</param>
    /// <param name="toIgnore">This object is ignored in element snap mode. Used so that objects won't try to snap to themselves</param>
    /// <returns></returns>
    public static Vector3 Snap(Vector3 inPosition , GameObject toIgnore = null)
    {
        if (GridSnappingEnabled) return SnapToGrid(inPosition);
        if (ElementSnappingEnabled) return SnapToElement( inPosition,  toIgnore);
        return inPosition;
    }

    public static Vector3 SnapToGrid(Vector3 inPosition)
    {
        Vector3 SnappedPos = new Vector3();
        int xIndex = (int)(inPosition.x / SnapDistance);
        int yIndex = (int)(inPosition.y / SnapDistance);
        int zIndex = (int)(inPosition.z / SnapDistance);


        SnappedPos.x = Mathf.Abs(inPosition.x % SnapDistance) > (SnapDistance / 2f) ? (inPosition.x >= 0 ? xIndex + 1 : xIndex - 1) * SnapDistance : (xIndex) * SnapDistance; //Magic
        SnappedPos.y = Mathf.Abs(inPosition.y % SnapDistance) > (SnapDistance / 2f) ? (inPosition.y >= 0 ? yIndex + 1 : yIndex - 1) * SnapDistance : (yIndex) * SnapDistance;
        SnappedPos.z = Mathf.Abs(inPosition.z % SnapDistance) > (SnapDistance / 2f) ? (inPosition.z >= 0 ? zIndex + 1 : zIndex - 1) * SnapDistance : (zIndex) * SnapDistance;

        return SnappedPos;
    }

    public static Vector3 SnapToElement(Vector3 inPosition, GameObject toIgnore)
    {
        GameObject trail = null;
        float dist = float.MaxValue;

        foreach(var g in GameObject.FindGameObjectsWithTag("Trail"))
        {
            if(g != toIgnore && Vector3.Distance(inPosition , g.transform.position) < dist)
            {
                trail = g;
                dist = Vector3.Distance(inPosition, g.transform.position);
            }
        }
        if (trail)
        {
            switch (ModeSelector.CurrentMode)
            {
                case ModeSelector.EditMode.VERTEX:
                    return trail.GetComponent<MeshEditor>().GetClosestVertex(inPosition).WorldPosition;
                case ModeSelector.EditMode.EDGE:
                    return trail.transform.TransformPoint(trail.GetComponent<MeshEditor>().GetClosestEdge(inPosition).center);
                case ModeSelector.EditMode.FACE:
                    return trail.transform.TransformPoint(trail.GetComponent<MeshEditor>().GetClosestFace(inPosition).center);
            }
        }
        return inPosition;
        
    }

    /// <summary>
    /// Draws the gridlines based on the size of the play area provided by steam VR.
    /// </summary>
    void DrawGridlines()
    {
        if (SnapDistance == 0) return;
        ClearGridlines();
        Valve.VR.HmdQuad_t roomDims = new Valve.VR.HmdQuad_t();
        SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref roomDims);
        Vector3 RoomScale = new Vector3(Mathf.Abs(roomDims.vCorners0.v0 - roomDims.vCorners2.v0), Mathf.Abs(roomDims.vCorners0.v2 - roomDims.vCorners2.v2), 1);
        float roomWidth = RoomScale.x;
        float roomDepth = RoomScale.y;
        float height = 3;

        for(int i = -(int)((roomWidth / SnapDistance)); i <= (int)((roomWidth / SnapDistance)); i++)
        {
            for(int j = -(int)((height/SnapDistance)); j <= (int)((height / SnapDistance)); j++)
            {
                var gl = Instantiate(GridlinePrefab);
                var lr = gl.GetComponent<LineRenderer>();
                lr.SetPosition(0 , new Vector3(i*SnapDistance , j*SnapDistance, -roomDepth));
                lr.SetPosition(1, new Vector3(i * SnapDistance, j * SnapDistance, roomDepth));
                gl.transform.position = new Vector3(i * SnapDistance, j * SnapDistance, 0);
                Gridlines.Add(gl);
            }
        }

        for (int i = -(int)((roomDepth / SnapDistance)); i <= (int)((roomDepth / SnapDistance)); i++)
        {
            for (int j = -(int)((height / SnapDistance)); j <= (int)((height / SnapDistance)); j++)
            {
                var gl = Instantiate(GridlinePrefab);
                var lr = gl.GetComponent<LineRenderer>();
                lr.SetPosition(0, new Vector3(-roomWidth, j * SnapDistance, i * SnapDistance));
                lr.SetPosition(1, new Vector3(roomWidth, j * SnapDistance, i * SnapDistance));
                gl.transform.position = new Vector3(0, j * SnapDistance, i * SnapDistance);
                Gridlines.Add(gl);
            }
        }

        for (int i = -(int)((roomWidth / SnapDistance)); i <= (int)((roomWidth / SnapDistance)); i++)
        {
            for (int j = -(int)((roomDepth / SnapDistance)); j <= (int)((roomDepth / SnapDistance)); j++)
            {
                var gl = Instantiate(GridlinePrefab);
                var lr = gl.GetComponent<LineRenderer>();
                lr.SetPosition(0, new Vector3(i * SnapDistance, -height , j * SnapDistance));
                lr.SetPosition(1, new Vector3(i * SnapDistance, height, j * SnapDistance));
                gl.transform.position = new Vector3(i * SnapDistance, 0, j * SnapDistance);
                Gridlines.Add(gl);
            }
        }
    }
    void ClearGridlines()
    {
        foreach (var g in Gridlines)
        {
            DestroyImmediate(g);
        }
        Gridlines.Clear();
    }
}
