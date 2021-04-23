using System.Collections;
using System.Collections.Generic;
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

        private int _turnBank;
        private int _diceCount;
        private int _avoidSingleCards;
        private int _dateCount;
        private int _wallet;
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

        private bool IsNotDaring
        {
            get => !(BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Dating || BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Home);
        }

        private bool IsMarriage
        {
            get => BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Marriage;
        }

        private bool IsRelationship
        {
            get => BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Relationship;
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
            _wallet = (int)playerProperties["wallet"];
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
                    BackToSingle(text, skipPrompt, piece, currentPlayer);

                    break;
                case PathNodeAction.Scenario:
                    Scenario(text);

                    break;
                case PathNodeAction.BackToSingleOrDisregardBecauseList:
                    BackToSingleOrDisregardBecauseList(text, skipPrompt, piece, currentPlayer);

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
                case PathNodeAction.RollAgainTwice:
                    LoseTurnsOrRollAgain(text, 2, false, skipPrompt, currentPlayer);

                    break;
                case PathNodeAction.RollToProceed:
                    if (!skipPrompt)
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

        private void BackToSingle(string text, bool skipPrompt, BoardPiece piece, Player currentPlayer)
        {
            List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();

            if (_protectedFromSingleAllGame)
            {
                if (!skipPrompt)
                {
                    text += " [ You have protected all game ]";
                    PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                    {
                        text = "Okay",
                        onClicked = () =>
                        {
                            DOVirtual.DelayedCall(1f, () => { _turnManager.EndTurn(); });
                        }
                    };

                    buttons.Add(yesBtn);
                }
                else
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                    });
                }
            }
            else if (_protectedFromSingleInRelationship && IsRelationship)
            {
                if (!skipPrompt)
                {
                    text += " [ You have protected in relationship. ]";
                    PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                    {
                        text = "Okay",
                        onClicked = () =>
                        {
                            DOVirtual.DelayedCall(1f, () => { _turnManager.EndTurn(); });
                        }
                    };

                    buttons.Add(yesBtn);
                }
                else
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                    });
                }
            }
            else if (_protectedFromSingleInMarriage && IsMarriage)
            {
                if (!skipPrompt)
                {
                    text += " [ You have protected in marriage game ]";
                    PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                    {
                        text = "Okay",
                        onClicked = () =>
                        {
                            DOVirtual.DelayedCall(1f, () => { _turnManager.EndTurn(); });
                        }
                    };

                    buttons.Add(yesBtn);
                }
                else
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                    });
                }
            }
            else if (_avoidSingleCards > 0)
            {
                if (!skipPrompt)
                {
                    text += " [ Avoid Going To Single Card Activated ]";
                    PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                    {
                        text = "Okay",
                        onClicked = () =>
                        {
                            UsedAvoidSingleCard?.Invoke(currentPlayer);
                            _gameStuff.DeleteCardFromInventory(TurnIndex);
                            
                            DOVirtual.DelayedCall(1f, () => { _turnManager.EndTurn(); });
                        }
                    };

                    PopupDialog.PopupButton noBtn = new PopupDialog.PopupButton()
                    {
                        text = "No, back to single",
                        onClicked = () =>
                        {
                            _protectedFromSingleInRelationship = false;
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

                    if (_wallet > 0 && IsNotDaring)
                    {
                        PopupDialog.PopupButton openStoreBtn = new PopupDialog.PopupButton()
                        {
                            text = "Or, invest in making it work.",
                            onClicked = () =>
                            {
                                string actionJson = JsonUtility.ToJson(ProceedAction.Nothing);
                                DOVirtual.DelayedCall(0.25f, () =>
                                {
                                    bool isAction = false;
                                    photonView.RPC(nameof(ShowStore), RpcTarget.AllBufferedViaServer, TurnIndex, isAction, actionJson);
                                });
                            }
                        };

                        buttons.Add(openStoreBtn);
                    }
                }
                else
                {
                    if (_avoidSingleCards > 0 || (_wallet > 0 && IsNotDaring) || _protectedFromSingleAllGame || (IsMarriage && _protectedFromSingleInMarriage) || (_protectedFromSingleInRelationship && IsRelationship))
                    {
                        DOVirtual.DelayedCall(0.25f, () =>
                        {
                            _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                        });
                    }
                    else 
                    {
                        _protectedFromSingleInRelationship = false;
                        piece.GoHome(currentPlayer);

                        DOVirtual.DelayedCall(1f, () =>
                        {
                            _turnManager.EndTurn();
                        });
                    }
                        
                }
            }

            if (!skipPrompt)
                ChoicedOfPlayer?.Invoke(text, buttons.ToArray(), currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, false);
            else
            {
                if (_avoidSingleCards == 0 && _wallet == 0 && !IsNotDaring && !_protectedFromSingleAllGame && !(IsMarriage && _protectedFromSingleInMarriage) && !(_protectedFromSingleInRelationship && IsRelationship))
                    DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
            }
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

        private void BackToSingleOrDisregardBecauseList(string text, bool skipPrompt, BoardPiece piece, Player currentPlayer)
        {
            if (_protectedFromSingleAllGame)
            {
                if (!skipPrompt)
                {
                    text += " [ You have protected all game ]";
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
                else
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                    });
                }
            }
            else if (_protectedFromSingleInMarriage && IsMarriage)
            {
                if (!skipPrompt)
                {
                    text += " [ You have protected in marriage game ]";

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
                else
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                    });
                }
            }
            if (!skipPrompt)
            {
                PopupDialog.PopupButton btn;
                string dialogText = "You settled for someone who didnt meet your non-negotiable list requirements. (Back to single.)";

                if (_avoidSingleCards < 1 && _wallet > 0 && IsNotDaring)
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
                        text = "Or, invest in making it work.",
                        onClicked = () =>
                        {
                            string actionJson = JsonUtility.ToJson(ProceedAction.Nothing);
                            DOVirtual.DelayedCall(0.25f, () =>
                            {
                                bool isAction = false;
                                photonView.RPC(nameof(ShowStore), RpcTarget.AllBufferedViaServer, TurnIndex, isAction, actionJson);
                            });
                        }
                    };

                    _diceCount = 1;
                    ChoicedOfPlayer?.Invoke(dialogText, new PopupDialog.PopupButton[] { btn, openStoreBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, false);
                }
                else if (_avoidSingleCards < 1 && _wallet < 0 && IsNotDaring)
                {
                    btn = new PopupDialog.PopupButton()
                    {
                        text = "Okay",
                        onClicked = () =>
                        {
                            _protectedFromSingleInRelationship = false;
                            piece.GoHome(currentPlayer);
                        }
                    };

                    _diceCount = 1;
                    ChoicedOfPlayer?.Invoke(dialogText, new PopupDialog.PopupButton[] { btn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing);
                }
                else if (_protectedFromSingleInRelationship && IsRelationship)
                {
                    dialogText = "Your mate matches your non-negotiable list.";
                    btn = new PopupDialog.PopupButton()
                    {
                        text = "Okay",
                        onClicked = () => { }
                    };

                    ChoicedOfPlayer?.Invoke(dialogText, new PopupDialog.PopupButton[] { btn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing);
                }
                else if (_avoidSingleCards > 0)
                {

                    _avoidSingleCards--;
                    Debug.Log("Used avoid card, not sending user back.");
                    dialogText += "\n\n[Activated Avoid Going to Single Card]";

                    PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                    {
                        text = "Okay",
                        onClicked = () =>
                        {
                            UsedAvoidSingleCard?.Invoke(currentPlayer);
                            _gameStuff.DeleteCardFromInventory(TurnIndex);
                        }
                    };

                    PopupDialog.PopupButton noBtn = new PopupDialog.PopupButton()
                    {
                        text = "No, back to single",
                        onClicked = () =>
                        {
                            _protectedFromSingleInRelationship = false;
                            piece.GoHome(currentPlayer);
                        }
                    };

                    ChoicedOfPlayer?.Invoke(dialogText, new PopupDialog.PopupButton[] { yesBtn, noBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, true);
                }
            }
            else
            {
                if ((_avoidSingleCards < 1 && _wallet > 0 && IsNotDaring) || _protectedFromSingleAllGame || (IsMarriage && _protectedFromSingleInMarriage) || _avoidSingleCards > 0 || (_protectedFromSingleInRelationship && IsRelationship))
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                    });

                }
                else
                {
                    piece.GoHome(currentPlayer);
                    _diceCount = 1;
                    _protectedFromSingleInRelationship = false;
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        _turnManager.EndTurn();
                    });
                }
            }
        }

        private void AdvanceAndGetAvoidCard(BoardPiece piece, Player currentPlayer)
        {
            //Debug.LogError("Advance and Get Avoid going to single card is not added yet. Just advancing instead.");

            BoardManager.Instance.DrawCard("avoid");
            UsedAvoidSingleCard?.Invoke(currentPlayer);

            APIManager.GetUserDetails((user) => 
            {
                APIManager.AddItem("avoidSingle", 1, (inventory) => 
                {
                    User.current.inventory = inventory;
                    StoreController.Instance.UpdateFromPlayerInventory();
                    _gameStuff.GetSpecialCard();
                });
            });

            DOVirtual.DelayedCall(2f, () =>
            {
                if (piece.pathRing == PathRing.Dating)
                    piece.GoToRelationship(currentPlayer);
                else if (piece.pathRing == PathRing.Relationship)
                    piece.GoToMarriage(currentPlayer);

                DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());
            });
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