using GameBrewStudios;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Knowlove.UI.Menus
{
    public class Window_EnterMatchPassword : Window
    {
        public RoomInfo roomToJoin;

        private const string _prefPassword = "roomPassword";

        public string password;

        [SerializeField] private TMP_InputField passwordField;

        [SerializeField] private Window_WaitingForPlayers waitingWindow;

        [SerializeField] private Window_MatchList _windowMatchList;

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

                PlayerPrefs.SetString(_prefPassword, password);
                Hide();
                _windowMatchList.Hide();
                waitingWindow.Show();

                CanvasLoading.Instance.Hide();

                NetworkManager.OnJoinedRoomFinished += this.NetworkManager_OnJoinedRoomFinished;
                PhotonNetwork.JoinRoom(this.roomToJoin.Name);
            }
            else
                PopupDialog.Instance.Show("Incorrect password. Try again.");
        }

        private void NetworkManager_OnJoinedRoomFinished()
        {
            NetworkManager.OnJoinedRoomFinished -= this.NetworkManager_OnJoinedRoomFinished;
            waitingWindow.Show();
            Hide();
            CanvasLoading.Instance.Hide();
        }
    }
}
