using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace GameBrewStudios
{

    
    public class Card : MonoBehaviour
    {
        public static event System.Action<Card> OnCardDestroyed;

        [SerializeField]
        TextMeshPro cardLabel;

        [SerializeField]
        CardDeck deck; //storing this incase we need to animate a card going back into a deck at some point.

        public void Init(string cardText, CardDeck deck)
        {
            if(string.IsNullOrEmpty(cardText))
            {
                cardText = "This is where the text on the card will go. This card was spawned randomly and is not using card data. Heres a random number to prove that it is unique though: " + Random.Range(0, 999999).ToString("n0");
            }

            this.deck = deck;

            if(cardLabel != null)
                cardLabel.text = cardText;
        }

        private void OnDestroy()
        {
            OnCardDestroyed?.Invoke(this);
        }
    }
}