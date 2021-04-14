using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI
{
    public class BottomPanel : MonoBehaviourPunCallbacks
    {
        public GameObject mapWindow;
        public GameObject mapButton;

        [SerializeField] private TurnManager TurnManager;
        [SerializeField] private RollDiceLogic _rollDiceLogic;

        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private Button bottomButton;
        [SerializeField] private TextMeshProUGUI bottomButtonLabel;

        [SerializeField] private GameObject _flipTheTableButton;

        private void Start()
        {
            RPC_HideBottomForPlayer();
            _flipTheTableButton.gameObject.SetActive(false);

            TurnManager.StartedTurn += ShowBottomForPlayer;
            TurnManager.GameOvered += HideBottomForEveryone;
        }

        private void OnDestroy()
        {
            TurnManager.StartedTurn -= ShowBottomForPlayer;
            TurnManager.GameOvered -= HideBottomForEveryone;
        }

        public void ShowBottomForPlayer(int index)
        {
            Debug.Log("NetworkManager.Instance.players.Count = " + NetworkManager.Instance.players.Count);

            for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
            {
                if (i == index)
                {
                    Debug.Log("Trying to show bottom for " + NetworkManager.Instance.players[i].NickName);
                    photonView.RPC(nameof(RPC_ShowBottomForPlayer), NetworkManager.Instance.players[i]);
                }
                else
                    photonView.RPC(nameof(RPC_HideBottomForPlayer), NetworkManager.Instance.players[i]);
            }
        }

        public void HideBottomForEveryone()
        {
            photonView.RPC(nameof(RPC_HideBottomForPlayer), RpcTarget.All);
        }

        [PunRPC]
        public void RPC_ShowBottomForPlayer()
        {
            photonView.RPC(nameof(RPC_SetActiveFlipTheTableButton), RpcTarget.All, true);

            mapWindow.SetActive(false);
            mapButton.SetActive(true);

            Debug.Log("SHOWING BOTTOM");
            bottomPanel.anchoredPosition = new Vector2(0, -bottomPanel.sizeDelta.y);
            bottomButton.interactable = false;
            bottomButton.onClick.RemoveAllListeners();

            ExitGames.Client.Photon.Hashtable playerProps = NetworkManager.Instance.players[TurnManager.turnIndex].CustomProperties;
            int diceCount = (int)playerProps["diceCount"];
            NetworkManager.Instance.players[TurnManager.turnIndex].SetCustomProperties(playerProps);

            bottomButton.onClick.AddListener(() =>
            {
                photonView.RPC(nameof(RPC_SetActiveFlipTheTableButton), RpcTarget.All, false);

                mapWindow.SetActive(false);
                mapButton.SetActive(false);
                _rollDiceLogic.RollDice(diceCount, "board");

                bottomButton.interactable = false;
                bottomPanel.DOAnchorPosY(-bottomPanel.sizeDelta.y, 0.5f);
            });

            bottomButtonLabel.text = "Roll";

            bottomPanel.DOAnchorPosY(0f, 0.5f).OnComplete(() =>
            {
                bottomButton.interactable = true;
            });
        }

        [PunRPC]
        private void RPC_HideBottomForPlayer()
        {
            Debug.Log("HIDING BOTTOM");
            mapWindow.SetActive(false);
            mapButton.SetActive(true);
            bottomPanel.DOAnchorPosY(-bottomPanel.sizeDelta.y, 0.5f);
        }

        [PunRPC]
        private void RPC_SetActiveFlipTheTableButton(bool isActive)
        {
            _flipTheTableButton.gameObject.SetActive(isActive);
        }
    }
}
