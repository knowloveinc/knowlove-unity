using DG.Tweening;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using static Knowlove.TurnManager;
using Knowlove.UI;

namespace Knowlove.ActionAndPathLogic
{
    public class ProceedActionLogic : MonoBehaviour
    {
        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;
        [SerializeField] private GameUI _gameUI;

        public void ExecuteProceedAction(ProceedAction action, System.Action OnFinished)
        {
            Player currentPlayer = NetworkManager.Instance.players[_turnManager.turnIndex];
            Debug.Log("Executing proceed action for player: " + currentPlayer.NickName);
            BoardPiece playerBoardPiece = BoardManager.Instance.pieces[_turnManager.turnIndex];
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;
            int turnBank = (int)playerProperties["turnBank"];
            int diceCount = (int)playerProperties["diceCount"];
            int wallet = (int)playerProperties["wallet"];

            int avoidSingleCards = (int)playerProperties["avoidSingleCards"];
            bool protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];

            switch (action)
            {
                case ProceedAction.AdvanceToRelationshipWithProtectionFromSingle:
                    protectedFromSingleInRelationship = true;

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

                    Debug.Log("MOVING TO MARRIAGE PATH");
                    playerBoardPiece.GoToMarriage(currentPlayer, () =>
                    {
                        Debug.Log("Finished moving to marriage path.");
                        _turnManager.EndTurn();
                        OnFinished?.Invoke();
                    });
                    break;
                case ProceedAction.GoToKNOWLOVE:
                    Debug.Log("YOU WON THE GAME!!!!!");
                    _turnManager.turnState = TurnState.GameOver;

                    playerBoardPiece.GoToKnowLove(currentPlayer, () =>
                    {
                        Debug.Log("Finished moving piece to KnowLove space.");
                        //gameUI.ShowPrompt("YOU WON THE GAME!!!", null, currentPlayer, 0);
                        OnFinished?.Invoke();
                    });
                    break;
                case ProceedAction.LoseATurn:
                    if (turnBank > 0) turnBank = 0;
                    turnBank -= 1;

                    _turnManager.EndTurn();
                    OnFinished?.Invoke();
                    break;
                case ProceedAction.LoseTwoTurns:
                    if (turnBank > 0) turnBank = 0;
                    turnBank -= 2;

                    _turnManager.EndTurn();
                    OnFinished?.Invoke();
                    break;
                case ProceedAction.LoseThreeTurns:
                    if (turnBank > 0) turnBank = 0;
                    turnBank -= 3;

                    _turnManager.EndTurn();
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

            playerProperties["turnBank"] = turnBank;
            playerProperties["diceCount"] = diceCount;
            playerProperties["protectedFromSingleInRelationship"] = protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void BackToSingle(ProceedAction action, Player currentPlayer, BoardPiece playerBoardPiece)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            int turnBank = (int)playerProperties["turnBank"];
            int diceCount = (int)playerProperties["diceCount"];
            int wallet = (int)playerProperties["wallet"];
            int avoidSingleCards = (int)playerProperties["avoidSingleCards"];

            if (avoidSingleCards > 0)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    _turnManager.ShowAvoidCardPrompts(diceCount, playerBoardPiece, currentPlayer);
                });
            }
            else if (wallet > 0 && avoidSingleCards == 0)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    ShowOfferToBuyCard(diceCount, playerBoardPiece, currentPlayer, action);
                });
            }
            else
            {
                diceCount = 1;
                playerBoardPiece.GoHome(currentPlayer);
                _turnManager.EndTurn();
            }

            playerProperties["turnBank"] = turnBank;
            playerProperties["diceCount"] = diceCount;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void BackToSingleAndLoseATurn(ProceedAction action, Player currentPlayer, BoardPiece playerBoardPiece)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            int turnBank = (int)playerProperties["turnBank"];
            int diceCount = (int)playerProperties["diceCount"];
            int wallet = (int)playerProperties["wallet"];
            int avoidSingleCards = (int)playerProperties["avoidSingleCards"];

            if (avoidSingleCards > 0)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    _turnManager.ShowAvoidCardPrompts(diceCount, playerBoardPiece, currentPlayer);
                });
            }
            else if (wallet > 0 && avoidSingleCards == 0)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    ShowOfferToBuyCard(diceCount, playerBoardPiece, currentPlayer, action);
                });
            }
            else
            {
                diceCount = 1;
                playerBoardPiece.GoHome(currentPlayer);
                _turnManager.EndTurn();
            }

            if (turnBank > 0) turnBank = 0;

            turnBank -= 1;
        }

        public void ShowOfferToBuyCard(int diceCount, BoardPiece playerBoardPiece, Player currentPlayer, ProceedAction action)
        {
            string textPromps = "Do you want to buy the card Avoid To Single?";

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
                            _pathNodeActionLogic.RPC_ShowStore(_turnManager.turnIndex, isProceed, actionJson);
                        });
                    }
                },
                new PopupDialog.PopupButton()
                {
                    text = "no",
                    buttonColor = PopupDialog.PopupButtonColor.Plain,
                    onClicked = () =>
                    {
                        diceCount = 1;
                        playerBoardPiece.GoHome(currentPlayer);
                        _turnManager.EndTurn();
                    }
                }
            };

            _gameUI.ShowPrompt(textPromps, buttons, currentPlayer, 0, false);
        }
    }
}
