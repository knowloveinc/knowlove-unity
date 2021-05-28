using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using DG.Tweening;
using Knowlove.UI;
using static Knowlove.TurnManager;
using Knowlove.MyStuffInGame;
using System;
using Knowlove.XPSystem;
using GameBrewStudios.Networking;
using GameBrewStudios;

namespace Knowlove.ActionAndPathLogic
{
    public class PathNodeActionLogic : MonoBehaviourPunCallbacks
    {
        public delegate void ShowedPrompts(string text, PopupDialog.PopupButton[] buttons = null, Player player = null, int bgColor = 0, bool autoEndTurn = true);

        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private RollDiceLogic _rollDiceLogic;
        [SerializeField] private GameStuff _gameStuff;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;
        [SerializeField] private BackSingleLogic _backSingleLogic;
        [SerializeField] private BackSingleIgnorList _backSingleIgnorList;

        private int _turnBank;
        private int _diceCount;
        private int _avoidSingleCards;
        private int _dateCount;
        private bool _protectedFromSingleInRelationship;
        private bool _protectedFromSingleInMarriage;
        private bool _protectedFromSingleAllGame;

        public Action<Player> UsedAvoidSingleCard;
        public Action<string, string> WentToKnowLove;
        public Action<CardData> PickedCardScenario;

        public event ShowedPrompts ChoicedOfPlayer;

        private int TurnIndex
        {
            get => _turnManager.turnIndex;
        }

        private bool IsRelationship
        {
            get => BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Relationship;
        }

        private bool IsMarriage
        {
            get => BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Marriage;
        }

        public void HandlePathNodeAction(PathNodeAction action, string text, int rollCheck = 0, ProceedAction onPassed = ProceedAction.Nothing, ProceedAction onFailed = ProceedAction.Nothing, bool skipPrompt = false)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Tried running HandlePathNodeAction on non-master client");
                return;
            }

