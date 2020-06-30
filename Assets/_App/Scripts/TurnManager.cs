using DG.Tweening;

using GameBrewStudios;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks, IPunObservable
{

    public int turnIndex = 0;

    public GameUI gameUI;
    public enum TurnState
    {
        ReadyToRoll,
        RollFinished,
        MovingBoardPiece,
        PieceMoved,
        BoardTextShowing,
        GameOver,
        TurnEnding
    }


    public static event System.Action<Player, ExitGames.Client.Photon.Hashtable> OnReceivedPlayerProps;

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.UserId == PhotonNetwork.LocalPlayer.UserId)
        {
            Debug.Log("Player Properties Updated: " + targetPlayer.CustomProperties.ToStringFull());
            if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                gameUI.SetStats();
            }


        }

        OnReceivedPlayerProps?.Invoke(targetPlayer, changedProps);
    }

    public static TurnManager Instance;

    private void Start()
    {
        Instance = this;
        playersReady = 0;
        turnIndex = 0;

        Debug.Log("TurnManager.Start()");
        gameUI.RPC_HideBottomForPlayer();

        if (PhotonNetwork.IsMasterClient)
        {
            NetworkManager.OnReadyToStart += ShowUserPickCard;
        }
    }

    int playersReady = 0;

    void ShowUserPickCard()
    {
        DOVirtual.DelayedCall(2f, () =>
        {
            CameraManager.Instance.SetCamera(2);

            gameUI.ShowPickCard();

        });

    }

    public void ReadyUp()
    {
        gameUI.BuildProgressBarList();
        photonView.RPC("RPC_ReadyUp", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_ReadyUp()
    {
        if (PhotonNetwork.IsMasterClient && playersReady < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            playersReady++;

            if (playersReady >= PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                //if(players == null || players.Count == 0)
                //{
                //    players = NetworkManager.Instance.players;
                //}

                int firstPlayerIndex = UnityEngine.Random.Range(0, NetworkManager.Instance.players.Count);
                turnIndex = firstPlayerIndex;

                List<string> playerNames = new List<string>();
                int j = firstPlayerIndex;


                Debug.Log("First player index = " + firstPlayerIndex);
                Debug.Log("Player Count: " + NetworkManager.Instance.players.Count);
                for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                {

                    Debug.Log("Adding " + NetworkManager.Instance.players[j].NickName);
                    playerNames.Add(NetworkManager.Instance.players[j].NickName);
                    j++;
                    if (j >= NetworkManager.Instance.players.Count)
                        j = 0;
                }

                Debug.Log("Sending " + playerNames.Count + " names for slot machine thing");
                gameUI.ShowPlayerSelection(playerNames.ToArray());


            }
        }
    }

    internal void PlayerDidElapseOneYear()
    {
        ExitGames.Client.Photon.Hashtable playerProps = NetworkManager.Instance.players[turnIndex].CustomProperties;

        //TODO: playerProps was null.....
        int yearsElapsed = (int)playerProps["yearsElapsed"];
        yearsElapsed++;

        playerProps["yearsElapsed"] = yearsElapsed;

        NetworkManager.Instance.players[turnIndex].SetCustomProperties(playerProps);

        Debug.Log("Years Elapsed increased for " + NetworkManager.Instance.players[turnIndex].NickName + " to " + yearsElapsed);
    }

    private void OnDiceFinishedRolling(int diceScore, string location)
    {
        Debug.Log("OnDiceFinishedRolling()");
        photonView.RPC("RPC_OnDiceFinished", RpcTarget.All, diceScore, location);

    }

    [PunRPC]
    public void RPC_OnDiceFinished(int diceScore, string location)
    {
        Debug.Log("RPC_OnDiceFinished()");
        if (PhotonNetwork.IsMasterClient)
        {


            if (location == "board")
            {
                Debug.Log("Handling board roll...");
                turnState = TurnState.RollFinished;

                string playerName = NetworkManager.Instance.players[turnIndex].NickName;

                gameUI.SetTopText($"{playerName} rolled a {diceScore.ToString("n0")}", "RESULT");
                DOVirtual.DelayedCall(1f, () =>
                {
                    CameraManager.Instance.SetCamera(0);
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        AdvanceGamePieceFoward(diceScore);
                    });
                });
            }
            else if (location == "scenario")
            {
                Debug.Log("Handling Scenario roll...");
                CameraManager.Instance.SetCamera(0);
                HandleScenarioRollResult(diceScore, currentRollCheck, currentOnPassed, currentOnFailed);
            }
            else
            {
                Debug.Log("Wtf happened here....");
            }
        }
    }



    public void RollDice(int amount, string location)
    {
        photonView.RPC("RPC_RollDice", RpcTarget.All, amount, location);

    }
    [PunRPC]
    public void RPC_RollDice(int amount, string location)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CameraManager.Instance.SetCamera(1);
            BoardManager.Instance.RollDice(amount, location);
            StartCoroutine(WaitForDiceToRoll());
        }
    }

    private void OnGUI()
    {

        //if (Application.isEditor)
        //{
        //    GUILayout.Label($"Turn Index: {turnIndex}");
        //    foreach (Player player in NetworkManager.Instance.players)
        //    {
        //        GUILayout.Label($"{player.NickName}:  turnBank = {player.CustomProperties["turnBank"]}");
        //    }
        //}

    }

    internal void ReallyStartGame()
    {
        StartGame();
    }

    private void StartGame()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("didShowStatsForAll"))
        {
            ExitGames.Client.Photon.Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
            roomProps.Add("didShowStatsForAll", true);
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
            gameUI.ShowStatsPanelForEveryone();
        }


        CameraManager.Instance.SetCamera(0);

        Debug.Log("StartGame();");


        //players.Clear();
        if (PhotonNetwork.CurrentRoom != null && (int)PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            Debug.Log("Started with multiplayer, building player list.");

            BoardManager.Instance.InitializeBoardPieces((int)PhotonNetwork.CurrentRoom.PlayerCount);

            //turnIndex = 0;
            NextTurn();
        }
        else
        {
            Debug.LogError("Current Room == null ????");
        }


    }

    void StartTurn()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        gameUI.SetTopText(NetworkManager.Instance.players[turnIndex].NickName);


        if (BoardManager.Instance.pieces[turnIndex].pathRing == PathRing.Dating || BoardManager.Instance.pieces[turnIndex].pathRing == PathRing.Home)
        {
            Debug.Log("Showing List Panel");
            gameUI.ForceShowListPanel(NetworkManager.Instance.players[turnIndex]);
        }
        else
        {
            Debug.Log("Showing Bottom for " + turnIndex + " Name: " + NetworkManager.Instance.players[turnIndex].NickName);
            gameUI.ShowBottomForPlayer(turnIndex);
        }
    }

    public void NextTurn()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("WTF IS WRONG WITH YOU, YOU AINT GOT PERMISSION FOR THAT");
            return;
        }

        Debug.Log("NextTurn()");
        didRoll = false;
        turnTimer = 30f;

        turnState = TurnState.ReadyToRoll;


        ExitGames.Client.Photon.Hashtable playerProperties = NetworkManager.Instance.players[turnIndex].CustomProperties;
        int turnBank = (int)playerProperties["turnBank"];
        //This first condition will only run if the NextTurn is called and the current player had turnBank credits, giving them another turn instantly
        if (turnBank > 0)
        {
            Debug.Log("TurnBank > 0");
            //This player has extra turns available, let them play them now.
            turnBank--;
            playerProperties["turnBank"] = turnBank;
            NetworkManager.Instance.players[turnIndex].SetCustomProperties(playerProperties);
            StartTurn();
        }
        else
        {


            //Update the playerProperties reference to point to the right player now that we've moved on to the next player in this block.
            playerProperties = NetworkManager.Instance.players[turnIndex].CustomProperties;

            //do the same for turnBank
            turnBank = (int)playerProperties["turnBank"];

            //If the next player is supposed to be skipped, do that now and trigger the NextTurn for the player that comes after that.
            if (turnBank < 0)
            {
                Debug.Log("TurnBank < 0");


                //Give the player credit for this turn getting skipped.
                turnBank++;
                playerProperties["turnBank"] = turnBank;
                NetworkManager.Instance.players[turnIndex].SetCustomProperties(playerProperties);

                //Skip this players turn.
                Debug.LogWarning($"Player {NetworkManager.Instance.players[turnIndex].NickName} turn skipped. Turn Bank is: {turnBank}");
                DOVirtual.DelayedCall(0.25f, () => { EndTurn(true); });


                return;
            }
            else
            {
                Debug.Log("TurnBank == 0");
            }

            StartTurn();
        }
    }

    public void EndTurn(bool isSkip = false)
    {
        Debug.Log("<color=Red>#####################  END OF TURN ####################</color>");
        turnState = TurnState.TurnEnding;
        if (isSkip)
        {
            gameUI.SetTopText("Skipping " + NetworkManager.Instance.players[turnIndex].NickName);
        }
        else
        {
            gameUI.SetTopText("Turn ending..");
        }


        if ((int)NetworkManager.Instance.players[turnIndex].CustomProperties["turnBank"] < 1)
        {
            turnIndex++;

            if (turnIndex >= NetworkManager.Instance.players.Count)
                turnIndex = 0;
        }
        DOVirtual.DelayedCall(1.5f, () =>
        {
            NextTurn();
        });

    }




    bool didRoll = false;

    public TurnState turnState = TurnState.TurnEnding;

    void CheckTurnState()
    {
        switch (turnState)
        {
            case TurnState.ReadyToRoll:
                //Debug.Log("STATE: ReadyToRoll");
                break;
            case TurnState.RollFinished:
                //Debug.Log("STATE: RollFinished");
                break;
            case TurnState.MovingBoardPiece:
                //Debug.Log("STATE: MovingBoardPiece");
                break;
            case TurnState.PieceMoved:
                //Debug.Log("STATE: PieceMoved");
                turnState = TurnState.BoardTextShowing;
                break;
            case TurnState.BoardTextShowing:
                //Debug.Log("STATE: BoardTextShowing");
                break;
            case TurnState.GameOver:
                Debug.Log("GameIsOver");
                //gameUI.ShowBottomForPlayer(-1);
                break;
        }
    }
    public void GameOver(string playerName)
    {
        gameUI.HideBottomForEveryone();
        photonView.RPC("RPC_GameOver", RpcTarget.All, playerName);
    }

    [PunRPC]
    public void RPC_GameOver(string playerName)
    {

        gameUI.ShowGameOver(playerName);

    }



    void AdvanceGamePieceFoward(int spaces)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            turnState = TurnState.MovingBoardPiece;
            gameUI.SetTopText("Moving...", "PLEASE WAIT");
            StartCoroutine(WaitForGamepieceToMove());
            BoardManager.Instance.photonView.RPC("RPC_MoveBoardPiece", RpcTarget.All, turnIndex, spaces);
        }
    }

    IEnumerator WaitForGamepieceToMove()
    {
        BoardManager.Instance.movingBoardPiece = true;

        while (BoardManager.Instance.movingBoardPiece == true)
        { yield return null; }

        GameManager_OnGamePieceFinishedMoving();
    }

    private void GameManager_OnGamePieceFinishedMoving()
    {
        turnState = TurnState.PieceMoved;
        gameUI.SetTopText("", "");
        ExecutePathNode((int)BoardManager.Instance.pieces[turnIndex].pathRing, BoardManager.Instance.pieces[turnIndex].pathIndex);
    }

    public void ExecutePathNode(int path, int pathIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Tried running ExecutePathNode on non-master client");
            return;
        }


        PathNode node = BoardManager.Instance.paths[path].nodes[pathIndex];

        HandlePathNodeAction(node.action, node.nodeText, node.rollCheck, node.rollPassed, node.rollFailed);

        Debug.Log("Finished executing path node: " + node.action.ToString());
    }

    public void HandlePathNodeAction(PathNodeAction action, string text, int rollCheck = 0, ProceedAction onPassed = ProceedAction.Nothing, ProceedAction onFailed = ProceedAction.Nothing, bool skipPrompt = false)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Tried running HandlePathNodeAction on non-master client");
            return;
        }

        BoardPiece piece = BoardManager.Instance.pieces[turnIndex];

        Player currentPlayer = NetworkManager.Instance.players[turnIndex];

        ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;
        int turnBank = (int)playerProperties["turnBank"];
        int diceCount = (int)playerProperties["diceCount"];
        int avoidSingleCards = (int)playerProperties["avoidSingleCards"];
        int dateCount = (int)playerProperties["dateCount"];
        bool protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];

        switch (action)
        {
            case PathNodeAction.GoToKNOWLOVE:
                piece.GoToKnowLove(currentPlayer);
                gameUI.SetTopText("Won the Game!", currentPlayer.NickName);
                turnState = TurnState.GameOver;



                break;
            case PathNodeAction.BackToSingle:

                List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();

                if (avoidSingleCards > 0)
                {
                    avoidSingleCards--;
                    if (!skipPrompt)
                    {
                        PopupDialog.PopupButton btn = new PopupDialog.PopupButton()
                        {
                            text = "Okay",
                            onClicked = () =>
                            {

                            }
                        };

                        text += " [ Avoid Going To Single Card Activated ]";

                        buttons.Add(btn);
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
                            }
                        };
                        buttons.Add(btn);

                    }
                    else
                    {
                        piece.GoHome(currentPlayer);
                    }

                }


                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, buttons.ToArray(), currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
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
                        gameUI.ShowCard(card);
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
                        gameUI.ShowCard(card);
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
                        gameUI.ShowCard(card);
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

                    if (!protectedFromSingleInRelationship && avoidSingleCards < 1)
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
                        gameUI.ShowPrompt(dialogText, new PopupDialog.PopupButton[] { btn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
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

                        gameUI.ShowPrompt(dialogText, new PopupDialog.PopupButton[] { btn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                    }
                    else if (avoidSingleCards > 0)
                    {

                        avoidSingleCards--;
                        Debug.Log("Used avoid card, not sending user back.");
                        dialogText += "\n\n[Activated Avoid Going to Single Card]";
                        btn = new PopupDialog.PopupButton()
                        {
                            text = "Okay",
                            onClicked = () =>
                            {
                                //Do nothing.
                            }
                        };
                        gameUI.ShowPrompt(dialogText, new PopupDialog.PopupButton[] { btn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                    }



                    
                }
                else
                {
                    if (!protectedFromSingleInRelationship)
                    {
                        piece.GoHome(currentPlayer);
                        diceCount = 1;
                    }

                }

                break;
            case PathNodeAction.AdvanceAndGetAvoidCard:
                //Debug.LogError("Advance and Get Avoid going to single card is not added yet. Just advancing instead.");

                avoidSingleCards++;


                BoardManager.Instance.DrawCard("avoid");
                gameUI.AvoidSingleCardAnimation(currentPlayer);

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

                    DOVirtual.DelayedCall(1f, () => EndTurn());
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
                DOVirtual.DelayedCall(1f, () => EndTurn());

                break;
            case PathNodeAction.AdvanceToRelationshipWithProtectionFromSingle:
                protectedFromSingleInRelationship = true;

                piece.GoToRelationship(currentPlayer);

                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
                }

                break;
            case PathNodeAction.LoseTurn:

                if (turnBank > 0) turnBank = 0;

                turnBank -= 1;

                playerProperties["turnBank"] = turnBank;

                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
                }

                break;
            case PathNodeAction.Lose2Turns:

                if (turnBank > 0)
                    turnBank = 0;
                
                turnBank -= 2;

                playerProperties["turnBank"] = turnBank;

                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
                }
                break;
            case PathNodeAction.Lose3Turns:
                if (turnBank > 0) turnBank = 0;

                turnBank -= 3;

                playerProperties["turnBank"] = turnBank;

                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
                }
                break;
            case PathNodeAction.RollAgain:
                turnBank += 1;
                playerProperties["turnBank"] = turnBank;


                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
                }
                break;
            case PathNodeAction.RollAgainTwice:
                turnBank += 2;
                playerProperties["turnBank"] = turnBank;
                currentPlayer.SetCustomProperties(playerProperties);

                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
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
                            currentRollCheck = rollCheck;
                            currentOnPassed = onPassed;
                            currentOnFailed = onFailed;
                            RollDice(diceCount, "scenario");
                        }
                    };

                    gameUI.ShowPrompt(text, new PopupDialog.PopupButton[] { rbtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing, false);
                }
                else
                {
                    currentRollCheck = rollCheck;
                    currentOnPassed = onPassed;
                    currentOnFailed = onFailed;
                    RollDice(diceCount, "scenario");
                }

                break;
            case PathNodeAction.RollWith2Dice:


                turnBank += 1; //Award the extra turn
                diceCount = 2;

                if (!skipPrompt)
                {
                    gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => EndTurn());
                }

                break;
            case PathNodeAction.Nothing:
            default:
                gameUI.ShowPrompt(text, null, currentPlayer, 1 + (int)BoardManager.Instance.pieces[turnIndex].pathRing);
                break;
        }

        playerProperties["turnBank"] = turnBank;
        playerProperties["diceCount"] = diceCount;
        playerProperties["dateCount"] = dateCount;
        playerProperties["protectedFromSingleInRelationship"] = protectedFromSingleInRelationship;
        playerProperties["avoidSingleCards"] = avoidSingleCards;
        currentPlayer.SetCustomProperties(playerProperties);
    }

    IEnumerator WaitForDiceToRoll()
    {
        while (!BoardManager.Instance.diceFinishedRolling)
        {
            yield return null;
        }

        OnDiceFinishedRolling(BoardManager.Instance.diceScore, BoardManager.Instance.diceRollLocation);
    }

    public int currentRollCheck;
    public ProceedAction currentOnPassed, currentOnFailed;


    void HandleScenarioRollResult(int diceScore, int rollCheck, ProceedAction onPassed, ProceedAction onFailed)
    {
        if (diceScore >= rollCheck)
        {
            ExecuteProceedAction(onPassed, () =>
            {
                Debug.Log("PROCEED ACTION FINISHED IN PASSED STATE");
                EndTurn();
            });
        }
        else
        {
            ExecuteProceedAction(onFailed, () =>
            {
                Debug.Log("PROCEED ACTION FINISHED IN FAILED STATE");
                EndTurn();
            });
        }
    }

    [ContextMenu("Make Current Player Win")]
    public void MakeCurrentPlayerWin()
    {
        if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

        ExecuteProceedAction(ProceedAction.GoToKNOWLOVE, () => { });
    }

    [ContextMenu("SendToRelationship")]
    public void SendToRelationship()
    {
        if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

        ExecuteProceedAction(ProceedAction.GoToRelationship, () => { });
    }

    [ContextMenu("SendToMarriage")]
    public void SendToMarriage()
    {
        if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

        ExecuteProceedAction(ProceedAction.GoToMarriage, () => { });
    }

    [ContextMenu("Back to Single")]
    public void SendToSingle()
    {
        if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

        ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
    }

    [ContextMenu("AdvanceToNextPath")]
    public void SendToAdvance()
    {
        if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

        HandlePathNodeAction(PathNodeAction.AdvanceToNextPath, "Advanced by Debug Code");
    }

    [ContextMenu("AdvanceToNextPath And Get Card")]
    public void SendToAdvanceWithCard()
    {
        if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

        HandlePathNodeAction(PathNodeAction.AdvanceAndGetAvoidCard, "Advanced by Debug Code and you got an avoid card");
    }

    [ContextMenu("Give Two Dice")]
    public void GiveTwoDice()
    {
        if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

        ExitGames.Client.Photon.Hashtable props = NetworkManager.Instance.players[turnIndex].CustomProperties;
        props["diceCount"] = 2;
        NetworkManager.Instance.players[turnIndex].SetCustomProperties(props);
    }

    public void ExecuteProceedAction(ProceedAction action, System.Action OnFinished)
    {
        Player currentPlayer = NetworkManager.Instance.players[turnIndex];
        Debug.Log("Executing proceed action for player: " + currentPlayer.NickName);
        BoardPiece playerBoardPiece = BoardManager.Instance.pieces[turnIndex];
        ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;
        int turnBank = (int)playerProperties["turnBank"];
        int diceCount = (int)playerProperties["diceCount"];

        int avoidSingleCards = (int)playerProperties["avoidSingleCards"];
        bool protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];

        switch (action)
        {
            case ProceedAction.AdvanceToRelationshipWithProtectionFromSingle:
                protectedFromSingleInRelationship = true;
                
                playerBoardPiece.GoToRelationship(currentPlayer, () =>
                {
                    Debug.Log("Finished moving to relationship path.");
                    OnFinished?.Invoke();
                });

                break;

            case ProceedAction.BackToSingle:

                if (avoidSingleCards > 0)
                {
                    avoidSingleCards--;
                }
                else
                {


                    diceCount = 1;
                    playerBoardPiece.GoHome(currentPlayer);
                }

                OnFinished?.Invoke();
                break;
            case ProceedAction.BackToSingleAndLoseATurn:


                if (avoidSingleCards > 0)
                    avoidSingleCards--;
                else
                {

                    diceCount = 1;
                    playerBoardPiece.GoHome(currentPlayer);
                }

                if (turnBank > 0) turnBank = 0;

                turnBank -= 1;

                OnFinished?.Invoke();
                break;
            case ProceedAction.GoToRelationship:

                playerBoardPiece.GoToRelationship(currentPlayer, () =>
               {
                   Debug.Log("Finished moving to relationship path.");
                   OnFinished?.Invoke();
               });

                break;
            case ProceedAction.GoToMarriage:

                Debug.Log("MOVING TO MARRIAGE PATH");
                playerBoardPiece.GoToMarriage(currentPlayer, () =>
                {
                    Debug.Log("Finished moving to marriage path.");
                    OnFinished?.Invoke();
                });
                break;
            case ProceedAction.GoToKNOWLOVE:
                Debug.Log("YOU WON THE GAME!!!!!");
                turnState = TurnState.GameOver;

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
                OnFinished?.Invoke();
                break;
            case ProceedAction.LoseTwoTurns:
                if (turnBank > 0) turnBank = 0;

                turnBank -= 2;
                OnFinished?.Invoke();
                break;
            case ProceedAction.LoseThreeTurns:
                if (turnBank > 0) turnBank = 0;
                turnBank -= 3;
                OnFinished?.Invoke();
                break;
            case ProceedAction.Nothing:
            default:
                OnFinished?.Invoke();
                break;
        }

        playerProperties["turnBank"] = turnBank;
        playerProperties["diceCount"] = diceCount;
        playerProperties["avoidSingleCards"] = avoidSingleCards;
        playerProperties["protectedFromSingleInRelationship"] = protectedFromSingleInRelationship;
        currentPlayer.SetCustomProperties(playerProperties);
    }















    private void Update()
    {
        CheckTurnState();
        UpdateTurnTimer();

    }

    public float turnTimer = 30f;

    void UpdateTurnTimer()
    {
        if (PhotonNetwork.IsMasterClient && turnState != TurnState.GameOver && turnState != TurnState.TurnEnding)
        {

            if (NetworkManager.Instance.players[turnIndex].IsInactive)
            {
                turnTimer -= Time.deltaTime;
            }
            else
            {
                turnTimer = 31f;
            }


            if (turnTimer <= 0)
            {
                EndTurn();
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(turnIndex);
            stream.SendNext(turnTimer);
            stream.SendNext((int)turnState);
        }
        else
        {
            turnIndex = (int)stream.ReceiveNext();
            turnTimer = (float)stream.ReceiveNext();
            turnState = (TurnState)(int)stream.ReceiveNext();
        }
    }
}
