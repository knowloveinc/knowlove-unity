using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// This script is designed to handle GAME DATA, and execute movement of pieces on the board.
/// </summary>

namespace Knowlove
{
    public class BoardManager : MonoBehaviourPun
    {
        public static BoardManager Instance;

        public BoardPiece[] pieces;
        public Path[] paths;

        [SerializeField] private Card datingCard, relationshipCard, marriageCard, avoidSingleCard, listCard;
        [SerializeField] private List<DeckData> decks = new List<DeckData>();

        [SerializeField] private Transform[] diceStartPositions;
        [SerializeField] private Transform[] dice;

        private Dice[] _diceScripts;
        private Rigidbody[] _diceRb;

        public int diceScore = -1;
        public string diceRollLocation;
        public bool diceFinishedRolling = false;

        internal bool movingBoardPiece;

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            _diceRb = new Rigidbody[dice.Length];
            _diceScripts = new Dice[dice.Length];

            for(int i = 0; i < dice.Length; i++)
            {
                _diceScripts[i] = dice[i].GetComponent<Dice>();
                _diceRb[i] = dice[i].GetComponent<Rigidbody>();
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                foreach (Transform die in dice)
                {
                    Rigidbody rb = die.GetComponent<Rigidbody>();

                    if (rb != null)
                        Destroy(rb);
                }
            }
        }

        public void SetDiceRigidbody()
        {
            if(dice[0].GetComponent<Rigidbody>() == null)
            {
                dice[0].gameObject.AddComponent<Rigidbody>();
                dice[1].gameObject.AddComponent<Rigidbody>();

                for (int i = 0; i < dice.Length; i++)
                {
                    _diceRb[i] = dice[i].GetComponent<Rigidbody>();

                    _diceRb[i].isKinematic = false;
                    _diceRb[i].useGravity = true;
                    _diceRb[i].mass = 0.04f;
                    _diceRb[i].drag = 0.3f;
                    _diceRb[i].angularDrag = 32;
                    _diceRb[i].collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    _diceRb[i].interpolation = RigidbodyInterpolation.None;
                }                   
            }
        }

        public void DestroyDiceRigidBody()
        {
            foreach (Transform die in dice)
            {
                Rigidbody rb = die.GetComponent<Rigidbody>();

                if (rb != null)
                    Destroy(rb);
            }
        }

        public void CheckWordLengthOfCards()
        {
            List<int> wordLengths = new List<int>();

            for (int i = 0; i < decks.Count; i++)
            {
                for (int j = 0; j < decks[i].cards.Count; j++)
                {
                    wordLengths.Add(decks[i].cards[j].text.Split(new char[] { ' ' }).Length);
                }
            }

            int averageWordCount = 0;
            int wordSum = 0;

            int longest = 0;

            for (int x = 0; x < wordLengths.Count; x++)
            {
                wordSum += wordLengths[x];

                if (wordLengths[x] > longest)
                    longest = wordLengths[x];
            }

            averageWordCount = wordSum / wordLengths.Count;

            Debug.Log("Average word length of cards: " + averageWordCount);
            Debug.Log("Longest word count on a card is: " + longest);
        }

