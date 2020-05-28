using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using Photon.Realtime;
using Photon.Pun;
using Photon;

namespace GameBrewStudios
{

    [System.Serializable]
    public class CardLibrary
    {
        public DeckData[] decks;
    }


    [System.Serializable]
    public class CardData
    {
        public string parentheses;
        public int id;
        public string text;

        public bool isPrompt;
        public string promptMessage;
        public CardPromptButton[] promptButtons;
        
        public PathNodeAction action;
        public int rollCheck = 0;
        public ProceedAction rollPassed;
        public ProceedAction rollFailed;
    }

    [System.Serializable]
    public class CardPromptButton
    {
        public string text;
        public ProceedAction action;
    }

    [System.Serializable]
    public class DeckData
    {
        public string name;
        public List<CardData> cards;


        protected List<CardData> available;
        protected List<CardData> drawn;


        public CardData DrawCard()
        {
            Shuffle();
            return available[0];
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

    public class GameManager : MonoBehaviourPun, IPunObservable
    {
        
        public static GameManager Instance;


        public enum PathRing
        {
            Home = -1,
            Dating = 0,
            Relationship = 1,
            Marriage = 2
        }

        public enum CardType
        {
            Dating = 0,
            Relationship,
            Marriage,
            AvoidSingle,
            NonNegotiable
        }

        public BoardPiece[] pieces;

        public CardDeck[] decks;

        public int playerIndex = 0;

        public Path[] paths;
        


        public CardLibrary allCards;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LoadCardData();
            foreach (BoardPiece piece in pieces)
            {
                piece.GoHome();
            }

            ShuffleAll();
        }

        [ContextMenu("Shuffle Available Cards")]
        public void ShuffleAll()
        {
            for (int i = 0; i < allCards.decks.Length; i++)
            {
                allCards.decks[i].Shuffle();
            }

            Debug.Log("All card decks have been shuffled.");
        }

        [PunRPC]
        [ContextMenu("LoadCards")]
        public void LoadCardData()
        {
            bool ready = false;

            try
            {
                TextAsset ta = Resources.Load<TextAsset>("CardData");
                if (ta != null)
                {
                    allCards = JsonConvert.DeserializeObject<CardLibrary>(ta.text);
                }
            }
            catch (System.Exception)
            {
                throw new System.Exception("Failed to load card data");
            }
            

            if(ready)
            {
                Debug.Log("Cards loaded.");
            }
        }

        //[ContextMenu("Save Changes to Cards")]
        //public void SaveCards()
        //{
        //    if (!Application.isEditor || allCards == null || allCards.decks == null)
        //        return;

        //    int id = 0;

        //    foreach (DeckData deck in allCards.decks)
        //    {
        //        deck.cards = deck.cards.OrderBy(x => x.action).ToList();

        //        for (int i = 0; i < deck.cards.Count; i++)
        //        {
        //            //string[] parts = deck.cards[i].text.Split(new char[] { '(' });
        //            //deck.cards[i].id = id;
        //            //deck.cards[i].text = parts[0].Trim();
        //            //if(parts.Length > 1)
        //            //{ 
        //            //    deck.cards[i].parentheses = parts[1].Replace(")", "");
        //            //}

        //            if(!deck.cards[i].isPrompt)
        //            {
        //                deck.cards[i].promptButtons = null;
        //                deck.cards[i].promptMessage = null;
        //            }

        //            if (deck.cards[i].parentheses == "Advance to relationship.")
        //                deck.cards[i].action = PathNodeAction.AdvanceToNextPath;

        //            if (deck.cards[i].parentheses == "Advance to marriage.")
        //                deck.cards[i].action = PathNodeAction.AdvanceToNextPath;

        //            if (deck.cards[i].parentheses.ToLower().StartsWith("roll again"))
        //                deck.cards[i].action = PathNodeAction.RollAgain;

        //            if (deck.cards[i].parentheses.ToLower().StartsWith("back to single"))
        //                deck.cards[i].action = PathNodeAction.BackToSingle;

        //            id++;
        //        }
        //    }

        //    string reserialized = JsonConvert.SerializeObject(allCards);

        //    JObject reorderedData = JsonConvert.DeserializeObject<JObject>(reserialized);

        //    using (StreamWriter file = File.CreateText(Application.dataPath + "/_App/Resources/NewCardData.json"))
        //    using (JsonTextWriter writer = new JsonTextWriter(file))
        //    {
        //        reorderedData.WriteTo(writer);
        //    }
        //}

        public void InitializeBoardPieces(int playerCount)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (i < playerCount)
                    pieces[i].GoHome();
                else
                    pieces[i].gameObject.SetActive(false);
            }
        }

