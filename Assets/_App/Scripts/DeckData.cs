using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove
{
    [System.Serializable]
    public class CardPromptButton
    {
        public string text;
        public ProceedAction action;
    }

    [System.Serializable]
    public enum PathRing
    {
        Home = -1,
        Dating = 0,
        Relationship = 1,
        Marriage = 2
    }

    [System.Serializable]
    public class DeckData
    {
        public string name;
        public List<CardData> cards;

        protected List<CardData> available;
        protected List<CardData> drawn;

        private int index = 0;
        private bool shuffled = false;
        public CardData DrawCard()
        {
            if (!shuffled)
            {
                Shuffle();
                shuffled = true;
            }

            CardData returnCard = cards[index];
            index++;

            if (index >= cards.Count)
            {
                Shuffle();
                index = 0;
            }

            return returnCard;
            //return cards[Random.Range(0, cards.Count)];
        }

        public void Shuffle()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (cards != null && cards.Count > 0 && (available == null || available.Count == 0))
                    available = new List<CardData>(cards);

                for (int i = 0; i < available.Count; i++)
                {
                    int a = i;
                    int b = Random.Range(0, available.Count - 1);

                    CardData card1 = available[a];
                    CardData card2 = available[b];
                    CardData limbo = card2;

                    available[b] = card1;
                    available[a] = limbo;
                }
            }         
        }
    }
}