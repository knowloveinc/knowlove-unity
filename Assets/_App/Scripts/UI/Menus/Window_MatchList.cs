using DG.Tweening;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI.Menus
{
    public class Window_MatchList : Window, IMatchmakingCallbacks, ILobbyCallbacks
    {

        [SerializeField] private RefreshableScrollRect scrollRect;

        [SerializeField] private GameObject listPrefab;

        [SerializeField] private Window_CreateMatch createMatchWindow;

        [SerializeField] private Window_PickMode pickModeWindow;

        [SerializeField] private Window_WaitingForPlayers waitingWindow;

        [SerializeField] private Window_EnterMatchPassword enterPasswordWindow;

        [SerializeField] private GameObject noRoomsMessage;

        private void OnDestroy()
        {
            Hide();
        }

        public override void Show()
        {
            NetworkManager.OnPhotonConnected += this.GetRoomList;
            NetworkManager.OnRoomListUpdated += this.NetworkManager_OnRoomListUpdated;
            PopulateList();

            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
            NetworkManager.OnRoomListUpdated -= this.NetworkManager_OnRoomListUpdated;
            NetworkManager.OnPhotonConnected -= this.GetRoomList;
        }

        public void GoBack()
        {
            if (PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.Disconnect();

            Hide();
            pickModeWindow.Show();
        }

        public void GetRoomList()
        {
            TypedLobby sqlLobby = new TypedLobby("sqlLobby", LobbyType.Default);
            if (PhotonNetwork.GetCustomRoomList(sqlLobby, "live = 86"))
                Debug.LogWarning("GETTING ROOM LIST");
            else
            {
                Debug.Log("Failed to call get room list");
                scrollRect.EndRefreshing();
            }
        }

        public void OnRoomSelected(RoomInfo roomInfo)
        {
            Debug.Log("ROOM CLICKED: " + JsonConvert.SerializeObject(roomInfo));
            Debug.Log("Room has password? = " + roomInfo.CustomProperties.ContainsKey("password"));

            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
                new PopupDialog.PopupButton()
                {
                    text = "Join",
                    onClicked = () =>
                    {
                        if(roomInfo.CustomProperties.ContainsKey("password"))
                        {
                            Debug.Log("Showing password screen");
                            enterPasswordWindow.Init(roomInfo, (string)roomInfo.CustomProperties["password"]);
                            enterPasswordWindow.Show();
                        }
                        else
                        {
                            CanvasLoading.Instance.Show();

                            this.Hide();
                            waitingWindow.Show();

                            CanvasLoading.Instance.Hide();

                            NetworkManager.OnJoinedRoomFinished += this.NetworkManager_OnJoinedRoomFinished;

                            PhotonNetwork.JoinRoom(roomInfo.Name);
                        }
                    }
                },
                new PopupDialog.PopupButton()
                {
                    text = "Cancel",
                    onClicked = () =>
                    {
                        //Leave this empty, and the popup will simply close when this button is clicked.
                    }
                }
            };

            PopupDialog.Instance.Show("Join room?", $"Are you sure you want to join \"{roomInfo.Name}\"?", buttons);
        }

        private void NetworkManager_OnJoinedRoomFinished()
        {
            NetworkManager.OnJoinedRoomFinished -= this.NetworkManager_OnJoinedRoomFinished;
            CanvasLoading.Instance.Hide();
            waitingWindow.Show();
        }

        public void PopulateList()
        {
            Debug.Log("Populating room list...");

            if (scrollRect.content == null)
                return;

            CanvasLoading.Instance.Show();

            foreach (Transform child in scrollRect.content)
            {
                if (child.gameObject != scrollRect.loadingAnimatorParent && child.gameObject != noRoomsMessage)
                    Destroy(child.gameObject);
            }

            if (NetworkManager.Instance.roomList != null && NetworkManager.Instance.roomList.Count > 0)
            {
                int countRoom = 0;
                noRoomsMessage.SetActive(false);
                foreach (RoomInfo ri in NetworkManager.Instance.roomList)
                {
                    if (ri.MaxPlayers == ri.PlayerCount)
                        continue;

                    if (ri.PlayerCount == 0)
                    {
                        countRoom++;
                        continue;
                    }
                        

                    Debug.Log($"Room found: {ri.Name}");
                    Debug.Log($"Room Data: {JsonConvert.SerializeObject(ri)}");

                    GameObject obj = Instantiate(listPrefab, scrollRect.content);
                    TextMeshProUGUI roomTitle = obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    string pass = ri.CustomProperties.ContainsKey("password") ? "(" + ri.CustomProperties["password"] + ")" : "";
                    roomTitle.text = $"{ri.Name}";

                    TextMeshProUGUI playerCountLabel = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                    playerCountLabel.text = ri.PlayerCount + " / " + ri.MaxPlayers;// + " (password: " + (ri.CustomProperties.ContainsKey("password") ? ri.CustomProperties["password"] : "n/a") + ")";

                    //Show the lock icon if the room is password protected
                    Transform lockIcon = obj.transform.GetChild(0);
                    lockIcon.gameObject.SetActive(!string.IsNullOrEmpty(pass));

                    Button btn = obj.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();

                    if (!ri.IsOpen)
                    {
                        btn.interactable = false;
                        btn.colors = new ColorBlock() { disabledColor = new Color(0.3f, 0.1f, 0.1f) };
                    }

                    RoomInfo roomInfo = ri;
                    btn.onClick.AddListener(() =>
                    {
                        OnRoomSelected(roomInfo);
                    });
                }

                if(countRoom == NetworkManager.Instance.roomList.Count)
                    noRoomsMessage.SetActive(true);
            }
            else
            {
                noRoomsMessage.SetActive(true);
                Debug.LogWarning("There don't seem to be any rooms to join. Try creating one.");
            }

            CanvasLoading.Instance.Hide();
        }

        public void CreateRoom()
        {
            createMatchWindow.Show();
            //PhotonNetwork.CreateRoom(User.current.displayName, new RoomOptions() { MaxPlayers = 2 });
        }     

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            PopupDialog.Instance.Show("Create Game Failed", message);
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            PopupDialog.Instance.Show("Join Game Failed", message);
        }

        public void OnJoinedLobby()
        {
            throw new System.NotImplementedException();
        }

        public void OnLeftLobby()
        {
            throw new System.NotImplementedException();
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.LogWarning("RoomListUpdated");
            if(scrollRect != null)
                scrollRect.EndRefreshing();
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            throw new System.NotImplementedException();
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList) { }

        public void OnCreatedRoom() { }

        public void OnJoinedRoom() { }

        public void OnJoinRandomFailed(short returnCode, string message) { }

        public void OnLeftRoom() { }

        private void NetworkManager_OnRoomListUpdated()
        {
            DOVirtual.DelayedCall(0.25f, () =>
            {
                PopulateList();
                if (scrollRect != null)
                    scrollRect.EndRefreshing();
            });            
        }
    }
}
