using DG.Tweening;
using Photon.Realtime;
using UnityEngine;
using static Knowlove.TurnManager;
using Knowlove.UI;
using Knowlove.MyStuffInGame;
using System;
using Knowlove.XPSystem;

namespace Knowlove.ActionAndPathLogic
{
    public class ProceedActionLogic : MonoBehaviour
    {
        public delegate void ShowedPrompts(string text, PopupDialog.PopupButton[] buttons = null, Player player = null, int bgColor = 0, bool autoEndTurn = true);

        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private GameStuff _gameStuff;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;

        private int _turnBank;
        private int _diceCount;
        private int _wallet;
        private int _avoidSingleCards;
        private bool _protectedFromSingleInRelationship;
        private bool _protectedFromSingleInMarriage;
        private bool _protectedFromSingleAllGame;

        public event ShowedPrompts ChoicedOfPlayer;
        public Action<Player> UsedAvoidSingleCard;

        private int TurnIndex
        {
            get => _turnManager.turnIndex;
        }

        private bool IsDaring
        {
            get => !(BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Dating || BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Home);
        }

        private bool IsRelationship
        {
            get => BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Relationship;
        }

        private bool IsMarriage
        {
            get => BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Marriage;
        }

        public void ExecuteProceedAction(ProceedAction action, System.Action OnFinished)
        {
            Player currentPlayer = NetworkManager.Instance.players[TurnIndex];
            Debug.Log("Executing proceed action for player: " + currentPlayer.NickName);
            BoardPiece playerBoardPiece = BoardManager.Instance.pieces[TurnIndex];
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            _turnBank = (int)playerProperties["turnBank"];
            _diceCount = (int)playerProperties["diceCount"];
            _wallet = (int)playerProperties["wallet"];

            _avoidSingleCards = (int)playerProperties["avoidSingleCards"];
            _protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];
            _protectedFromSingleInMarriage = InfoPlayer.Instance.PlayerState.ProtectedFromBackToSingleInMarriagePerGame;
            _protectedFromSingleAllGame = InfoPlayer.Instance.PlayerState.ProtectedFromBackToSinglePerGame;

            switch (action)
            {
                case ProceedAction.AdvanceToRelationshipWithProtectionFromSingle:
                    _protectedFromSingleInRelationship = true;

                    playerBoardPiece.GoToRelationship(currentPlayer, () =>
                    {
                        Debug.Log("Finished moving to relationship path.");
                        _turnManager.EndTurn();
                        OnFinished?.Invoke();
                    });

                    break;

                case ProceedAction.BackToSingle:
                    BackToSingle(action, currentPlayer, playerBoardPiece);

                    OnFinished?.Invoke();
                    break;
                case ProceedAction.BackToSingleAndLoseATurn:
                    BackToSingleAndLoseATurn(action, currentPlayer, playerBoardPiece);

                    OnFinished?.Invoke();
                    break;
                case ProceedAction.GoToRelationship:
                    playerBoardPiece.GoToRelationship(currentPlayer, () =>
                    {
                        Debug.Log("Finished moving to relationship path.");
                        _turnManager.EndTurn();
                        OnFinished?.Invoke();
                    });
                    break;
                case ProceedAction.GoToMarriage:
                    playerBoardPiece.GoToMarriage(currentPlayer, () =>
                    {
                        Debug.Log("Finished moving to marriage path.");
                        _turnManager.EndTurn();
                        OnFinished?.Invoke();
                    });
                    break;
                case ProceedAction.GoToKNOWLOVE:
                    _turnManager.turnState = TurnState.GameOver;

                    playerBoardPiece.GoToKnowLove(currentPlayer, () =>
                    {
                        Debug.Log("Finished moving piece to KnowLove space.");
                        //gameUI.ShowPrompt("YOU WON THE GAME!!!", null, currentPlayer, 0);
                        OnFinished?.Invoke();
                    });
                    break;
                case ProceedAction.LoseATurn:
                    LoseTurns(1);

                    OnFinished?.Invoke();
                    break;
                case ProceedAction.LoseTwoTurns:
                    LoseTurns(2);

                    OnFinished?.Invoke();
                    break;
                case ProceedAction.LoseThreeTurns:
                    LoseTurns(3);

                    OnFinished?.Invoke();
                    break;
                case ProceedAction.Nothing:
                    _turnManager.EndTurn();
                    break;
                default:
                    _turnManager.EndTurn();
                    OnFinished?.Invoke();
                    break;
            }

