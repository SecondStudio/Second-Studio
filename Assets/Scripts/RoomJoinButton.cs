using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomJoinButton : MonoBehaviour {

    public RoomInfo[] MyInfo;
    public Text RoomNameText;
    public int Index;

	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetInfo(RoomInfo[] inInfo , int index)
    {
        Index = index;
        MyInfo = inInfo;
        RoomNameText.text = MyInfo[index].name;

    }

    public void JoinAsActiveVRObserver()
    {
        NetworkManager.buttonPressed = 1;
        NetworkManager.JoinRoom(MyInfo[Index].name);
    }

    public void JoinAsPassiveVRObserver()
    {
        NetworkManager.buttonPressed = 2;
        PhotonNetwork.JoinRoom(MyInfo[Index].name);
    }

    public void JoinAsPCObserver() //we prob don't need this because they cant press with vive anyways
    {
        NetworkManager.buttonPressed = 3;
        PhotonNetwork.JoinRoom(MyInfo[Index].name);
    }

    public void JoinAsVRTheater()
    {
        NetworkManager.buttonPressed = 4;
        PhotonNetwork.JoinRoom(MyInfo[Index].name);
    }

    public void NextInfo()
    {
        Index = Index + 1 >= MyInfo.Length ? 0 : Index + 1;
        RoomNameText.text = MyInfo[Index].name;
    }
    public void PrevInfo()
    {
        Index = Index == 0 ? MyInfo.Length - 1 : Index - 1;
        RoomNameText.text = MyInfo[Index].name;
    }

}
