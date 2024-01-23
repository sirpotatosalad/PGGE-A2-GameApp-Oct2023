using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LobbyManager : MonoBehaviourPunCallbacks
{

    public static LobbyManager instance;

    public TMP_InputField roomInpField;
    public TextMeshProUGUI welcomeText;

    [Header("UI")] public Transform contentListParent;
    public RoomItem roomListItemPrefab;

    private List<RoomItem> roomItemsList = new List<RoomItem>();
    private List<RoomItem> dummyRoomsList = new List<RoomItem>();

    [Header("Default Rooms")] public int roomsToAdd;


    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Joined Lobby");
        welcomeText.text = "Hello, " + PhotonNetwork.NickName;

    }


    public void OnCreateRoom()
    {
        if (roomInpField != null)
        {
            PhotonNetwork.CreateRoom(roomInpField.text);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Client is in a room.");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Joining: " + PhotonNetwork.CurrentRoom.Name);
            PhotonNetwork.LoadLevel("MultiplayerMap00");
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        UpdateRooms(roomList);
    }

    void UpdateRooms(List<RoomInfo> roomList)
    {
        //dummyRoomsList.Clear();

        //for (int i = 0; i < roomsToAdd; i++)
        //{
        //    string newRoomName = "Room " + (1 + i);
            
        //    if (!dummyRoomsList.Exists(x => x.roomName.text == newRoomName))
        //    {
        //        RoomItem dummyRoom = Instantiate(roomListItemPrefab, contentListParent);
        //        dummyRoom.SetRoomName(newRoomName);
        //        dummyRoom.SetPlayerCount(0);
        //        dummyRoomsList.Add(dummyRoom);
        //    }
            
        //}

        foreach (RoomInfo room in roomList)
        {

            if (dummyRoomsList.Count > 0)
            {
                int index = dummyRoomsList.FindIndex(x => x.roomName.text == room.Name);
                if (index != -1 && dummyRoomsList[index] != null)
                {
                    Destroy(dummyRoomsList[index].gameObject);
                }
            }

            if (room.RemovedFromList)
            {
                int index = roomList.FindIndex(x => x.Name == room.Name);
                if (index != -1)
                {
                    Destroy(roomItemsList[index].gameObject);
                    roomItemsList.RemoveAt(index);
                }
            }
            else
            {
                RoomItem newRoom = Instantiate(roomListItemPrefab, contentListParent);
                if (newRoom != null)
                {
                    newRoom.SetRoomName(room.Name);
                    newRoom.SetPlayerCount(room.PlayerCount);
                    roomItemsList.Add(newRoom);
                }
                
            }

        }

        


    }

    public void JoinRoomByName(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        PhotonNetwork.JoinOrCreateRoom(roomName,roomOptions,TypedLobby.Default);
    }

    public void RefreshList()
    {
        PhotonNetwork.GetCustomRoomList(TypedLobby.Default, null);
    }

}