            playerProperties["turnBank"] = _turnBank;
            playerProperties["diceCount"] = _diceCount;
            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void BackToSingle(ProceedAction action, Player currentPlayer, BoardPiece playerBoardPiece)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            if (_protectedFromSingleAllGame)
                ProtectedFromSingle(currentPlayer, "You have protected all game");
            else if (_protectedFromSingleInMarriage && IsMarriage)
                ProtectedFromSingle(currentPlayer, "You have protected in marriage ring");
            else if(_protectedFromSingleInRelationship && IsRelationship)
                ProtectedFromSingle(currentPlayer, "You have protected in relationship.");
            else if (_avoidSingleCards > 0)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    ShowAvoidCardPrompts(playerBoardPiece, currentPlayer);
                });
            }
            else if (_wallet > 0 && _avoidSingleCards == 0 && IsDaring)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    ShowOfferToBuyCard(playerBoardPiece, currentPlayer, action);
                });
            }
            else
            {
                _diceCount = 1;
                _protectedFromSingleInRelationship = false;
                playerBoardPiece.GoHome(currentPlayer);

                DOVirtual.DelayedCall(1f, () =>
                {
                    _turnManager.EndTurn();
                });
            }

            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            playerProperties["diceCount"] = _diceCount;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void BackToSingleAndLoseATurn(ProceedAction action, Player currentPlayer, BoardPiece playerBoardPiece)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            if (_protectedFromSingleAllGame)
                ProtectedFromSingle(currentPlayer, "You have protected all game");
            else if (_protectedFromSingleInMarriage && IsMarriage)
                ProtectedFromSingle(currentPlayer, "You have protected in marriage ring");
            else if (_protectedFromSingleInRelationship && IsRelationship)
                ProtectedFromSingle(currentPlayer, "You have protected in relationship.");
            else if (_avoidSingleCards > 0)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    ShowAvoidCardPrompts(playerBoardPiece, currentPlayer);
                });
            }
            else if (_wallet > 0 && _avoidSingleCards == 0 && IsDaring)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    ShowOfferToBuyCard(playerBoardPiece, currentPlayer, action);
                });
            }
            else
            {
                _diceCount = 1;
                playerBoardPiece.GoHome(currentPlayer);
                _protectedFromSingleInRelationship = false;

                DOVirtual.DelayedCall(1f, () =>
                {
                    _turnManager.EndTurn();
                });
            }

            if (_turnBank > 0) 
                _turnBank = 0;

            _turnBank -= 1;

            playerProperties["turnBank"] = _turnBank;
            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void ShowOfferToBuyCard(BoardPiece playerBoardPiece, Player currentPlayer, ProceedAction action)
        {
            string textPromps = "Do you want to save the relationship by investing in self-improvement unstead?";

            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
                new PopupDialog.PopupButton()
                {
                    text = "Yes",
                    buttonColor = PopupDialog.PopupButtonColor.Green,
                    onClicked = () =>
                    {
                        string actionJson = JsonUtility.ToJson(action);
                        bool isProceed = true;

                        DOVirtual.DelayedCall(0.25f, () =>
                        {
                            _pathNodeActionLogic.RPC_ShowStore(TurnIndex, isProceed, actionJson);
                        });
                    }
                },
                new PopupDialog.PopupButton()
                {
                    text = "no",
                    buttonColor = PopupDialog.PopupButtonColor.Plain,
                    onClicked = () =>
                    {
                        GoHome(playerBoardPiece, currentPlayer);
                    }
                }
            };

            ChoicedOfPlayer?.Invoke(textPromps, buttons, currentPlayer, 0, false);
        }

        private void ShowAvoidCardPrompts(BoardPiece playerBoardPiece, Player currentPlayer)
        {
            string textPromps = "Do you want to use the card Avoid To Single?";

            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
                new PopupDialog.PopupButton()
                {
                    text = "Yes",
                    buttonColor = PopupDialog.PopupButtonColor.Green,
                    onClicked = () =>
                    {
                        UsedAvoidSingleCard?.Invoke(currentPlayer);
                        _gameStuff.DeleteCardFromInventory(TurnIndex);
                    }
                },
                new PopupDialog.PopupButton()
                {
                    text = "no",
                    buttonColor = PopupDialog.PopupButtonColor.Plain,
                    onClicked = () =>
                    {
                        GoHome(playerBoardPiece, currentPlayer);
                    }
                }
            };

            ChoicedOfPlayer?.Invoke(textPromps, buttons, currentPlayer);           
        }

        private void LoseTurns(int count)
        {
            if (_turnBank > 0) _turnBank = 0;
            _turnBank -= count;

            DOVirtual.DelayedCall(1f, () => { _turnManager.EndTurn(); });            
        }

        private void ProtectedFromSingle(Player currentPlayer, string textBtn)
        {            
            PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
            {
                text = "Okay",
                onClicked = () =>
                {
                    DOVirtual.DelayedCall(1f, () => { _turnManager.EndTurn(); });
                }
            };

            ChoicedOfPlayer?.Invoke(textBtn, new PopupDialog.PopupButton[] { yesBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, false);
        }

        private void GoHome(BoardPiece playerBoardPiece, Player currentPlayer)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            _diceCount = 1;
            _protectedFromSingleInRelationship = false;
            playerBoardPiece.GoHome(currentPlayer);

            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }
    }
}