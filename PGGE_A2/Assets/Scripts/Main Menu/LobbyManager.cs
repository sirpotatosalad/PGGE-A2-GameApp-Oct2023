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

    // allows selection of "dummy rooms" to start with
    [Header("Default Rooms")] public int roomsToAdd;

    // a simplified version of singleton
    // done simply to ease the implementation of the RoomItem prefab
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // joins the main lobby on start,
        // greets player with their nickname chosen in launcher scene
        PhotonNetwork.JoinLobby();
        Debug.Log("Joined Lobby");
        welcomeText.text = "Hello, " + PhotonNetwork.NickName;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        roomCreationError.text = "";
    }

    //logic for room creation button
    public void OnCreateCustomRoom()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        if (roomInpField != null && roomSizeDropdown != null)
        {
            // check if player has left input field empty
            if (string.IsNullOrWhiteSpace(roomInpField.text))
            {
                Debug.Log("Field is empty.");
                roomCreationError.text = "Room name cannot be empty. Please enter a valid room name.";

                StartCoroutine(ClearErrorMessage(5f));

                return;

            }

            // set the size of the room based on the number selected by player in drop-down ui
            int maxPlayers = int.Parse(roomSizeDropdown.options[roomSizeDropdown.value].text);

            // set the room size and create a custom room with player chosen name
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

    // Loads Multiplayer map when clicking on join room button for RoomItem prefab.
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Client is in a room.");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Joining: " + PhotonNetwork.CurrentRoom.Name);
            PhotonNetwork.LoadLevel("MultiplayerMap00");
        }
    }

    // to update room list when player is in lobby
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


   // basically, every time this function is called in OnRoomListUpdate it will update the the Room list by clearing and replacing any rooms that have changed
    void UpdateRooms(List<RoomInfo> roomList)
    {
        // clear roomItemsList and destroy all RoomItem prefabs
        foreach (RoomItem room in roomItemsList)
        {
            Destroy(room.gameObject);
        }
        roomItemsList.Clear();

        //create default dummy rooms based on number of rooms indicated in inspector
        for (int i = 0; i < roomsToAdd; i++)
        {
            CreateOrUpdateDummyRoom("[Default] Room " + (1 + i));
        }

        foreach (RoomInfo room in roomList)
        {
            // check if there are any "real" dummy rooms, and delete the fake version
            if (dummyRoomsList.Count > 0)
            {
                // finds if index of any dummy room inside dummyRoomsList match an existing room
                int index = dummyRoomsList.FindIndex(x => x.roomName.text == room.Name);
                // destroys and removes gameObject and list entry if any are found
                if (index != -1 && dummyRoomsList[index] != null)
                {
                    Destroy(dummyRoomsList[index].gameObject);
                    dummyRoomsList.RemoveAt(index);
                }
            }

            // delete custom rooms that are empty and have been closed/deleted by photon
            if (room.RemovedFromList)
            {
                // same logic as for the dummy room check, only now for custom rooms.
                int index = roomItemsList.FindIndex(x => x.roomName.text == room.Name);
                if (index != -1)
                {
                    Destroy(roomItemsList[index].gameObject);
                    roomItemsList.RemoveAt(index);
                }

                // check if the deleted room is a closed dummy room
                // if so, create a new dummy room - based on the closed room's name
                CreateOrUpdateDummyRoom(room.Name);
            }
            else
            {
                // create a new prefab with updated information for each current room
                RoomItem newRoom = Instantiate(roomListItemPrefab, contentListParent);
                if (newRoom != null)
                {
                    newRoom.SetRoomName(room.Name);
                    newRoom.SetPlayerCount(room.PlayerCount, room.MaxPlayers);

                    // calculates the % occupancy of each room, changing the bg colour based on how many slots are left.
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

    //logic to create dummy rooms
    void CreateOrUpdateDummyRoom(string _roomName)
    {
        // check if the room is a custom room
        bool isCustomRoom = !_roomName.StartsWith("[Default]");

        if (!isCustomRoom)
        {
            // check if the dummy room already exists
            RoomItem dummyRoom = dummyRoomsList.Find(x => x.GetRoomName() == _roomName);

            if (dummyRoom == null)
            {
                // if not, create a new dummy room
                dummyRoom = Instantiate(roomListItemPrefab, contentListParent);
                dummyRoom.SetPlayerCount(0,4);
                dummyRoomsList.Add(dummyRoom);
            }

            // set dummy room name
            dummyRoom.SetRoomName(_roomName);
        }
    }

    // logic for join button for RoomItem prefab
    public void JoinRoomByName(string roomName)
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(roomName,roomOptions,TypedLobby.Default);
    }

    // rudimentary and simple way to "refresh" the main lobby, in case bugs or no updates occur.
    public void RefreshList()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene("Multiplayer_Lobby");
       
    }



    // logic for Join Random Room button
    public void OnClickJoinRandomRoom()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        Debug.Log("Finding a room");
        // compiles all current rooms in the server - including dummy rooms
        List<RoomItem> allRooms = new List<RoomItem>();
        allRooms.AddRange(dummyRoomsList);
        allRooms.AddRange(roomItemsList);
        
        // joins a random room - so long as there are rooms available/visible in the room list.
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

    // logic for back button to return to previous scene
    public void OnClickBackButton()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();

        // disconnects current player from photon, allowing them to change name as well.
        PhotonNetwork.Disconnect();
        Debug.Log("Disconnecting from Lobby");
        SceneManager.LoadScene("Multiplayer_Launcher");
        Debug.Log("Goodbye");

    }



}
