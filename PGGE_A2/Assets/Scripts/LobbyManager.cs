using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class LobbyManager : MonoBehaviourPunCallbacks
{

    public static LobbyManager instance;
    public TextMeshProUGUI welcomeText;

    [Header("Room Creation")]
    public TMP_InputField roomInpField;
    public TMP_Dropdown roomSizeDropdown;
    public TextMeshProUGUI roomCreationError;





    [Header("ContentList")] public Transform contentListParent;
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

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        roomCreationError.text = "";
    }


    public void OnCreateCustomRoom()
    {
        if (roomInpField != null && roomSizeDropdown != null)
        {
            if (string.IsNullOrWhiteSpace(roomInpField.text))
            {
                Debug.Log("Field is empty.");
                roomCreationError.text = "Room name cannot be empty. Please enter a valid room name.";

                StartCoroutine(ClearErrorMessage(5f));

                return;

            }

            int maxPlayers = int.Parse(roomSizeDropdown.options[roomSizeDropdown.value].text);

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = (byte)maxPlayers;

            PhotonNetwork.CreateRoom("[Custom] " + roomInpField.text, roomOptions, TypedLobby.Default);
            Debug.Log("Room Created - Name: " + "[Custom] " + roomInpField.text + ", Max Players: " + maxPlayers);
        }
    }

    private IEnumerator ClearErrorMessage(float duration)
    {
        yield return new WaitForSeconds(duration);

        roomCreationError.text = "";
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

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            Debug.Log("Joined Lobby");
        }
    }



    void UpdateRooms(List<RoomInfo> roomList)
    {

        foreach (RoomItem room in roomItemsList)
        {
            Destroy(room.gameObject);
        }
        roomItemsList.Clear();

        for (int i = 0; i < roomsToAdd; i++)
        {
            CreateOrUpdateDummyRoom("[Default] Room " + (1 + i));
        }

        foreach (RoomInfo room in roomList)
        {

            if (dummyRoomsList.Count > 0)
            {
                int index = dummyRoomsList.FindIndex(x => x.roomName.text == room.Name);
                if (index != -1 && dummyRoomsList[index] != null)
                {
                    Destroy(dummyRoomsList[index].gameObject);
                    dummyRoomsList.RemoveAt(index);
                }
            }

            if (room.RemovedFromList)
            {
                int index = roomItemsList.FindIndex(x => x.roomName.text == room.Name);
                if (index != -1)
                {
                    Destroy(roomItemsList[index].gameObject);
                    roomItemsList.RemoveAt(index);
                }

                CreateOrUpdateDummyRoom(room.Name);
            }
            else
            {

                RoomItem newRoom = Instantiate(roomListItemPrefab, contentListParent);
                if (newRoom != null)
                {
                    newRoom.SetRoomName(room.Name);
                    newRoom.SetPlayerCount(room.PlayerCount, room.MaxPlayers);

                    float occupancyPercentage = (float)room.PlayerCount / room.MaxPlayers;

                    if (occupancyPercentage >= 0.7f)
                    {
                        newRoom.SetColour(Color.red);
                    }
                    else if (occupancyPercentage >= 0.5f)
                    {
                        newRoom.SetColour(Color.yellow);
                    }
                    else
                    {
                        newRoom.SetColour(Color.white);
                    }

                    roomItemsList.Add(newRoom);
                }


            }

        }
 
    }


    void CreateOrUpdateDummyRoom(string _roomName)
    {
        // Check if the room is a custom room
        bool isCustomRoom = !_roomName.StartsWith("[Default]");

        if (!isCustomRoom)
        {
            // Check if the dummy room already exists
            RoomItem dummyRoom = dummyRoomsList.Find(x => x.GetRoomName() == _roomName);

            if (dummyRoom == null)
            {
                // If not, create a new dummy room
                dummyRoom = Instantiate(roomListItemPrefab, contentListParent);
                dummyRoom.SetPlayerCount(0,4);
                dummyRoomsList.Add(dummyRoom);
            }

            // Set dummy room name
            dummyRoom.SetRoomName(_roomName);
        }
    }


    public void JoinRoomByName(string roomName)
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(roomName,roomOptions,TypedLobby.Default);
    }

    public void RefreshList()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene("Multiplayer_Lobby");
       
    }

    //public void OnClickJoinRandomRoom(RoomItem )
    //{
    //    if (PhotonNetwork.IsConnected)
    //    {
    //        PhotonNetwork.JoinRandomRoom();
    //    }
    //    else if (dummyRoom)
    //    {

    //    }
    //}



    public void OnClickJoinRandomRoom()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        Debug.Log("Finding a room");
        List<RoomItem> allRooms = new List<RoomItem>();
        allRooms.AddRange(dummyRoomsList);
        allRooms.AddRange(roomItemsList);

        if (allRooms.Count > 0)
        {
            // Pick a random index from allRooms
            int randomIndex = Random.Range(0, allRooms.Count);

            // Access the RoomItem at the randomly picked index
            RoomItem randomRoomItem = allRooms[randomIndex];

            // Check if randomRoomItem is not null before attempting to join
            if (randomRoomItem != null)
            {
                // Join the room using its name
                JoinRoomByName(randomRoomItem.GetRoomName());
            }
            // Handle the case where randomRoomItem is null (no valid rooms)
        }
        else
        {
            // Handle the case where allRooms is empty (no valid rooms)
            Debug.Log("No valid rooms available to join.");
        }
    }

    public void OnClickBackButton()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        PhotonNetwork.Disconnect();
        Debug.Log("Disconnecting from Lobby");
        StartCoroutine(DisconnectDelay());
        
    }

    IEnumerator DisconnectDelay()
    {
        yield return new WaitForSeconds(0.75f);
        Debug.Log("Goodbye");
        SceneManager.LoadScene("Multiplayer_Launcher");
    }

}
