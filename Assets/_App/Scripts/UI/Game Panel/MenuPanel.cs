using DG.Tweening;
using Photon.Pun;
using UnityEngine;

namespace Knowlove.UI
{
    public class MenuPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform menuRect;

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
                        NetworkManager.Instance.isLeave = true;
                        NetworkManager.Instance.isReconnect = false;
                        PhotonNetwork.LeaveRoom();
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
    }
}
