using DG.Tweening;
using GameBrewStudios;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Knowlove.UI.Menus
{
    public class Window_PickMode : Window
    {
        [SerializeField]
        Window_Multiplayer multiplayerWindow;

        [SerializeField]
        Window settingsWindow, myStuffWindow;

        [SerializeField]
        Window_WaitingForPlayers waitingWindow;


        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void SignOut()
        {
            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
            new PopupDialog.PopupButton()
            {
                text = "Sign Out",
                onClicked = () =>
                {
                    CanvasLoading.Instance.Show();
                    User.current = null;
                    SceneManager.LoadScene(0);
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        CanvasLoading.Instance.Hide();
                    });
                },
                buttonColor = PopupDialog.PopupButtonColor.Red
            },
            new PopupDialog.PopupButton()
            {
                text = "Cancel",
                onClicked = () =>
                {

                },
                buttonColor = PopupDialog.PopupButtonColor.Plain
            }
            };

            PopupDialog.Instance.Show("Sign Out", "Are you sure you want to sign out? You will be returned to the login screen.", buttons);
        }

        public void StartSinglePlayer()
        {
            Debug.Log("StartSinglePlayer()");

            //if(PhotonNetwork.IsConnectedAndReady)
            //    PhotonNetwork.Disconnect();

            //PhotonNetwork.OfflineMode = true;

            //RoomOptions options = new RoomOptions()
            //{
            //    PublishUserId = true,
            //    MaxPlayers = 1,
            //    IsOpen = false,
            //    IsVisible = false,
            //    EmptyRoomTtl = 0
            //};
            //CanvasLoading.Instance.Show();
            //PhotonNetwork.CreateRoom("OfflineMode", options);
            CanvasLoading.Instance.Show();

            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.CurrentLobby != null)
            {
                CanvasLoading.Instance.Hide();
                Hide();
                multiplayerWindow.Show();
            }
            else
            {
                NetworkManager.OnPhotonConnected += this.NetworkManager_OnPhotonConnectedSinglePlayer;
                NetworkManager.Instance.Connect();
            }

        }

        public void StartMultiplayer()
        {
            Debug.Log("StartMultiplayer()");
            CanvasLoading.Instance.Show();
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.CurrentLobby != null)
            {
                CanvasLoading.Instance.Hide();
                Hide();
                multiplayerWindow.Show();
            }
            else
            {
                NetworkManager.OnPhotonConnected += this.NetworkManager_OnPhotonConnected;
                NetworkManager.Instance.Connect();
            }

        }

        public void OpenSettings()
        {
            settingsWindow.Show();
        }

        private void NetworkManager_OnPhotonConnected()
        {
            NetworkManager.OnPhotonConnected -= this.NetworkManager_OnPhotonConnected;
            CanvasLoading.Instance.Hide();
            Hide();
            multiplayerWindow.Show();
        }

        private void NetworkManager_OnPhotonConnectedSinglePlayer()
        {
            NetworkManager.OnPhotonConnected -= this.NetworkManager_OnPhotonConnectedSinglePlayer;
            CanvasLoading.Instance.Hide();
            Hide();

            RoomOptions options = new RoomOptions()
            {
                PublishUserId = true,
                MaxPlayers = 1,
                IsOpen = false,
                IsVisible = false,
                EmptyRoomTtl = 0
            };

            PhotonNetwork.CreateRoom("OfflineMode" + UnityEngine.Random.Range(9, 999999), options);
            waitingWindow.Show();
        }

        public void OpenStore()
        {
            StoreController.Show();
        }

        public void OpenMyStuff()
        {
            myStuffWindow.Show();
        }
    }
}
