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

public class Window_MatchList : Window
{

    [SerializeField]
    Transform container;

    [SerializeField]
    GameObject listPrefab;

    [SerializeField]
    Window_CreateMatch createMatchWindow;

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

    private void OnDestroy()
    {
        Hide();
    }

    private void NetworkManager_OnRoomListUpdated()
    {
        PopulateList();
    }
    public void GetRoomList()
    {
        Debug.LogWarning("PHOTON IS READY TO GO, GETTING ROOM LIST");
        PhotonNetwork.GetCustomRoomList(null, null);
    }

    public void OnRoomSelected(RoomInfo roomInfo)
    {
        Debug.Log("ROOM CLICKED: " + JsonConvert.SerializeObject(roomInfo));

        PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
        {
            new PopupDialog.PopupButton()
            {
                text = "Join",
                onClicked = () => 
                {
                    PhotonNetwork.JoinRoom(roomInfo.Name);
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

    public void PopulateList()
    {
        Debug.Log("Populating room list...");
        CanvasLoading.Instance.Show();
        foreach (Transform child in container)
            Destroy(child.gameObject);

        if (NetworkManager.Instance.roomList != null && NetworkManager.Instance.roomList.Count > 0)
        {
            foreach (RoomInfo ri in NetworkManager.Instance.roomList)
            {
                Debug.Log($"Room found: {ri.Name}");

                GameObject obj = Instantiate(listPrefab, container);
                TextMeshProUGUI roomTitle = obj.transform.Find("Label - Name").GetComponent<TextMeshProUGUI>();
                string pass = ri.CustomProperties.ContainsKey("password") ? "(" + ri.CustomProperties["password"] + ")" : "";
                roomTitle.text = $"{ri.Name} {pass}";

                TextMeshProUGUI playerCountLabel = obj.transform.Find("Label - PlayerCount").GetComponent<TextMeshProUGUI>();
                playerCountLabel.text = ri.PlayerCount + " / " + ri.MaxPlayers;

                //Show the lock icon if the room is password protected
                Transform lockIcon = obj.transform.Find("Lock");
                lockIcon.gameObject.SetActive(!string.IsNullOrEmpty(pass));

                Button btn = obj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();

                RoomInfo roomInfo = ri;
                btn.onClick.AddListener(() =>
                {
                    OnRoomSelected(roomInfo);
                });


            }
        }
        else
        {
            Debug.LogWarning("There don't seem to be any rooms to join. Try creating one.");
        }

        CanvasLoading.Instance.Hide();
    }


    public void CreateRoom()
    {
        createMatchWindow.Show();   
        //PhotonNetwork.CreateRoom(User.current.displayName, new RoomOptions() { MaxPlayers = 2 });
    }

}
