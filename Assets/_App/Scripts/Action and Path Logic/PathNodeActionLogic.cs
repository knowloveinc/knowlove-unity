using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using DG.Tweening;
using Knowlove.UI;
using static Knowlove.TurnManager;
using Knowlove.MyStuffInGame;

namespace Knowlove.ActionAndPathLogic
{
    public class PathNodeActionLogic : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private GameStuff _gameStuff;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;

        public void HandlePathNodeAction(PathNodeAction action, string text, int rollCheck = 0, ProceedAction onPassed = ProceedAction.Nothing, ProceedAction onFailed = ProceedAction.Nothing, bool skipPrompt = false)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Tried running HandlePathNodeAction on non-master client");
                return;
            }

            BoardPiece piece = BoardManager.Instance.pieces[_turnManager.turnIndex];

            Player currentPlayer = NetworkManager.Instance.players[_turnManager.turnIndex];

            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;
            int turnBank = (int)playerProperties["turnBank"];
            int diceCount = (int)playerProperties["diceCount"];
            int avoidSingleCards = (int)playerProperties["avoidSingleCards"];
            int dateCount = (int)playerProperties["dateCount"];
            int wallet = (int)playerProperties["wallet"];
            bool protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];

            switch (action)
            {
                case PathNodeAction.GoToKNOWLOVE:
                    piece.GoToKnowLove(currentPlayer);
                    _gameUI.SetTopText("Won the Game!", currentPlayer.NickName);
                    _turnManager.turnState = TurnState.GameOver;
                    break;
                case PathNodeAction.BackToSingle:
                    List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();

                    if (avoidSingleCards > 0)
                    {
                        if (!skipPrompt)
                        {
                            text += " [ Avoid Going To Single Card Activated ]";
                            PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                            {
                                text = "Okay",
                                onClicked = () =>
                                {
                                    _gameStuff.DeleteCardFromInventory(0, _turnManager.turnIndex);
                                    _turnManager.EndTurn();
                                }
                            };

                            PopupDialog.PopupButton noBtn = new PopupDialog.PopupButton()
                            {
                                text = "No, back to single",
                                onClicked = () =>
                                {
                                    piece.GoHome(currentPlayer);
                                    _turnManager.EndTurn();
                                }
                            };

                            buttons.Add(yesBtn);
                            buttons.Add(noBtn);
                        }
                        else
                        {
                            DOVirtual.DelayedCall(0.25f, () =>
                            {
                                _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                            });
                        }

                    }
                    else
                    {
                        if (!skipPrompt)
                        {
                            PopupDialog.PopupButton btn = new PopupDialog.PopupButton()
                            {
                                text = "Okay",
                                onClicked = () =>
                                {
                                    piece.GoHome(currentPlayer);
                                    _turnManager.EndTurn();
                                }
                            };

                            buttons.Add(btn);

                            if (wallet > 0)
                            {
                                PopupDialog.PopupButton openStoreBtn = new PopupDialog.PopupButton()
                                {
                                    text = "No, i want to buy the Avoid To Single card",
                                    onClicked = () =>
                                    {
                                        string actionJson = JsonUtility.ToJson(ProceedAction.Nothing);
                                        DOVirtual.DelayedCall(0.25f, () =>
                                        {
                                            bool isAction = false;
                                            photonView.RPC(nameof(ShowStore), RpcTarget.AllBufferedViaServer, _turnManager.turnIndex, isAction, actionJson);
                                        });
                                    }
                                };

                                buttons.Add(openStoreBtn);
                            }
                        }
                        else
                        {
                            if (wallet > 0)
                            {
                                DOVirtual.DelayedCall(0.25f, () =>
                                {
                                    _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                                });
                            }
                            else
                                piece.GoHome(currentPlayer);
                        }
                    }

                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, buttons.ToArray(), currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing, false);
                    }
                    else
                    {
                        if (avoidSingleCards == 0 && wallet == 0)
                            DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }

                    break;
                case PathNodeAction.Scenario:
                    if (text.ToLower().Contains("dating"))
                    {
                        CardData card = BoardManager.Instance.DrawCard("dating");
                        Debug.Log("Got card: " + card.id);
                        //GameManager.Instance.DrawCardFromDeck("dating", () =>
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            Debug.Log("Finished drawing card animation...");
                            _gameUI.ShowCard(card);
                        });

                        dateCount++;
                    }
                    else if (text.ToLower().Contains("relationship"))
                    {
                        CardData card = BoardManager.Instance.DrawCard("relationship");
                        Debug.Log("Got card: " + card.id);
                        //GameManager.Instance.DrawCardFromDeck("relationship", () =>
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            Debug.Log("Finished drawing card animation...");
                            _gameUI.ShowCard(card);
                        });

                    }
                    else if (text.ToLower().Contains("marriage"))
                    {
                        CardData card = BoardManager.Instance.DrawCard("marriage");
                        Debug.Log("Got card: " + card.id);
                        //GameManager.Instance.DrawCardFromDeck("relationship", () =>
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            Debug.Log("Finished drawing card animation...");
                            _gameUI.ShowCard(card);
                        });
                    }
                    else
                    {
                        Debug.LogError("SHOULD NOT BE ABLE TO HIT THIS CASE IN SCENARIO BRANCH: Scenario type was set to = " + text);
                    }
                    break;
                case PathNodeAction.BackToSingleOrDisregardBecauseList:
                    if (!skipPrompt)
                    {
                        PopupDialog.PopupButton btn;
                        string dialogText = "You settled for someone who didnt meet your non-negotiable list requirements. (Back to single.)";

                        if (!protectedFromSingleInRelationship && avoidSingleCards < 1 && wallet > 0)
                        {
                            btn = new PopupDialog.PopupButton()
                            {
                                text = "Okay",
                                onClicked = () =>
                                {
                                    piece.GoHome(currentPlayer);
                                    _turnManager.EndTurn();
                                }
                            };

                            PopupDialog.PopupButton openStoreBtn = new PopupDialog.PopupButton()
                            {
                                text = "No, i want to buy the Avoid To Single card",
                                onClicked = () =>
                                {
                                    string actionJson = JsonUtility.ToJson(ProceedAction.Nothing);
                                    DOVirtual.DelayedCall(0.25f, () =>
                                    {
                                        bool isAction = false;
                                        photonView.RPC(nameof(ShowStore), RpcTarget.AllBufferedViaServer, _turnManager.turnIndex, isAction, actionJson);
                                    });
                                }
                            };

                            diceCount = 1;
                            _gameUI.ShowPrompt(dialogText, new PopupDialog.PopupButton[] { btn, openStoreBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing, false);
                        }
                        else if (!protectedFromSingleInRelationship && avoidSingleCards < 1)
                        {
                            btn = new PopupDialog.PopupButton()
                            {
                                text = "Okay",
                                onClicked = () =>
                                {
                                    piece.GoHome(currentPlayer);
                                }
                            };

                            diceCount = 1;
                            _gameUI.ShowPrompt(dialogText, new PopupDialog.PopupButton[] { btn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                        }
                        else if (protectedFromSingleInRelationship)
                        {
                            dialogText = "Your mate matches your non-negotiable list.";
                            btn = new PopupDialog.PopupButton()
                            {
                                text = "Okay",
                                onClicked = () =>
                                {
                                    Debug.Log("Disregarding issues and doing nothing.");
                                }
                            };

                            _gameUI.ShowPrompt(dialogText, new PopupDialog.PopupButton[] { btn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                        }
                        else if (avoidSingleCards > 0)
                        {

                            avoidSingleCards--;
                            Debug.Log("Used avoid card, not sending user back.");
                            dialogText += "\n\n[Activated Avoid Going to Single Card]";

                            PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                            {
                                text = "Okay",
                                onClicked = () =>
                                {
                                    _gameStuff.DeleteCardFromInventory(0, _turnManager.turnIndex);
                                    text += " [ Avoid Going To Single Card Activated ]";
                                }
                            };

                            PopupDialog.PopupButton noBtn = new PopupDialog.PopupButton()
                            {
                                text = "No, back to single",
                                onClicked = () =>
                                {
                                    piece.GoHome(currentPlayer);
                                }
                            };
                            _gameUI.ShowPrompt(dialogText, new PopupDialog.PopupButton[] { yesBtn, noBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing, true);
                        }
                    }
                    else
                    {
                        if (avoidSingleCards > 0 && wallet > 0)
                        {
                            DOVirtual.DelayedCall(0.25f, () =>
                            {
                                _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                            });

                        }
                        else if (!protectedFromSingleInRelationship)
                        {
                            piece.GoHome(currentPlayer);
                            diceCount = 1;
                            _turnManager.EndTurn();
                        }
                    }

                    break;
                case PathNodeAction.AdvanceAndGetAvoidCard:
                    //Debug.LogError("Advance and Get Avoid going to single card is not added yet. Just advancing instead.");

                    avoidSingleCards++;

                    BoardManager.Instance.DrawCard("avoid");
                    _gameUI.AvoidSingleCardAnimation(currentPlayer);

                    DOVirtual.DelayedCall(2f, () =>
                    {
                        if (piece.pathRing == PathRing.Dating)
                        {
                            protectedFromSingleInRelationship = false;
                            piece.GoToRelationship(currentPlayer);
                        }
                        else if (piece.pathRing == PathRing.Relationship)
                        {
                            protectedFromSingleInRelationship = false;
                            piece.GoToMarriage(currentPlayer);
                        }

                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    });


                    break;
                case PathNodeAction.AdvanceToNextPath:
                    if (piece.pathRing == PathRing.Dating)
                    {
                        protectedFromSingleInRelationship = false;
                        piece.GoToRelationship(currentPlayer);
                    }
                    else if (piece.pathRing == PathRing.Relationship)
                    {
                        protectedFromSingleInRelationship = false;
                        piece.GoToMarriage(currentPlayer);
                    }
                    DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());

                    break;
                case PathNodeAction.AdvanceToRelationshipWithProtectionFromSingle:
                    protectedFromSingleInRelationship = true;

                    piece.GoToRelationship(currentPlayer);

                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }

                    break;
                case PathNodeAction.LoseTurn:

                    if (turnBank > 0) turnBank = 0;

                    turnBank -= 1;

                    playerProperties["turnBank"] = turnBank;

                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }

                    break;
                case PathNodeAction.Lose2Turns:

                    if (turnBank > 0)
                        turnBank = 0;

                    turnBank -= 2;

                    playerProperties["turnBank"] = turnBank;

                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }
                    break;
                case PathNodeAction.Lose3Turns:
                    if (turnBank > 0) turnBank = 0;

                    turnBank -= 3;

                    playerProperties["turnBank"] = turnBank;

                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }
                    break;
                case PathNodeAction.RollAgain:
                    turnBank += 1;
                    playerProperties["turnBank"] = turnBank;


                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }
                    break;
                case PathNodeAction.RollAgainTwice:
                    turnBank += 2;
                    playerProperties["turnBank"] = turnBank;
                    currentPlayer.SetCustomProperties(playerProperties);

                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }
                    break;
                case PathNodeAction.RollToProceed:

                    if (!skipPrompt)
                    {
                        PopupDialog.PopupButton rbtn = new PopupDialog.PopupButton()
                        {
                            text = "Roll",
                            onClicked = () =>
                            {
                                _turnManager.currentRollCheck = rollCheck;
                                _turnManager.currentOnPassed = onPassed;
                                _turnManager.currentOnFailed = onFailed;
                                _turnManager.RollDice(diceCount, "scenario");
                            }
                        };

                        _gameUI.ShowPrompt(text, new PopupDialog.PopupButton[] { rbtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing, false);
                    }
                    else
                    {
                        _turnManager.currentRollCheck = rollCheck;
                        _turnManager.currentOnPassed = onPassed;
                        _turnManager.currentOnFailed = onFailed;
                        _turnManager.RollDice(diceCount, "scenario");
                    }

                    break;
                case PathNodeAction.RollWith2Dice:


                    turnBank += 1; //Award the extra turn
                    diceCount = 2;

                    if (!skipPrompt)
                    {
                        _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
                    }

                    break;
                case PathNodeAction.Nothing:
                default:
                    _gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing);
                    break;
            }

            playerProperties["turnBank"] = turnBank;
            playerProperties["diceCount"] = diceCount;
            playerProperties["dateCount"] = dateCount;
            playerProperties["protectedFromSingleInRelationship"] = protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
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
    }
}
