using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.VR;
using System;

public class NetworkManager : MonoBehaviour {

    private const string typeName = "Simulator";
    private const string gameName = "SimRoom";

    private const string roomName = "Sculpt Room";
    private RoomInfo[] roomsList;

    private string organization = "default";



    public Camera sceneCam;
    public GameObject masterPrefab;
    public GameObject playerPrefab;
    public GameObject activeObserverPrefab;
    public GameObject passiveObserverPrefab;
    public GameObject VRPassiveObserverPrefab;
    public GameObject controllerObserverPrefab;
    public GameObject go;
    public GameObject OriginalCameraRig;


    public GameObject OfflineControlGroup;
    public GameObject OnlineNotConnectedControlGroup;
    public GameObject RoomJoinPrefab;
    public GameObject RoomJoin2DPrefab;
    public GameObject ConnectOnlineButton;
    public InputField RoomNameField;
    public InputField OrganizationField;
    public Transform ToolContainer;
    public Transform Shelves;
    private Transform RoomJoinButtons;
    public static Vector3 RoomScale;
    public WandController wandController;

    public Vector3 spawnLocation = new Vector3(0f, 0, 0f);
    private Vector3 TheatreLocation = new Vector3(0, 100, 0);
    private Vector3 controllerLocation = new Vector3(.3f, 1, -2.22f);

    public string onlineScene = "test";


    public static int buttonPressed = 0;

    // Nels April 27th 2017 wrote: 
    // Below are a list of methods for creating and joining
    // rooms with various access priveleges. Each of these need
    // to be removed from a GUI btn and assigned to an env var.


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        OnlineNotConnectedControlGroup.SetActive(false);
        RoomJoinButtons = OnlineNotConnectedControlGroup.transform;

