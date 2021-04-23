using DG.Tweening;
using GameBrewStudios;
using Knowlove.XPSystem;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Knowlove.UI.Menus
{
    public class Window_PickMode : Window
    {
        [SerializeField] private Window_Multiplayer multiplayerWindow;

        [SerializeField] private Window settingsWindow, myStuffWindow;

        [SerializeField] private Window_WaitingForPlayers waitingWindow;

        [SerializeField] private RankImage _rankImage;

        public override void Show()
        {
            base.Show();
            
            DOVirtual.DelayedCall(0.75f, () =>
            {
                _rankImage.ChangeRank();
            });

            if (!PlayerPrefs.HasKey("IsSaveDate") && User.current != null)
            {
                DOVirtual.DelayedCall(2f, () =>
                {
                    InfoPlayer.Instance.FromJSONPlayerInfo();
                });
            }
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void OpenStore()
        {
            StoreController.Show();
        }

        public void OpenMyStuff()
        {
            myStuffWindow.Show();
        }

        public void OpenSettings()
        {
            settingsWindow.Show();
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
                        
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            SceneManager.LoadScene(0);
                            CanvasLoading.Instance.Hide();
                        });
                    },
                    buttonColor = PopupDialog.PopupButtonColor.Red
                },
                new PopupDialog.PopupButton()
                {
                    text = "Cancel",
                    onClicked = () => { },
                    buttonColor = PopupDialog.PopupButtonColor.Plain
                }
            };

            PopupDialog.Instance.Show("Sign Out", "Are you sure you want to sign out? You will be returned to the login screen.", buttons);
        }

        public void StartSinglePlayer()
        {
            Debug.Log("StartSinglePlayer()");

            if(PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.Disconnect();

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

            NetworkManager.OnPhotonConnected += this.NetworkManager_OnPhotonConnectedSinglePlayer;
            NetworkManager.Instance.Connect();
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
    }
}
