using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Window_CreateMatch : Window
{
    [SerializeField]
    TMP_InputField roomNameField, passwordField;

    [SerializeField]
    Window_MatchList matchListWindow;

    [SerializeField]
    Window_WaitingForPlayers waitingForPlayersWindow;

    [SerializeField]
    TMP_Dropdown playerCountDropdown;

    public override void Show()
    {
        base.Show();
        roomNameField.text = PhotonNetwork.NickName + "'s Room";
        passwordField.text = "";
        playerCountDropdown.value = 0;
    }


    public void DoCreateMatch()
    {

        bool hasPassword = !string.IsNullOrEmpty(passwordField.text);

        RoomOptions options = new RoomOptions();
        if(hasPassword)
        {
            options.CustomRoomProperties.Add("password", passwordField.text);
        }

        options.MaxPlayers = Convert.ToByte(PlayerCountFromDropdown());

        options.IsVisible = true;
        options.IsOpen = options.MaxPlayers > 1;

        options.PublishUserId = true;

        if(options.MaxPlayers == 1)
        { 
            PhotonNetwork.OfflineMode = true; 
        }
        else
        {
            PhotonNetwork.OfflineMode = false;
        }

        if (PhotonNetwork.CreateRoom(roomNameField.text, options, null))
        {
            
            matchListWindow.Hide();
            waitingForPlayersWindow.Show();
            this.Hide();
        }
        else
        {
            PopupDialog.Instance.Show("Create Game Failed", "An error occured while creating your game. Please try again.");
        }
    }

    int PlayerCountFromDropdown()
    {
        /*
            Options are:
                Single Player = 1,
                Two Players = 2,
                Three Players = 3,
                Four Players = 4
             
        */

        return Mathf.Clamp(playerCountDropdown.value + 1, 1, 4);
    }
}
