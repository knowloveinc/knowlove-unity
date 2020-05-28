using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    [SerializeField]
    RoomInfo roomInfo;

    [SerializeField]
    TextMeshProUGUI roomLabel, playerCountLabel;

    internal void Init(RoomInfo info)
    {

        this.roomInfo = info;

        roomLabel.text = "(" + roomInfo.masterClientId + ") " + roomInfo.Name;

        playerCountLabel.text = info.PlayerCount + " / " + info.MaxPlayers;

        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        bool hasPassword = info.CustomProperties.ContainsKey("password") && !string.IsNullOrEmpty((string)info.CustomProperties["password"]);

        if (info.RemovedFromList)
        {
            button.interactable = false;
            roomLabel.text += " <i>(Game ending...)</i>";
        }

        if(hasPassword)
        {
            roomLabel.text += " (" + (string)info.CustomProperties["password"] + ")";
        }

        button.onClick.AddListener(() =>
        {
            PhotonNetwork.JoinRoom(roomInfo.Name);
        });
    }
}
