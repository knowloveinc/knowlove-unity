using DG.Tweening;
using GameBrewStudios;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Window_MatchList : Window, IMatchmakingCallbacks, ILobbyCallbacks
{

    [SerializeField]
    RefreshableScrollRect scrollRect;

    [SerializeField]
    GameObject listPrefab;

    [SerializeField]
    Window_CreateMatch createMatchWindow;

    [SerializeField]
    Window_PickMode pickModeWindow;

    [SerializeField]
    Window_WaitingForPlayers waitingWindow;

    [SerializeField]
    Window_EnterMatchPassword enterPasswordWindow;

    public override void Show()
    {
        NetworkManager.OnPhotonConnected += this.GetRoomList;
        NetworkManager.OnRoomListUpdated += this.NetworkManager_OnRoomListUpdated;

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
        {
            PhotonNetwork.Disconnect();
        }

        Hide();
        pickModeWindow.Show();
        
    }

    private void OnDestroy()
    {
        Hide();
    }

    private void NetworkManager_OnRoomListUpdated()
    {
        PopulateList();
        scrollRect.EndRefreshing();
    }
    public void GetRoomList()
    {

        TypedLobby sqlLobby = new TypedLobby("sqlLobby", LobbyType.Default);
        if(PhotonNetwork.GetCustomRoomList(sqlLobby, "live = 86"))
        {
            Debug.LogWarning("GETTING ROOM LIST");
        }
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

    [SerializeField] GameObject noRoomsMessage;

    public void PopulateList()
    {
        Debug.Log("Populating room list...");
        CanvasLoading.Instance.Show();
        foreach (Transform child in scrollRect.content)
        {
            
            if(child.gameObject != scrollRect.loadingAnimatorParent && child.gameObject != noRoomsMessage)
                Destroy(child.gameObject);
        }

        if (NetworkManager.Instance.roomList != null && NetworkManager.Instance.roomList.Count > 0)
        {
            noRoomsMessage.SetActive(false);
            foreach (RoomInfo ri in NetworkManager.Instance.roomList)
            {
                Debug.Log($"Room found: {ri.Name}");
                Debug.Log($"Room Data: {JsonConvert.SerializeObject(ri)}");

                GameObject obj = Instantiate(listPrefab, scrollRect.content);
                TextMeshProUGUI roomTitle = obj.transform.Find("Label - Name").GetComponent<TextMeshProUGUI>();
                string pass = ri.CustomProperties.ContainsKey("password") ? "(" + ri.CustomProperties["password"] + ")" : "";
                roomTitle.text = $"{ri.Name}";

                TextMeshProUGUI playerCountLabel = obj.transform.Find("Label - PlayerCount").GetComponent<TextMeshProUGUI>();
                playerCountLabel.text = ri.PlayerCount + " / " + ri.MaxPlayers;// + " (password: " + (ri.CustomProperties.ContainsKey("password") ? ri.CustomProperties["password"] : "n/a") + ")";

                //Show the lock icon if the room is password protected
                Transform lockIcon = obj.transform.Find("Lock");
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

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        
    }

    public void OnCreatedRoom()
    {
        
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        PopupDialog.Instance.Show("Create Game Failed", message);
    }

    public void OnJoinedRoom()
    {
        
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        PopupDialog.Instance.Show("Join Game Failed", message);
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        
    }

    public void OnLeftRoom()
    {
        
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
        scrollRect.EndRefreshing();
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        throw new System.NotImplementedException();
    }
}
