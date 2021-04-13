using Knowlove.UI.Menus;
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

        private int currentDeckIndex = 0;

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
        }

        public override void Show()
        {
            throw new System.NotSupportedException("Show() is not valid for this object. Use Show(deckName) instead.");
        }

        public void Show(string deckName)
        {
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
            cardTextLabel.text = currentCard.text + " (" + currentCard.parentheses + ")";

            bool isFancyCard = currentCard.action == PathNodeAction.AdvanceToRelationshipWithProtectionFromSingle;
            cardImage.color = isFancyCard ? specialCardColor : normalCardColor;
            cardSilhouetteObj.SetActive(isFancyCard);
            cardTextLabel.color = isFancyCard ? Color.white : Color.black;
        }
    }
}

