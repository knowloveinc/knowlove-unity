using GameBrewStudios;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Window_EnterMatchPassword : Window
{
    public string password;

    public RoomInfo roomToJoin;

    [SerializeField]
    TMP_InputField passwordField;

    [SerializeField]
    Window_WaitingForPlayers waitingWindow;

    public override void Show()
    {
        base.Show();
        passwordField.text = "";
    }
    public void Init(RoomInfo room, string password)
    {
        roomToJoin = room;
        this.password = password;
    }

    public void Join()
    {
        if (password == passwordField.text)
        {
            CanvasLoading.Instance.Show();

            NetworkManager.OnJoinedRoomFinished += this.NetworkManager_OnJoinedRoomFinished;
            PhotonNetwork.JoinRoom(this.roomToJoin.Name);
        }
        else
        {
            PopupDialog.Instance.Show("Incorrect password. Try again.");
        }
    }


    private void NetworkManager_OnJoinedRoomFinished()
    {
        NetworkManager.OnJoinedRoomFinished -= this.NetworkManager_OnJoinedRoomFinished;
        waitingWindow.Show();
        Hide();
        CanvasLoading.Instance.Hide();
    }
}
