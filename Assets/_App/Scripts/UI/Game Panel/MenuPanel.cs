using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Knowlove.UI
{
    public class MenuPanel : MonoBehaviourPunCallbacks
    {
        [SerializeField] private RectTransform menuRect;

        private void OnApplicationQuit()
        {
            photonView.RPC(nameof(LeaveRoom), RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer);
        }

        public void CloseMenu()
        {
            menuRect.DOAnchorPosX(-menuRect.sizeDelta.x, 0.2f);
        }

        public void OpenMenu()
        {
            menuRect.DOAnchorPosX(0, 0.2f);
        }

        public void LeaveMatch()
        {
            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
                new PopupDialog.PopupButton()
                {
                    text = "Yes, Leave",
                    onClicked = () =>
                    {
                        photonView.RPC(nameof(LeaveRoom), RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer);
                    },
                    buttonColor = PopupDialog.PopupButtonColor.Red
                },
                new PopupDialog.PopupButton()
                {
                    text = "Nevermind",
                    onClicked = () => { }
                }
            };

            PopupDialog.Instance.Show("Really Leave Game?", "If you leave this match, it will end the game for all players. Are you sure you want to leave? (Please be considerate of others.)", buttons);
        }

        [PunRPC]
        private void LeaveRoom(Player player)
        {
            NetworkManager.Instance.isLeave = true;
            NetworkManager.Instance.isReconnect = false;

            if(PhotonNetwork.LocalPlayer == player)
                PhotonNetwork.LeaveRoom();
        }
    }
}
