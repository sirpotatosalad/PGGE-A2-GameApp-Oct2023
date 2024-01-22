using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{

    public static LobbyManager instance;

    public TMP_InputField roomInpField;
    public TextMeshProUGUI welcomeText;

    [Header("UI")] public Transform contentListParent;
    public RoomItem roomListItemPrefab;

    private List<RoomItem> roomItemsList = new List<RoomItem>();

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.JoinLobby();
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
        foreach (RoomItem room in roomItemsList)
        {
            Destroy(room.gameObject);
        }
        roomItemsList.Clear();

        foreach (RoomInfo room in roomList)
        {
            RoomItem newRoom = Instantiate(roomListItemPrefab, contentListParent);
            newRoom.SetRoomName(room.Name);
            newRoom.SetPlayerCount(room.PlayerCount);
            roomItemsList.Add(newRoom);
        }
    }

    public void JoinRoomByName(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

}
