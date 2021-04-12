using Knowlove.ActionAndPathLogic;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Knowlove.UI
{
    public class TopPanel : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TurnManager TurnManager;
        [SerializeField] private RollDiceLogic _rollDiceLogic;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;
        [SerializeField] private MoveBoardPiece _moveBoardPiece;

        [SerializeField] private RectTransform topPanel;
        [SerializeField] private TextMeshProUGUI topText, topTitle;

        private void Start()
        {
            TurnManager.StartedAndEndedTurn += SetTopText;
            _rollDiceLogic.DiceFinishedRoll += SetTopText;
            _pathNodeActionLogic.WentToKnowLove += SetTopText;
            _moveBoardPiece.ShowedStatePiece += SetTopText;
        }

        private void OnDestroy()
        {
            TurnManager.StartedAndEndedTurn -= SetTopText;
            _rollDiceLogic.DiceFinishedRoll -= SetTopText;
            _pathNodeActionLogic.WentToKnowLove -= SetTopText;
            _moveBoardPiece.ShowedStatePiece -= SetTopText;
        }

        public void SetTopText(string text, string title = "TURN")
        {
            photonView.RPC(nameof(RPC_SetTopText), RpcTarget.All, text, title);
        }

        [PunRPC]
        public void RPC_SetTopText(string text, string title)
        {
            topText.text = text;
            topTitle.text = title;

            if (topTitle.text.ToLower().Contains("turn") && topText.text.ToLower().Contains(PhotonNetwork.LocalPlayer.NickName.ToLower()))
                topText.text = "YOUR TURN";

            if (topText.text.ToLower().Contains("rolled a "))
                topText.text = topText.text.Replace(PhotonNetwork.LocalPlayer.NickName, "You");

            topText.text.Replace("[host]", "");
        }
    }
}