        #region Board Pieces
        public void InitializeBoardPieces(int playerCount)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (i < playerCount)
                {
                    pieces[i].GoHome();

                    photonView.RPC(nameof(RPC_ToggleGamePiece), RpcTarget.All, i, true);
                    //pieces[i].gameObject.SetActive(true);
                }
                else
                {
                    photonView.RPC(nameof(RPC_ToggleGamePiece), RpcTarget.All, i, false);
                    //pieces[i].gameObject.SetActive(false);
                }
            }
        }

        [PunRPC]
        public void RPC_ToggleGamePiece(int index, bool enabled)
        {
            pieces[index].gameObject.SetActive(enabled);
        }

        [PunRPC]
        public void RPC_ToggleDice(bool enabled, int count)
        {
            for (int i = 0; i < dice.Length; i++)
                dice[i].gameObject.SetActive(enabled && i < count);                
        }

        [PunRPC]
        public void RPC_MoveBoardPiece(int playerIndex, int spaces)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Running MoveBoardPiece RPC");
                MoveBoardPiece(playerIndex, spaces);
            }
        }

        //public static event System.Action OnGamePieceFinishedMoving;

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

                        ExitGames.Client.Photon.Hashtable playerProperties = NetworkManager.Instance.players[TurnManager.Instance.turnIndex].CustomProperties;
                        playerProperties["progress"] = 0.333f;
                        NetworkManager.Instance.players[TurnManager.Instance.turnIndex].SetCustomProperties(playerProperties);

                        if (spaces >= 1)
                            photonView.RPC(nameof(RPC_MoveBoardPiece), RpcTarget.All, playerIndex, spaces - 1);
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
                    //OnGamePieceFinishedMoving?.Invoke();
                    movingBoardPiece = false;
                });
            }
        }
        #endregion

        #region Cards
        
        #if UNITY_EDITOR
        [ContextMenu("Load Card Data")]
        public void LoadCards()
        {
            if (Application.isEditor)
            {
                TextAsset ta = Resources.Load<TextAsset>("CardData");
                decks = JsonConvert.DeserializeObject<List<DeckData>>(ta.text);
            }
        }

        [ContextMenu("Save Card Data")]
        public void SaveCardData()
        {
            StreamWriter sw = new StreamWriter(Application.dataPath + "/_App/Resources/CardData.json");
            sw.Write(JsonConvert.SerializeObject(decks));
            sw.Close();
        }
        #endif

        public CardData DrawCard(string deckName)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (deckName.ToLower() != "avoid")
                {
                    for (int i = 0; i < decks.Count; i++)
                    {
                        Debug.Log(deckName + " == " + decks[i].name + " ??");
                        if (decks[i].name.ToLower() == deckName.ToLower())
                        {
                            Debug.Log("Deck found, drawing card...");
                            CardData cardData = decks[i].DrawCard();
                            Debug.Log("Card drawn: " + cardData.text);

                            if (deckName.ToLower() == "dating")
                                datingCard.DrawToCamera();

                            if (deckName.ToLower() == "relationship")
                                relationshipCard.DrawToCamera();

                            if (deckName.ToLower() == "marriage")
                                marriageCard.DrawToCamera();

                            return cardData;
                        }
                    }
                }
                else if (deckName.ToLower() == "avoid")
                {
                    Debug.Log("Drawing avoid single card");
                    avoidSingleCard.DrawToCamera();
                    return null;
                }

                Debug.LogError("Deck not found: " + deckName);
            }

            return null;
        }
        #endregion

        #region Dice
        public void RollDice(int amount = 1, string location = "board")
        {
            diceFinishedRolling = false;
            Debug.Log("RollDice");
            photonView.RPC(nameof(RPC_RollDice), RpcTarget.All, amount, location);
        }

        [PunRPC]
        public void RPC_RollDice(int amount = 1, string location = "board")
        {
            Debug.Log("RPC_RollDice");
            if (PhotonNetwork.IsMasterClient)
            {
                diceFinishedRolling = false;
                StopCoroutine(nameof(DoRollDice));
                StartCoroutine(DoRollDice(amount, location));
            }
        }        

        private IEnumerator DoRollDice(int amount, string location)
        {
            Debug.Log("DoRollDice");
            diceFinishedRolling = false;

            List<Rigidbody> rigidbodies = new List<Rigidbody>();

            List<Dice> diceComponents = new List<Dice>();

            photonView.RPC(nameof(RPC_ToggleDice), RpcTarget.All, true, amount);

            for (int i = 0; i < dice.Length; i++)
            {
                Dice diceObj = _diceScripts[i];

                if (i < amount)
                {
                    Debug.Log("Rolling Dice # " + i);
                    diceObj.gameObject.SetActive(true);

                    //dice.photonView.RPC("ToggleActive", RpcTarget.All, true);

                    diceObj.transform.position = diceStartPositions[i].position;

                    diceObj.transform.eulerAngles = new Vector3(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

                    diceComponents.Add(diceObj);

                    Rigidbody rb = _diceRb[i];
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.ResetCenterOfMass();

                    rb.AddForce(new Vector3(-1f, 0f, 1f) * Random.Range(0.01f, .04f), ForceMode.Impulse);
                    rb.AddTorque(Random.onUnitSphere * 0.2f, ForceMode.Impulse);

                    rigidbodies.Add(rb);

                    SoundManager.Instance.PlaySound("dice");
                }
                else
                {
                    diceObj.gameObject.SetActive(false);
                    //dice.photonView.RPC("ToggleActive", RpcTarget.All, false);
                }
            }

            while (rigidbodies.Any(x => x.IsSleeping() == false))
                yield return null;

            int score = 0;

            foreach (Dice die in diceComponents)
                score += die.number;

            Debug.Log($"Dice rolled {score}");

            yield return new WaitForSeconds(2f);

            //foreach (Dice die in diceComponents)
            //{
            //    //die.gameObject.SetActive(false);
            //    die.photonView.RPC("ToggleActive", RpcTarget.All, false);
            //}

            photonView.RPC(nameof(RPC_ToggleDice), RpcTarget.All, false, 0);

            diceScore = score;
            diceRollLocation = location;
            diceFinishedRolling = true;
        }
        #endregion
    }
}
