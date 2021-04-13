using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI.Menus
{
    public class Window_CreateMatchWizard : Window
    {
        public GameObject[] wizardPanels;

        public int wizardIndex = 0;

        [SerializeField] private Window_WaitingForPlayers waitingForPlayersWindow;

        [SerializeField] private TMP_InputField roomNameInputField, passwordInputField;

        [SerializeField] private Button[] playerCountButtons;

        [SerializeField] private Color buttonSelectedColor, buttonNormalColor;


        [SerializeField, Header("PROPERTIES")] private string roomName, password;
        [SerializeField] private int playerCount = 2;

        public override void Show()
        {
            roomName = PhotonNetwork.NickName + "'s Room";
            roomNameInputField.SetTextWithoutNotify(roomName);
            password = "";

            passwordInputField.SetTextWithoutNotify(password);

            SetPlayerCount(2, false);
            wizardIndex = -1;

            NextWizard();
            base.Show();
        }

        public void SetPlayerCount(int count)
        {
            SetPlayerCount(count, true);
        }

        public void SetPlayerCount(int count, bool nextWizard)
        {
            playerCount = count;

            for (int i = 0; i < 3; i++)
            {
                if (playerCount - 2 == i)
                    playerCountButtons[i].targetGraphic.color = buttonSelectedColor;
                else
                    playerCountButtons[i].targetGraphic.color = buttonNormalColor;
            }

            if (nextWizard)
                NextWizard();
        }

        public void PreviousWizard()
        {
            --wizardIndex;

            if (wizardIndex < 0)
                wizardIndex = 0;

            for (int i = 0; i < wizardPanels.Length; i++)
            {
                wizardPanels[i].SetActive(i == wizardIndex);
            }
        }

        public void NextWizard()
        {
            ++wizardIndex;

            for (int i = 0; i < wizardPanels.Length; i++)
            {
                wizardPanels[i].SetActive(i == wizardIndex);
            }
        }

        public void SetRoomName(string roomName)
        {
            this.roomName = roomName;
        }

        public void SetPassword(string password)
        {
            this.password = password;
        }

        public void DoCreateMatch()
        {

            if (string.IsNullOrEmpty(roomName))
            {
                PopupDialog.Instance.Show("Please enter a valid name for the room.");
                return;
            }

            bool hasPassword = !string.IsNullOrEmpty(password);

            RoomOptions options = new RoomOptions();
            options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { };

            if (hasPassword)
            {
                options.CustomRoomProperties.Add("password", password);
                options.CustomRoomPropertiesForLobby = new string[] { "password" };
            }

            options.MaxPlayers = Convert.ToByte(playerCount);

            options.IsVisible = true;
            options.IsOpen = options.MaxPlayers > 1;
            options.PlayerTtl = 33;
            options.PublishUserId = true;

            TypedLobby sqlLobby = new TypedLobby("sqlLobby", LobbyType.Default);

            if (PhotonNetwork.CreateRoom(roomName, options, sqlLobby))
            {
                this.Hide();
                waitingForPlayersWindow.Show();
            }
            else
                PopupDialog.Instance.Show("Create Game Failed", "An error occured while creating your game. Please try again.");
        }
    }
}

