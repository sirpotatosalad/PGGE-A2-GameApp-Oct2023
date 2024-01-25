using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class RoomItem : MonoBehaviour
{

    // script for the RoomItem prefab
    // contains simple get and set functions to obtain data from the prefab

    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerCount;
    public Image bgImg;

    private Color defaultColour;

    private void Start()
    {
        defaultColour = bgImg.color;
    }

    public void SetRoomName(string _roomName)
    {
        roomName.text = _roomName;
    }

    public string GetRoomName()
    {
        return roomName.text;
    }

    public void SetPlayerCount(int _playerCount, int _maxPlayers)
    {
        playerCount.text = "Players: " + _playerCount + "/" + _maxPlayers;
    }

    // gets the numerical player count
    public int GetPlayerCount()
    {
        // parse the player count from the playerCount.text
        string[] playerCountTextParts = playerCount.text.Split('/');

        if (playerCountTextParts.Length == 2)
        {
            int currentPlayers;
            if (int.TryParse(playerCountTextParts[0], out currentPlayers))
            {
                return currentPlayers;
            }
        }

        return 0; // default value if parsing fails
    }

    public void SetColour(Color _color)
    {
        bgImg.color = _color;
    }
    public void ResetColour()
    {
        bgImg.color = defaultColour;
    }

    // additional Join Button logic for the prefab
    public void OnClickJoin()
    {
        // calls the JoinRoomByName function in LobbyManager by accessing its singleton, passing in the prefab's room name
        LobbyManager.instance.JoinRoomByName(roomName.text);
    }
}
