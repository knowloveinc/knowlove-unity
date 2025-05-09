﻿using GameBrewStudios;
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


    int ellipsesCount = 0;
    float lastEllipsesChange = 0f;

    private void Update()
    {

        string status = "Please Wait..\n";

        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.MaxPlayers > 1)
            {
                status = "Waiting for Players";
                for(int i = 0; i < ellipsesCount; i++)
                {
                    status += ".";
                }

                if (Time.time - lastEllipsesChange >= 1f)
                {
                    lastEllipsesChange = Time.time;
                    ellipsesCount++;
                    if (ellipsesCount > 3)
                        ellipsesCount = 0;
                }

                status += "\n" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
                
                if(PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
                    cancelButton.gameObject.SetActive(true);
            }
            else
            {
                status = "Loading, Please Wait...";
            }
            //status += "\nOnline Game: " + (!PhotonNetwork.CurrentRoom.IsOffline).ToString();
            //status += "\n" + PhotonNetwork.CurrentRoom.CustomProperties.ToStringFull();
        }

        statusLabel.text = status;
    }

    public override void Hide()
    {
        base.Hide();

        

    }

    public void CancelMatchmaking()
    {
        Hide();
        if (PhotonNetwork.LeaveRoom(false))
        {
            matchListWindow.Show();
        }
    }
}