        SceneManager.LoadScene("Draw", LoadSceneMode.Additive);
    }

    void PopulateRoomList()
    {
        if (PhotonNetwork.insideLobby) print("In a lobby... should see some rooms");
        roomsList = PhotonNetwork.GetRoomList();

        //Delete old buttons starts at 1 becasue 0th element is the create room stuff
        for(int ii = 1; ii < RoomJoinButtons.childCount; ii++)
        {
            Destroy(RoomJoinButtons.GetChild(ii).gameObject);
        }

        //Add the new buttons based on updated list
        int i = 0;
        foreach(RoomJoinButton rjb in FindObjectsOfType<RoomJoinButton>())
        {
            if (i >= roomsList.Length) return;

            GameObject newButton = Instantiate(RoomJoin2DPrefab);
            newButton.transform.SetParent(RoomJoinButtons, false);
            //rjb.transform.SetParent(RoomJoinButtons, false);
            newButton.GetComponent<RoomJoinButton>().SetInfo(roomsList, i);
            rjb.SetInfo(roomsList , i);
        }
    }
    
    public static void JoinRoom(string name)
    {
        
        PhotonNetwork.automaticallySyncScene = false;
        PhotonNetwork.JoinRoom(name);
    }

    //Handles scaling of UI tools for different room sizes
    void Position3DUI()
    {
        ToolContainer = GameObject.Find("ToolContainer").transform;
        foreach(var wc in FindObjectsOfType<WandController>())
        {
            wc.DrawingControlContainer = ToolContainer;
        }
        //Shelves = GameObject.Find("ControlContainer").transform;
        Valve.VR.HmdQuad_t roomDims = new Valve.VR.HmdQuad_t();
        SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref roomDims);
        RoomScale = new Vector3(Mathf.Abs(roomDims.vCorners0.v0 - roomDims.vCorners2.v0), Mathf.Abs(roomDims.vCorners0.v2 - roomDims.vCorners2.v2), 1);


        if (false)//RoomScale.x < 0.1f && RoomScale.y < 0.1f)
        {
            // Destroy(GameObject.Find("ToolContainer"));
            //Destroy(GameObject.Find("ControlContainer"));
            ToolContainer.Translate(-0.5f * 3, 0, 0);
            Shelves.Translate(0, 0, 0.5f * 3);
        }
        else
        {
            ToolContainer.Translate(-0.5f * RoomScale.x, 0, 0);
            //Shelves.Translate(0, 0, 0.5f * RoomScale.y);
        }
    }

    

    #region Callbacks

    private void OnConnectedToMaster()
    {
        TypedLobby OrganizationLobby = new TypedLobby(organization, LobbyType.Default);
        Debug.Log("Connected to MasterServer... Getting room list");
        OfflineControlGroup.SetActive(false);
        OnlineNotConnectedControlGroup.SetActive(true);
        if (PhotonNetwork.JoinLobby(OrganizationLobby))
        {
            PopulateRoomList();
        }
        
    }
    
    void OnReceivedRoomListUpdate()
    {
        roomsList = PhotonNetwork.GetRoomList();
        Debug.Log("Found " + roomsList.Length + " rooms.");
        PopulateRoomList();
    }

    void OnJoinedRoom()
    {
        Debug.Log("Connected to Room");
        SceneLoad();
    }

    void SceneLoad()
    {
        /*var SceneLoading = SceneManager.LoadSceneAsync("Draw");
        PhotonNetwork.LoadLevel("Draw");

        while (!SceneLoading.isDone)
        {
            yield return null;
        }*/
        PhotonNetwork.isMessageQueueRunning = false;
        OnlineNotConnectedControlGroup.SetActive(false);
        //wandController.InDrawingRoom = true;
        if (OriginalCameraRig != null)
            Destroy(OriginalCameraRig);
        SceneManager.UnloadScene("Lobby");

        //if (VRDevice.isPresent)
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (VRDevice.isPresent)
                    PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation, Quaternion.identity, 0);
                else
                    PhotonNetwork.Instantiate(controllerObserverPrefab.name, controllerLocation, Quaternion.identity, 0);
            }
            else
            {
                // determines what type of player based on GUI
                switch (buttonPressed)
                {
                    case 1: // instantiate player
                        PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation, Quaternion.identity, 0);
                        break;
                    case 2:
                    case 3: // instantiate observer
                        if (VRDevice.isPresent)
                            PhotonNetwork.Instantiate(passiveObserverPrefab.name, spawnLocation, Quaternion.identity, 0);
                        else
                            PhotonNetwork.Instantiate(controllerObserverPrefab.name, controllerLocation, Quaternion.identity, 0);
                        break;
                    case 4: // instantiate theatre mode
                        if (!VRDevice.isPresent)
                            PhotonNetwork.Instantiate(passiveObserverPrefab.name, TheatreLocation, Quaternion.identity, 0);
                        else
                            PhotonNetwork.Instantiate(VRPassiveObserverPrefab.name, TheatreLocation, Quaternion.identity, 0);
                        break;
                }
            }
            
        }
        /*else
        {
            PhotonNetwork.Instantiate(controllerObserverPrefab.name, spawnLocation, Quaternion.identity, 0);
        }*/

        PhotonNetwork.isMessageQueueRunning = true;
        Position3DUI();
        
    }


    #endregion


    #region Methods to connect to GUI buttons

    public void ConnectToServer()
    {
        PhotonNetwork.automaticallySyncScene = true;
        if (OrganizationField.text != "") organization = OrganizationField.text;
        PhotonNetwork.ConnectUsingSettings("4.2");
        GameObject.Find("Tracker").GetComponent<TrackerScript>().isOnline = true;
        ConnectOnlineButton.SetActive(false);
    }

    public void WorkOffline()
    {
        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom(roomName);
        GameObject.Find("Tracker").GetComponent<TrackerScript>().isOnline = false;
    }

    public void CreateRoom()
    {
        //Conditional is there just in case the user dosen't enter a name

        string roomName = RoomNameField.text == "" ? "defaultName" : RoomNameField.text;
        PhotonNetwork.CreateRoom(roomName);
        GameObject.Find("Tracker").GetComponent<TrackerScript>().isOnline = true;
        GameObject.Find("Tracker").GetComponent<TrackerScript>().isHost = true;
    }

    #endregion
}
