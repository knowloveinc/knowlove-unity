using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameBrewStudios
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {

        public List<RoomInfo> roomList = new List<RoomInfo>();

        #region Listeners


        public void Connect()
        {

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "0.1.1";

            PhotonNetwork.NickName = User.current.displayName;

            PhotonNetwork.ConnectUsingSettings();
        }

        

        public override void OnConnected()
        {
            base.OnConnected();
            Debug.Log("CONNECTED TO PHOTON");
        }

        public override void OnConnectedToMaster()
        {
            //OnConnectedToMaster being called signals that we can now make calls to join rooms and list them etc

            //if (isConnecting)
            //{
            //    PhotonNetwork.JoinRandomRoom();
            //}

            base.OnConnectedToMaster();

            Debug.Log("CONNECTED TO MASTER");

            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"Disconnected due to: {cause}");
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            Debug.Log("CREATED ROOM");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.LogError("CREATE ROOM FAILED");
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("JOINED LOBBY");

            OnPhotonConnected?.Invoke();
        }

        public override void OnJoinedRoom()
        {
            //Triggered when the local player joins a room.
            base.OnJoinedRoom();

            UpdatePlayerList();

            Debug.Log($"JOINED ROOM: {PhotonNetwork.CurrentRoom.Name}");

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                Debug.Log(playerCount + " of " + PhotonNetwork.CurrentRoom.MaxPlayers);
                Debug.Log("Joined room that is waiting for more players...");
                
            }
            else
            {
                Debug.Log("Joined room and the match is now ready to start...");
                CanvasLoading.Instance.ForceHide();
                PhotonNetwork.LoadLevel("Testing");
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);

            Debug.LogError("No room found.");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            foreach (RoomInfo ri in roomList)
                Debug.Log("ROOM FOUND: " + ri.Name);
            this.roomList = roomList;
            OnRoomListUpdated?.Invoke();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log("LEFT ROOM");
            isConnecting = false;
        }

        public List<Player> players = new List<Player>();

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            UpdatePlayerList();

            Debug.Log("Another player has joined the room: " + newPlayer.NickName);

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                Debug.Log("Match is ready to begin.");

                OnRoomFull?.Invoke();

                PhotonNetwork.LoadLevel("Testing");
                CanvasLoading.Instance.ForceHide();
            }
        }

        private void UpdatePlayerList()
        {
            players = PhotonNetwork.PlayerList.ToList();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            OnPlayerLeft?.Invoke(otherPlayer);
            UpdatePlayerList();
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
        }

        #endregion


        public static NetworkManager Instance;

        #region Events

        public static event System.Action OnPhotonConnected;
        public static event System.Action OnRoomFull;
        public static event System.Action<Player> OnPlayerLeft;
        public static event Action OnRoomListUpdated;

        #endregion

        #region Public Methods


        public bool isConnecting = false;

        

        private void Awake()
        {
            
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);


            
        }

        public bool isUserAuthenticated()
        {
            return User.current != null;
        }


        #endregion
    }
}