            BoardPiece piece = BoardManager.Instance.pieces[TurnIndex];
            Player currentPlayer = NetworkManager.Instance.players[TurnIndex];

            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            _turnBank = (int)playerProperties["turnBank"];
            _diceCount = (int)playerProperties["diceCount"];
            _avoidSingleCards = (int)playerProperties["avoidSingleCards"];
            _dateCount = (int)playerProperties["dateCount"];
            _protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];

            _protectedFromSingleInMarriage = InfoPlayer.Instance.PlayerState.ProtectedFromBackToSingleInMarriagePerGame;
            _protectedFromSingleAllGame = InfoPlayer.Instance.PlayerState.ProtectedFromBackToSinglePerGame;

            switch (action)
            {
                case PathNodeAction.GoToKNOWLOVE:
                    piece.GoToKnowLove(currentPlayer);
                    WentToKnowLove?.Invoke("Won the Game!", currentPlayer.NickName);
                    _turnManager.turnState = TurnState.GameOver;
                    break;
                case PathNodeAction.BackToSingle:
                    _backSingleLogic.BackToSingle(text, skipPrompt, piece, currentPlayer);

                    break;
                case PathNodeAction.Scenario:
                    Scenario(text);

                    break;
                case PathNodeAction.BackToSingleOrDisregardBecauseList:
                    _backSingleIgnorList.BackToSingleOrDisregardBecauseList(text, skipPrompt, piece, currentPlayer);

                    break;
                case PathNodeAction.AdvanceAndGetAvoidCard:
                    AdvanceAndGetAvoidCard(piece, currentPlayer);

                    break;
                case PathNodeAction.AdvanceToNextPath:
                    if (piece.pathRing == PathRing.Dating)
                        piece.GoToRelationship(currentPlayer);
                    else if (piece.pathRing == PathRing.Relationship)
                        piece.GoToMarriage(currentPlayer);
                    DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());

                    break;
                case PathNodeAction.AdvanceToRelationshipWithProtectionFromSingle:
                    _protectedFromSingleInRelationship = true;

                    piece.GoToRelationship(currentPlayer);

                    if (!skipPrompt)
                        ChoicedOfPlayer?.Invoke(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing);
                    else
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());

                    break;
                case PathNodeAction.LoseTurn:
                    LoseTurnsOrRollAgain(text, -1, true, skipPrompt, currentPlayer);

                    break;
                case PathNodeAction.Lose2Turns:
                    LoseTurnsOrRollAgain(text, -2, true, skipPrompt, currentPlayer);

                    break;
                case PathNodeAction.Lose3Turns:
                    LoseTurnsOrRollAgain(text, -3, true, skipPrompt, currentPlayer);

                    break;
                case PathNodeAction.RollAgain:
                    LoseTurnsOrRollAgain(text, 1, false, skipPrompt, currentPlayer);

                    break;
                case PathNodeAction.CollectAvoidSingleCard:
                    BoardManager.Instance.DrawCard("avoid");
                    UsedAvoidSingleCard?.Invoke(currentPlayer);

                    photonView.RPC(nameof(GetAvoidCard), RpcTarget.All, currentPlayer);

                    DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    break;
                case PathNodeAction.RollAgainTwice:
                    LoseTurnsOrRollAgain(text, 2, false, skipPrompt, currentPlayer);

                    break;
                case PathNodeAction.RollToProceed:
                    if (!skipPrompt)
                    {
                        if(piece.pathIndex == 27 && IsMarriage && _protectedFromSingleAllGame)
                            ProtectBackSingle("This Cheating Landing Space Doesn’t apply to a player with your Know Love Status, your marriage is safe.", currentPlayer);
                        else if((piece.pathIndex == 28 || piece.pathIndex == 10) && IsRelationship && (_protectedFromSingleAllGame || _protectedFromSingleInMarriage))
                            ProtectBackSingle("This Cheating Landing Space Doesn’t apply to a player with your Know Love Status, your relationship is safe.", currentPlayer);
                        else
                        {
                            PopupDialog.PopupButton rbtn = new PopupDialog.PopupButton()
                            {
                                text = "Roll",
                                onClicked = () =>
                                {
                                    Roll(rollCheck, onPassed, onFailed);
                                }
                            };

                            ChoicedOfPlayer?.Invoke(text, new PopupDialog.PopupButton[] { rbtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, false);
                        }                        
                    }
                    else
                        Roll(rollCheck, onPassed, onFailed);

                    break;
                case PathNodeAction.RollWith2Dice:
                    _turnBank += 1; //Award the extra turn
                    _diceCount = 2;

                    if (!skipPrompt)
                        ChoicedOfPlayer?.Invoke(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing);
                    else
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());

                    break;
                case PathNodeAction.Nothing:
                default:
                    ChoicedOfPlayer?.Invoke(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing);
                    break;
            }

            playerProperties["turnBank"] = _turnBank;
            playerProperties["diceCount"] = _diceCount;
            playerProperties["dateCount"] = _dateCount;
            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void ProtectBackSingle(string text, Player currentPlayer)
        {
            PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
            {
                text = "Okay",
                onClicked = () =>
                {
                    DOVirtual.DelayedCall(1f, () => { _turnManager.EndTurn(); });
                }
            };

            ChoicedOfPlayer?.Invoke(text, new PopupDialog.PopupButton[] { yesBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, false);
        }

        public void RPC_ShowStore(int playerIndex, bool isProceedAction, string actionJSOn)
        {
            DOVirtual.DelayedCall(0.25f, () =>
            {
                photonView.RPC(nameof(ShowStore), RpcTarget.AllBufferedViaServer, playerIndex, isProceedAction, actionJSOn);
            });
            
        }

        [PunRPC]
        private void ShowStore(int playerIndex, bool isProceedAction, string actionJSOn)
        {
            Player currentPlayer = NetworkManager.Instance.players[playerIndex];

            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {
                ProceedAction action = JsonUtility.FromJson<ProceedAction>(actionJSOn);
                StoreController.Show();

                StoreController.Instance.SetAction(isProceedAction, action);
            }
        }

        private void Roll(int rollCheck, ProceedAction onPassed, ProceedAction onFailed)
        {
            _turnManager.currentRollCheck = rollCheck;
            _turnManager.currentOnPassed = onPassed;
            _turnManager.currentOnFailed = onFailed;
            _rollDiceLogic.RollDice(_diceCount, "scenario");
        }        

        private void Scenario(string text)
        {
            if (text.ToLower().Contains("dating"))
            {
                ShowCardScenario("dating");
                _dateCount++;
            }
            else if (text.ToLower().Contains("relationship"))
                ShowCardScenario("relationship");
            else if (text.ToLower().Contains("marriage"))
                ShowCardScenario("marriage");
            else
                Debug.LogError("SHOULD NOT BE ABLE TO HIT THIS CASE IN SCENARIO BRANCH: Scenario type was set to = " + text);
        }

        private void ShowCardScenario(string scenario)
        {
            CardData card = BoardManager.Instance.DrawCard(scenario);
            Debug.Log("Got card: " + card.id);
            //GameManager.Instance.DrawCardFromDeck("relationship", () =>
            DOVirtual.DelayedCall(1f, () =>
            {
                Debug.Log("Finished drawing card animation...");
                PickedCardScenario?.Invoke(card);
            });
        }

        private void AdvanceAndGetAvoidCard(BoardPiece piece, Player currentPlayer)
        {
            //Debug.LogError("Advance and Get Avoid going to single card is not added yet. Just advancing instead.");

            BoardManager.Instance.DrawCard("avoid");
            UsedAvoidSingleCard?.Invoke(currentPlayer);

            photonView.RPC(nameof(GetAvoidCard), RpcTarget.All, currentPlayer);

            DOVirtual.DelayedCall(2f, () =>
            {
                if (piece.pathRing == PathRing.Dating)
                    piece.GoToRelationship(currentPlayer);
                else if (piece.pathRing == PathRing.Relationship)
                    piece.GoToMarriage(currentPlayer);

                DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
            });
        }

        [PunRPC]
        private void GetAvoidCard(Player currentPlayer)
        {
            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {
                APIManager.GetUserDetails((user) =>
                {
                    APIManager.AddItem("avoidSingle", 1, (inventory) =>
                    {
                        User.current.inventory = inventory;
                        StoreController.Instance.UpdateFromPlayerInventory();
                        _gameStuff.GetSpecialCard();
                    });
                });
            }
        }

        private void LoseTurnsOrRollAgain(string text, int count, bool isLose, bool skipPrompt, Player currentPlayer)
        {
            if (_turnBank > 0 && isLose) _turnBank = 0;

            _turnBank += count;

            if (!skipPrompt)
                ChoicedOfPlayer?.Invoke(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing);
            else
                DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
        }
    }
}