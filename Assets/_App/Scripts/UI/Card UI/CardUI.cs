using DG.Tweening;
using Knowlove.ActionAndPathLogic;
using Lean.Touch;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI
{
    public class CardUI : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TurnManager TurnManager;
        [SerializeField] private BottomPanel _bottomPanel;
        [SerializeField] private GamePrompt _gamePrompt;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;

        [SerializeField] private RectTransform cardUIObj;
        [SerializeField] private Button cardUIButton;
        [SerializeField] private TextMeshProUGUI cardUIText;

        private string _waitingClickFromUserNickName = null;

        public System.Action onCardClicked = null;

        private void Start()
        {
            _pathNodeActionLogic.PickedCardScenario += ShowCard;
        }

        private void OnDestroy()
        {
            _pathNodeActionLogic.PickedCardScenario -= ShowCard;
        }

        public void ShowCard(CardData card)
        {
            //Only the master can call this... All players should then get the RPC to show the card
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("DO NOT RUN ShowCard() ON NON-MASTER CLIENTS");
                return;
            }

            if (card.isPrompt)
            {
                onCardClicked = () =>
                {
                    Debug.Log("CARD CLICK RECIEVED ON MASTER, RUNNING ShowPrompt for current user");
                    _gamePrompt.ShowPrompt(card.promptMessage, GetPromptButtons(card.promptButtons), NetworkManager.Instance.players[TurnManager.turnIndex], 0, false);
                };
            }
            else
            {
                onCardClicked = () =>
                {
                    Debug.Log("CARD CLICK RECIEVED ON MASTER, RUNNING HandlePathNodeAction");
                    TurnManager.PathNodeActionLogic.HandlePathNodeAction(card.action, card.parentheses, card.rollCheck, card.rollPassed, card.rollFailed, true);
                };
            }

            _waitingClickFromUserNickName = NetworkManager.Instance.players[TurnManager.turnIndex].NickName;
            foreach (Player player in NetworkManager.Instance.players)
            {
                Debug.LogWarning("-------");
                Debug.Log("Player Nickname: " + player.NickName);
                Debug.LogWarning("Player ID: " + player.UserId);
            }
            Debug.Log("Set waiting User ID to " + _waitingClickFromUserNickName);

            bool isFancyCard = card.action == PathNodeAction.AdvanceToRelationshipWithProtectionFromSingle;

            photonView.RPC(nameof(RPC_ShowCard), RpcTarget.All, card.text + "(" + card.parentheses + ")", NetworkManager.Instance.players[TurnManager.turnIndex], (int)BoardManager.Instance.pieces[TurnManager.turnIndex].pathRing, isFancyCard);
        }

        [PunRPC]
        private void RPC_ShowCard(string cardText, Player targetPlayer, int pathIndex, bool isFancyCard)
        {
            _bottomPanel.mapWindow.SetActive(false);
            _bottomPanel.mapButton.SetActive(false);

            Image cardImage = cardUIObj.transform.Find("Mask").GetComponent<Image>();
            cardImage.color = isFancyCard ? new Color(104 / 255f, 54 / 255f, 149 / 255f) : Color.white;

            cardUIObj.transform.Find("Mask/silhouette").gameObject.SetActive(isFancyCard);

            cardUIObj.anchoredPosition = new Vector2(cardUIObj.anchoredPosition.x, -1080f);

            cardUIText.text = cardText;

            cardUIText.color = isFancyCard ? Color.white : Color.black;

            //cardUIButton.onClick.RemoveAllListeners();
            cardUIButton.enabled = false;

            LeanFingerTap lft = cardUIObj.transform.Find("Mask").GetComponent<LeanFingerTap>();
            lft.OnFinger.RemoveAllListeners();

            LeanPinchScale lps = cardUIObj.transform.Find("Mask").GetComponent<LeanPinchScale>();
            lps.transform.localScale = Vector3.one;

            LeanSelectable ls = cardUIObj.transform.Find("Mask").GetComponent<LeanSelectable>();
            ls.Select();

            //Only make the card clickable for the user who is taking their turn right now.
            if (PhotonNetwork.LocalPlayer.NickName == targetPlayer.NickName)
            {

                lft.OnFinger.AddListener((finger) =>
                {
                    Debug.Log("Attempted tap: " + finger.Index + " - " + LeanTouch.Fingers.Count);
                    ls.Deselect();
                    OnCardClicked();
                });

                //cardUIButton.onClick.AddListener(() =>
                //{
                //    OnCardClicked();
                //});
            }

            CanvasGroup scenarioTextGroup = cardUIObj.Find("SCENARIO").GetComponent<CanvasGroup>();
            TextMeshProUGUI scenarioText = scenarioTextGroup.GetComponent<TextMeshProUGUI>();
            string scenarioTextStr = PhotonNetwork.LocalPlayer.NickName.ToLower() != targetPlayer.NickName.ToLower() ? $"<size=30>{targetPlayer.NickName.Replace("[host]", "").Trim()} Got A</size>\n" : "<size=30>You Got A</size>\n";

            scenarioTextStr += (pathIndex == 0 ? "DATING" : (pathIndex == 1 ? "RELATIONSHIP" : (pathIndex == 2 ? "MARRIAGE" : ""))) + " SCENARIO";

            scenarioText.text = scenarioTextStr;
            scenarioTextGroup.alpha = 0f;
            scenarioTextGroup.DOFade(1f, 0.25f).OnComplete(() =>
            {
                //Bring the card up onto the screen
                cardUIObj.DOAnchorPosY(0f, 0.25f).SetDelay(1f).OnComplete(() =>
                {
                    CanvasGroup cg = cardUIObj.GetComponent<CanvasGroup>();
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    Debug.Log("Card fully visible.");
                    scenarioTextGroup.alpha = 0f;
                });
            });
        }

        public void OnCardClicked()
        {
            Debug.Log("CLICKED CARD: Calling RPC NOW");
            photonView.RPC(nameof(RPC_OnCardClicked), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);

            //Go ahead and hide the card immediately for the player who clicked on it, incase there is any delay in the response returning to them
            HideCard();
        }

        [PunRPC]
        private void RPC_OnCardClicked(string userNickname)
        {
            Debug.Log("RPC_OnCardClicked()");

            Debug.Log(_waitingClickFromUserNickName);

            if (PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(_waitingClickFromUserNickName) && userNickname == _waitingClickFromUserNickName)
            {
                //The correct user has clicked and we are running on the master client, execute the proper response from the cache
                onCardClicked?.Invoke();

                if (userNickname == _waitingClickFromUserNickName)
                    photonView.RPC(nameof(RPC_HideCard), RpcTarget.All);
            }
            //If the user who clicked is the one we were waiting on, go ahead and hide the card for everyone.
        }

        [PunRPC]
        public void RPC_HideCard()
        {
            HideCard();
        }

        private void HideCard()
        {
            CanvasGroup cg = cardUIObj.GetComponent<CanvasGroup>();

            if (cg.interactable == false) return; //This means we already started hiding it and dont need to do it again.

            cg.interactable = false;
            cg.blocksRaycasts = false;

            cardUIObj.DOAnchorPosY(-1080f, 0.25f).OnComplete(() =>
            {
                Debug.Log("Card fully hidden.");
            });
        }

        public PopupDialog.PopupButton[] GetPromptButtons(CardPromptButton[] cardButtons)
        {
            List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();
            for (int i = 0; i < cardButtons.Length; i++)
            {
                ProceedAction action = cardButtons[i].action;
                PopupDialog.PopupButton btn = new PopupDialog.PopupButton()
                {
                    text = cardButtons[i].text,
                    onClicked = () =>
                    {
                        //GameManager.Instance.pieces[turnIndex].GoHome();
                        TurnManager.Instance.ProceedActionLogic.ExecuteProceedAction(action, () => { });
                    },
                    buttonColor = PopupDialog.PopupButtonColor.Plain
                };

                buttons.Add(btn);
            }

            return buttons.ToArray();
        }
    }
}
