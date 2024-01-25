using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public string mPlayerPrefabName;
    public PlayerSpawnPoints mSpawnPoints;

    [HideInInspector]
    public GameObject mPlayerGameObject;
    [HideInInspector]
    private ThirdPersonCamera mThirdPersonCamera;

    private void Start()
    {
        Transform randomSpawnTransform = mSpawnPoints.GetSpawnPoint();
        mPlayerGameObject = PhotonNetwork.Instantiate(mPlayerPrefabName,
            randomSpawnTransform.position,
            randomSpawnTransform.rotation,
            0);

        mThirdPersonCamera = Camera.main.gameObject.AddComponent<ThirdPersonCamera>();

        //mPlayerGameObject.GetComponent<PlayerMovement>().mFollowCameraForward = false;
        mThirdPersonCamera.mPlayer = mPlayerGameObject.transform;
        mThirdPersonCamera.mDamping = 20.0f;
        mThirdPersonCamera.mCameraType = CameraType.Follow_Track_Pos_Rot;
    }

    // modified previous code to leave the current room, and making use of the button sound.
    public void LeaveRoom()
    {
        MenuSoundManager.Instance.PlayButtonClickSound();
        Debug.LogFormat("LeaveRoom");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.LogFormat("OnLeftRoom()");
    }


    // when player connects to the master server after leaving room, reconnects them to the Multiplayer_Lobby scene
    public override void OnConnectedToMaster()
    {
        // make player join the main lobby to allow them to see room updates
        PhotonNetwork.JoinLobby();
        Debug.Log("Joining back main lobby");
        // delay loading of multiplayer_lobby to allow player to fully connect to the main lobby first
        StartCoroutine(LoadWithDelay());
        //SceneManager.LoadScene("Multiplayer_Lobby");
    }

    IEnumerator LoadWithDelay()
    {
        yield return new WaitForSeconds(0.75f);
        SceneManager.LoadScene("Multiplayer_Lobby");
    }

}