        [PunRPC]
        public void RPCMoveBoardPiece(int playerIndex, int spaces)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Running MoveBoardPiece RPC");
                MoveBoardPiece(playerIndex, spaces);
            }
        }

        public static event System.Action OnGamePieceFinishedMoving;

        public void MoveBoardPiece(int playerIndex, int spaces)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                BoardPiece piece = pieces[playerIndex];

                //nodes.Add(ring[piece.pathIndex]);
                if (piece.pathRing == PathRing.Home)
                {
                    Debug.Log("PLAYER ATTEMPTED TO MOVE " + spaces + " SPACES FROM SINGLE");
                    Debug.Log("Player still at single, moving to first position and restarting path.");

                    piece.JumpTo(paths[0].NodesAsVector3()[0], () =>
                    {
                        piece.pathIndex = 0;
                        piece.pathRing = PathRing.Dating;
                        if (spaces >= 1)
                        {
                            MoveBoardPiece(playerIndex, spaces - 1);
                        }
                        
                    });

                    return;
                }

                List<Vector3> ringNodes = paths[(int)piece.pathRing].NodesAsVector3();

                List<Vector3> nodes = new List<Vector3>();

                int j = piece.pathIndex + 1;

                for (int i = 0; i < spaces; i++)
                {
                    if (j >= ringNodes.Count)
                    {
                        j = 0;
                    }
                    Debug.Log("J = " + j + ", i = " + i);
                    nodes.Add(ringNodes[j]);

                    j++;
                }
                Debug.Log("Jump Path contains " + spaces + " spaces");
                Debug.Log("Jump path started with pathIndex of: " + piece.pathIndex);

                piece.JumpPath(nodes, () =>
                {
                    Debug.Log("Jump path finished with pathIndex of: " + piece.pathIndex);
                    OnGamePieceFinishedMoving?.Invoke();
                });
            }
        }

        public Transform[] dice;
        public Transform[] diceStartPositions;


        public static event System.Action<int, string> OnDiceFinishedRolling;

        [PunRPC]
        public void RollDice(int amount = 1, string location = "board")
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StopCoroutine("DoRollDice");
                StartCoroutine(DoRollDice(amount, location));
            }
        }

        IEnumerator DoRollDice(int amount, string location)
        {
            int i = 0;

            List<Rigidbody> rigidbodies = new List<Rigidbody>();

            List<Dice> diceComponents = new List<Dice>();

            foreach (Transform die in dice)
            {
                Dice dice = die.GetComponent<Dice>();
                if (i < amount)
                {
                    //die.gameObject.SetActive(true);

                    dice.photonView.RPC("ToggleActive", RpcTarget.All, true);

                    die.position = diceStartPositions[i].position;

                    die.eulerAngles = new Vector3(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

                    diceComponents.Add(dice);

                    Rigidbody rb = die.GetComponent<Rigidbody>();
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.ResetCenterOfMass();

                    rb.AddForce(new Vector3(-1f, 0f, 1f) * Random.Range(0.01f, .035f), ForceMode.Impulse);
                    rb.AddTorque(Random.onUnitSphere * 0.2f, ForceMode.Impulse);

                    rigidbodies.Add(rb);

                }
                else
                {
                    //die.gameObject.SetActive(false);
                    dice.photonView.RPC("ToggleActive", RpcTarget.All, false);
                }
                i++;
            }



            while (rigidbodies.Any(x => x.IsSleeping() == false))
            {
                yield return null;
            }

            int score = 0;
            foreach (Dice die in diceComponents)
            {
                score += die.number;
            }

            Debug.Log($"You rolled {score}");

            yield return new WaitForSeconds(2f);

            foreach (Dice die in diceComponents)
            {
                //die.gameObject.SetActive(false);
                die.photonView.RPC("ToggleActive", RpcTarget.All, false);
            }

            OnDiceFinishedRolling?.Invoke(score, location);
        }

        public CardData GetRandomCardData(string deckName)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < allCards.decks.Length; i++)
                {
                    Debug.Log(deckName + " == " + allCards.decks[i].name + " ??");
                    if (allCards.decks[i].name.ToLower() == deckName.ToLower())
                    {
                        Debug.Log("Deck found, drawing card...");
                        CardData cardData = allCards.decks[i].DrawCard();
                        Debug.Log("Card drawn: " + cardData.text);
                        return cardData;
                    }
                }
                Debug.LogError("Deck not found: " + deckName);
            }
            
            return null;
        }

        public void DrawCardFromDeck(string deckName, System.Action OnFinishedAnimating = null)
        {
            for(int i = 0; i < decks.Length; i++)
            {
                Debug.Log(deckName.ToLower() + " == " + decks[i].name.ToLower());
                if(decks[i].name.ToLower().Contains(deckName.ToLower()))
                {
                    Debug.Log("Deck found to do draw animation");
                    decks[i].DrawCard(OnFinishedAnimating);
                    return;
                }
            }

            Debug.LogError("DECK NOT FOUND FOR CARD ANIMATION");
        }

        string lastAllCards = "";

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.IsWriting)
            {
                //if (lastAllCards != JsonConvert.SerializeObject(allCards))
                //{
                Debug.Log("SENDING ALL CARDS BECAUSE OF CHANGE??");
                    lastAllCards = JsonConvert.SerializeObject(allCards);
                    stream.SendNext(lastAllCards);
                //}
            }
            else
            {
                Debug.Log("ALL CARDS RECEIVED");
                allCards = JsonConvert.DeserializeObject<CardLibrary>((string)stream.ReceiveNext());
            }
        }
    }
}