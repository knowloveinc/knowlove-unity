using GameBrewStudios;
using Knowlove.RoomReconnect;
using Knowlove.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Knowlove
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance;

        public static event Action OnJoinedRoomFinished;

        public List<RoomInfo> roomList = new List<RoomInfo>();
        public List<Player> players = new List<Player>();

        public bool readyToStart = false;
        public bool isLeave = false;
        public bool isReconnect = false;

        private const string _roomName = "RoomName";
        private const string _playerProperties = "Properties";

        #region Events

        public static event System.Action OnPhotonConnected;
        public static event System.Action OnReadyToStart;
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

            gameObject.AddComponent<PhotonView>();

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.DeleteKey(_roomName);
        }

        public bool isUserAuthenticated()
        {
            return User.current != null;
        }
        #endregion

        #region Listeners
        public void Connect()
        {
            if (isReconnect)
                CanvasLoading.Instance.Show();

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "0.8.2";

            PhotonNetwork.NickName = User.current != null ? User.current.displayName : "Player " + UnityEngine.Random.Range(1, 9999);

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
            base.OnConnectedToMaster();

            Debug.Log("CONNECTED TO MASTER");

            TypedLobby sqlLobby = new TypedLobby("sqlLobby", LobbyType.Default);

            if (!PhotonNetwork.OfflineMode)
                PhotonNetwork.JoinLobby(sqlLobby);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"Disconnected due to: {cause}");
            CanvasLoading.Instance.Hide();

            if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.DisconnectByServerLogic)
            {

                PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
                {
                    new PopupDialog.PopupButton()
                    {
                        text = "Back to Menu",
                        onClicked = () =>
                        {
                            isLeave = true;
                            isReconnect = false;
                            PhotonNetwork.Disconnect();

                            PlayerPrefs.DeleteKey(_roomName);
                            StartCoroutine(GoBackToMainMenu());
                        },
                        buttonColor = PopupDialog.PopupButtonColor.Plain
                    },
                    new PopupDialog.PopupButton()
                    {
                        text = "Back to Game",
                        onClicked = () =>
                        {
                            Connect();
                            isReconnect = true;
                        },
                        buttonColor = PopupDialog.PopupButtonColor.Plain
                    }
                };

                PopupDialog.Instance.Show("Disconnected", $"Lost connection to game server. Reason: {cause}\n\n Please try again later.", buttons);
            }
            else
                Debug.Log("Disconnect appear to be intentional...");
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            Debug.Log("CREATED ROOM");
            string username = User.current.displayName;
            //if (!PhotonNetwork.OfflineMode)
            //{
            //    username += " [host]";
            //}
            PhotonNetwork.LocalPlayer.NickName = username;
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.LogError("CREATE ROOM FAILED");
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            base.OnJoinRoomFailed(returnCode, message);
            Debug.LogError("JOIN ROOM FAILED: " + returnCode + " " + message);

            CanvasLoading.Instance.ForceHide();
            PopupDialog.Instance.Show("Failed to join game: " + message);
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("JOINED LOBBY");
            OnPhotonConnected?.Invoke();

            if (PlayerPrefs.HasKey(_roomName) && roomList != null)
            {
                string roomName = PlayerPrefs.GetString(_roomName);

                foreach (RoomInfo ri in roomList)
                {
                    if (ri.Name == roomName)
                    {
                        PhotonNetwork.JoinRoom(roomName);
                        return;
                    }
                }

                RoomOptions options = new RoomOptions()
                {
                    PublishUserId = true,
                    MaxPlayers = 1,
                    IsOpen = false,
                    IsVisible = false,
                    EmptyRoomTtl = 0
                };

                PhotonNetwork.CreateRoom("OfflineMode" + UnityEngine.Random.Range(9, 999999), options);
            }                
        }

        public override void OnJoinedRoom()
        {
            //Triggered when the local player joins a room.
            base.OnJoinedRoom();

            PlayerPrefs.SetString(_roomName, PhotonNetwork.CurrentRoom.Name);
            UpdatePlayerList();

            if (PhotonNetwork.IsMasterClient && !isReconnect)
                StartCoroutine(WaitingForPlayers());

            Debug.Log($"JOINED ROOM: {PhotonNetwork.CurrentRoom.Name}");

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                Debug.Log(playerCount + " of " + PhotonNetwork.CurrentRoom.MaxPlayers);
                Debug.Log("Joined room that is waiting for more players...");

                if (SceneManager.GetActiveScene().name.ToUpper().Contains("NETWORK"))
                    Debug.Log("Connect another client to begin...");
            }
            else
            {
                Debug.Log("Joined room and the match is now ready to start...");

                OnJoinedRoomFinished?.Invoke();
                Debug.Log("Ready to start.. ");
            }

            GameObject myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
            PlayerController pCtrl = myPlayer.GetComponent<PlayerController>();
            //isReconnect = false;
        }

        private IEnumerator WaitingForPlayers()
        {
            readyToStart = false;
            while (PhotonNetwork.CurrentRoom != null && players.Count < PhotonNetwork.CurrentRoom.MaxPlayers)
                yield return null;

            if (PhotonNetwork.CurrentRoom == null)
                yield break;

            PhotonNetwork.LoadLevel("GameBoard");
            yield return PhotonNetwork._AsyncLevelLoadingOperation;
            yield return new WaitForSeconds(2f);

            readyToStart = true;
            OnReadyToStart.Invoke();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);

            Debug.LogError("JOIN RANDOM ROOM FAILED: " + returnCode + " " + message);
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

            if (isLeave)
            {
                PlayerPrefs.DeleteKey(_roomName);               
                StartCoroutine(GoBackToMainMenu());
            }            
        }

        private IEnumerator GoBackToMainMenu()
        {
            CanvasLoading.Instance.Show();
            yield return new WaitForSeconds(1f);
            PhotonNetwork.LocalPlayer.SetCustomProperties(null);

            if (SceneManager.GetActiveScene().buildIndex != 0) 
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(0);
                yield return op;
                isLeave = false;
                isReconnect = false;
                CanvasLoading.Instance.Hide();
            }
            else
                CanvasLoading.Instance.Hide();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            UpdatePlayerList();

            Debug.Log("OnPlayerEnteredRoom: " + newPlayer.NickName);

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                //PhotonNetwork.CurrentRoom.IsOpen = false;
                Debug.Log("Match is ready to begin.");

                //PhotonNetwork.LoadLevel("Testing");
            }
        }

        private void UpdatePlayerList()
        {
            players = PhotonNetwork.PlayerList.ToList();

            if (isReconnect)
            {
                ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;

                PlayerCustomValue playerCustomValue = JsonUtility.FromJson<PlayerCustomValue>(PlayerPrefs.GetString(_playerProperties));

                playerProperties["avoidSingleCards"] = playerCustomValue.avoidSingleCard;
                playerProperties["wallet"] = playerCustomValue.wallet;
                playerProperties["turnBank"] = playerCustomValue.turnBank;
                playerProperties["protectedFromSingleInRelationship"] = playerCustomValue.protectedFromSingleInRelationship;
                playerProperties["diceCount"] = playerCustomValue.diceCount;
                playerProperties["progress"] = playerCustomValue.progress;
                playerProperties["dateCount"] = playerCustomValue.dateCount;
                playerProperties["relationshipCount"] = playerCustomValue.relationshipCount;
                playerProperties["marriageCount"] = playerCustomValue.marriageCount;
                playerProperties["yearsElapsed"] = playerCustomValue.yearsElapsed;

                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
                isConnecting = false;
                CanvasLoading.Instance.Hide();
                photonView.RPC(nameof(PlayerReconnectGame), RpcTarget.Others);
                return;
            }

            foreach (Player player in players)
            {
                ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;

                playerProperties["avoidSingleCards"] = 0;
                playerProperties["wallet"] = 0;
                playerProperties["turnBank"] = 0;
                playerProperties["protectedFromSingleInRelationship"] = false;
                playerProperties["diceCount"] = 1;
                playerProperties["progress"] = 0f;
                playerProperties["dateCount"] = 0;
                playerProperties["relationshipCount"] = 0;
                playerProperties["marriageCount"] = 0;
                playerProperties["yearsElapsed"] = 0;

                player.SetCustomProperties(playerProperties);
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            OnPlayerLeft?.Invoke(otherPlayer);
            UpdatePlayerList();
            isReconnect = true;

            if (players.Count < PhotonNetwork.CurrentRoom.MaxPlayers && TurnManager.Instance.turnState != TurnManager.TurnState.GameOver)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;

                PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
                {
                    new PopupDialog.PopupButton()
                    {
                        text = "Return to Main Menu",
                        onClicked = () =>
                        {
                            isLeave = true;
                            isReconnect = false;
                            PhotonNetwork.LeaveRoom(true);
                            SceneManager.LoadScene(0);
                        },
                        buttonColor = PopupDialog.PopupButtonColor.Plain
                    },
                    new PopupDialog.PopupButton()
                    {
                        text = "Return to Main Menu",
                        onClicked = () =>
                        {
                            CanvasLoading.Instance.Show();
                        }
                    }
                };

                PopupDialog.Instance.Show(otherPlayer + " has left the game.", "The game will now end.", buttons);
            }
        }

        public void PlayerReconnectGame()
        {
            CanvasLoading.Instance.Hide();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
        }
        #endregion
    }
}