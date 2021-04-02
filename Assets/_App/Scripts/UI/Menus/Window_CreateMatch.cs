using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;

namespace Knowlove.UI.Menus
{
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
            options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { };

            if (hasPassword)
            {
                options.CustomRoomProperties.Add("password", passwordField.text);
                options.CustomRoomPropertiesForLobby = new string[] { "password" };
            }

            options.MaxPlayers = Convert.ToByte(PlayerCountFromDropdown());

            options.IsVisible = true;
            options.IsOpen = options.MaxPlayers > 1;
            options.PlayerTtl = 33;
            options.PublishUserId = true;

            TypedLobby sqlLobby = new TypedLobby("sqlLobby", LobbyType.Default);


            if (PhotonNetwork.CreateRoom(roomNameField.text, options, sqlLobby))
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
                    Two Players = 0,
                    Three Players = 1,
                    Four Players = 2

            */

            return Mathf.Clamp(playerCountDropdown.value + 2, 2, 4);
        }
    }
}
