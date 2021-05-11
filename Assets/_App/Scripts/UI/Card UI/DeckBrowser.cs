using DG.Tweening;
using GameBrewStudios;
using GameBrewStudios.Networking;
using Knowlove.UI.Menus;
using Knowlove.XPSystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI
{
    public class DeckBrowser : Window
    {
        public static DeckBrowser Instance;

        private List<DeckData> decks = new List<DeckData>();
        private DeckData currentDeck;

        [SerializeField] private TextMeshProUGUI cardTextLabel;

        [SerializeField] private Image cardImage;

        [SerializeField] private GameObject cardSilhouetteObj;

        [SerializeField] private Color normalCardColor, specialCardColor;

        [SerializeField] private TextMeshProUGUI statusLabel;

        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private TextMeshProUGUI walletLabels;

        [SerializeField] private GameObject _lockImage;
        [SerializeField] private GameObject[] _buySeeImage;

        private int currentDeckIndex = 0;
        private string cardType;
        private bool _isLock;

        private int _costCard = 100;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            this.Hide();
            LoadCards();

            _lockImage.SetActive(false);
        }

        public override void Show()
        {
            throw new System.NotSupportedException("Show() is not valid for this object. Use Show(deckName) instead.");
        }

        public void Show(string deckName)
        {
            cardType = deckName;
            cardImage.color = normalCardColor;
            cardTextLabel.text = "";
            cardSilhouetteObj.SetActive(false);

            currentDeck = decks.FirstOrDefault(x => x.name.ToLower() == deckName.ToLower());
            currentDeckIndex = 0;

            if (currentDeck != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                base.Show();
                ChangeCard(0);
            }
            else
                PopupDialog.Instance.Show("Something went wrong.");

            UpdateText();
        }

        public override void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            base.Hide();
        }

        [ContextMenu("Load Card Data")]
        public void LoadCards()
        {
            TextAsset ta = Resources.Load<TextAsset>("CardData");
            decks = JsonConvert.DeserializeObject<List<DeckData>>(ta.text);

            Debug.Log("LOADED DECKS FOR DECK BROWSER");
        }

        public void ChangeCard(int direction)
        {
            currentDeckIndex += direction;

            if (currentDeckIndex < 0)
                currentDeckIndex = currentDeck.cards.Count - 1;

            if (currentDeckIndex >= currentDeck.cards.Count)
                currentDeckIndex = 0;

            UpdateStatusText();

            ShowCard();
        }

        public void UpdateStatusText()
        {
            statusLabel.text = currentDeck.name + " " + (currentDeckIndex + 1).ToString("n0") + "/" + currentDeck.cards.Count;
        }

        public void ShowCard()
        {
            //Show the card based on the current index
            CardData currentCard = currentDeck.cards[currentDeckIndex];
            PlayerXP currentPlayer = InfoPlayer.Instance.PlayerState;
            _isLock = false;

            switch (cardType)
            {
                case "dating":
                    CardText(currentPlayer.playerDeckCard.datingCard[currentDeckIndex], currentPlayer.playerDeckCard.isBuyDatingCard[currentDeckIndex], currentCard);
                    break;
                case "relationship":
                    CardText(currentPlayer.playerDeckCard.relationshipCard[currentDeckIndex], currentPlayer.playerDeckCard.isBuyRelationshipCard[currentDeckIndex], currentCard);
                    break;
                case "marriage":
                    CardText(currentPlayer.playerDeckCard.marriagepCard[currentDeckIndex], currentPlayer.playerDeckCard.isBuyMarriagepCard[currentDeckIndex], currentCard);                       
                    break;
                default:
                    break;
            }

            bool isFancyCard = currentCard.action == PathNodeAction.AdvanceToRelationshipWithProtectionFromSingle;
            cardImage.color = isFancyCard ? specialCardColor : normalCardColor;
            cardSilhouetteObj.SetActive(isFancyCard);
            cardTextLabel.color = isFancyCard ? Color.white : Color.black;
        }

        public void BuyCard()
        {
            if (!_isLock)
                return;

            APIManager.GetUserDetails(user =>
            {
                if (user.wallet >= _costCard)
                {
                    string text = "Do you want to buy this card? \n It will be cost 100 KL bucks";

                    PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
                    {
                        new PopupDialog.PopupButton()
                        {
                            text = "Okey",
                            buttonColor = PopupDialog.PopupButtonColor.Plain,
                            onClicked = () =>
                            {
                                APIManager.AddCurrency(-_costCard, balance => 
                                {
                                    switch (cardType)
                                    {
                                        case "dating":
                                            InfoPlayer.Instance.PlayerState.playerDeckCard.datingCard[currentDeckIndex] = true;
                                            InfoPlayer.Instance.PlayerState.playerDeckCard.isBuyDatingCard[currentDeckIndex] = true;
                                            break;
                                        case "relationship":
                                            InfoPlayer.Instance.PlayerState.playerDeckCard.relationshipCard[currentDeckIndex] = true;
                                            InfoPlayer.Instance.PlayerState.playerDeckCard.isBuyRelationshipCard[currentDeckIndex] = true;
                                            break;
                                        case "marriage":
                                            InfoPlayer.Instance.PlayerState.playerDeckCard.marriagepCard[currentDeckIndex] = true;
                                            InfoPlayer.Instance.PlayerState.playerDeckCard.isBuyMarriagepCard[currentDeckIndex] = true;
                                            break;
                                        default:
                                            break;
                                    }

                                    User.current.wallet = balance;
                                    ShowCard();
                                    UpdateText();
                                    InfoPlayer.Instance.JSONPlayerInfo();
                                    StoreController.Instance.UpdateFromPlayerWallet();
                                });
                            }
                        },
                        new PopupDialog.PopupButton()
                        {
                            text = "Nevermind",
                            buttonColor = PopupDialog.PopupButtonColor.Plain,
                            onClicked = () =>{ }
                        }
                    };

                    PopupDialog.Instance.Show(" " ,text, buttons);
                }
                else
                    return;
            });
        }

        private void UpdateText()
        {
            walletLabels.text = User.current.wallet.ToString("n0") + " <sprite=0>";
        }

        private void CardText(bool isOpenCard, bool isBuy, CardData currentCard)
        {
            if (isOpenCard)
            {
                if (isBuy)
                {
                    _buySeeImage[0].SetActive(true);
                    _buySeeImage[1].SetActive(false);
                }
                else
                {
                    _buySeeImage[0].SetActive(false);
                    _buySeeImage[1].SetActive(true);
                }                   

                _lockImage.SetActive(false);
                cardTextLabel.text = currentCard.text + " (" + currentCard.parentheses + ")";
            }
            else
            {
                _isLock = true;
                cardTextLabel.text = "";
                _lockImage.SetActive(true);
                _buySeeImage[0].SetActive(false);
                _buySeeImage[1].SetActive(false);
            }
        }
    }
}

