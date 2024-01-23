using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomItem : MonoBehaviour
{

    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerCount;

    public void SetRoomName(string _roomName)
    {
        roomName.text = _roomName;
    }

    public string GetRoomName()
    {
        return roomName.text;
    }

    public void SetPlayerCount(int _playerCount)
    {
        playerCount.text = "Players: " + _playerCount + "/16";
    }

    public void OnClickJoin()
    {
        LobbyManager.instance.JoinRoomByName(roomName.text);
    }
}
