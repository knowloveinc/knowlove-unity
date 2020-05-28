using GameBrewStudios;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Window_WaitingForPlayers : Window
{

    [SerializeField]
    Window_MatchList matchListWindow;

    [SerializeField]
    TextMeshProUGUI statusLabel;

    [SerializeField]
    Button cancelButton;

    public override void Show()
    {
        base.Show();
        cancelButton.gameObject.SetActive(false);
    }

    private void Update()
    {

        string status = "Connecting to room..";

        if (PhotonNetwork.CurrentRoom != null)
        {
            status += PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + " Players in room.";
            status += "\nOnline Game: " + (!PhotonNetwork.CurrentRoom.IsOffline).ToString();
            status += "\n" + PhotonNetwork.CurrentRoom.CustomProperties.ToStringFull();
            cancelButton.gameObject.SetActive(true);
        }

        statusLabel.text = "Game created.\n" + status;
    }

    public override void Hide()
    {
        base.Hide();

        if(PhotonNetwork.LeaveRoom(false))
        {
            matchListWindow.Show();
        }

    }
}
