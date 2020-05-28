using DG.Tweening;
using GameBrewStudios;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public List<GamePlayer> players = new List<GamePlayer>();
    public int turnIndex = 0;

    [SerializeField] RectTransform topPanel;
    [SerializeField] TextMeshProUGUI currentPlayernameLabel;


    [SerializeField] RectTransform bottomPanel;
    [SerializeField] Button bottomButton;
    [SerializeField] TextMeshProUGUI bottomButtonLabel;



    private void Start()
    {
        players.Clear();

        GameManager.OnDiceFinishedRolling += this.GameManager_OnDiceFinishedRolling;

        if (PhotonNetwork.CurrentRoom != null && (int)PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            Debug.Log("Started with multiplayer, building player list.");
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                players.Add(new GamePlayer() { username = p.NickName, player = p });
            }
        }
        else
        {
            Debug.Log("Started without photon, just creating a local player");
            players.Add(new GamePlayer() { username = "Player 1" });
        }

        GameManager.Instance.InitializeBoardPieces(players.Count);

        NextTurn();
    }

    private void GameManager_OnDiceFinishedRolling(int diceScore, string location)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (location == "board")
            {
                turnState = TurnState.RollFinished;

                currentPlayernameLabel.text = "You rolled a " + diceScore;
                DOVirtual.DelayedCall(1f, () =>
                {
                    AdvanceGamePieceFoward(diceScore);
                });
            }
            else if( location == "scenario")
            {
                //HandleProceedAction(diceScore, rollCheck, onPassed, onFailed);
            }
        }
    }

    public void NextTurn()
    {
        didRoll = false;
        turnState = TurnState.ReadyToRoll;

        if (players[turnIndex].turnBank > 0)
        {
            //This player has extra turns available, let them play them now.
            players[turnIndex].turnBank--;
            StartTurn();
        }
        else
        {
            //Go to next player now
            turnIndex++;
            if (turnIndex >= players.Count)
                turnIndex = 0;

            //If the next player is supposed to be skipped, do that now and trigger the NextTurn for the player that comes after that.
            if (players[turnIndex].turnBank < 0)
            {
                //Skip this players turn.
                Debug.LogWarning("Player turn skipped.");

                //Give the player credit for this turn getting skipped.
                players[turnIndex].turnBank++;

                NextTurn();
                return;
            }


            StartTurn();
        }
    }

    public void EndTurn()
    {
        Debug.Log("END OF TURN");

        currentPlayernameLabel.text = "Turn ending..";

        DOVirtual.DelayedCall(1.5f, () =>
        {
            NextTurn();
        });

    }


    public enum TurnState
    {
        ReadyToRoll,
        RollFinished,
        MovingBoardPiece,
        PieceMoved,
        BoardTextShowing
    }

    bool didRoll = false;

    public TurnState turnState;

    void CheckTurnState()
    {
        switch (turnState)
        {
            case TurnState.ReadyToRoll:
                Debug.Log("STATE: ReadyToRoll");
                break;
            case TurnState.RollFinished:
                Debug.Log("STATE: RollFinished");
                break;
            case TurnState.MovingBoardPiece:
                Debug.Log("STATE: MovingBoardPiece");
                break;
            case TurnState.PieceMoved:
                Debug.Log("STATE: PieceMoved");
                turnState = TurnState.BoardTextShowing;
                break;
            case TurnState.BoardTextShowing:
                Debug.Log("STATE: BoardTextShowing");
                break;
        }
    }

    void StartTurn()
    {
        currentPlayernameLabel.text = players[turnIndex].username;

        bottomPanel.anchoredPosition = new Vector2(0, -bottomPanel.sizeDelta.y);

        bottomButton.interactable = false;
        bottomButton.onClick.RemoveAllListeners();
        bottomButton.onClick.AddListener(() =>
        {


            GameManager.Instance.RollDice(1);
            didRoll = true;

            bottomButton.interactable = false;
            bottomPanel.DOAnchorPosY(-bottomPanel.sizeDelta.y, 0.5f);

        });

        bottomButtonLabel.text = "Roll";

        bottomPanel.DOAnchorPosY(0f, 0.5f).OnComplete(() =>
        {
            bottomButton.interactable = true;
        });
    }


    void AdvanceGamePieceFoward(int spaces)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            turnState = TurnState.MovingBoardPiece;
            GameManager.OnGamePieceFinishedMoving += this.GameManager_OnGamePieceFinishedMoving;
            GameManager.Instance.photonView.RPC("RPCMoveBoardPiece", RpcTarget.All, turnIndex, spaces);
        }
    }

    private void GameManager_OnGamePieceFinishedMoving()
    {
        GameManager.OnGamePieceFinishedMoving -= GameManager_OnGamePieceFinishedMoving;
        
        turnState = TurnState.PieceMoved;
        ExecutePathNode((int)GameManager.Instance.pieces[turnIndex].pathRing, GameManager.Instance.pieces[turnIndex].pathIndex);
        
    }

    public GameObject scenarioWindow;
    public TextMeshProUGUI scenarioWindowText;

    public void ExecutePathNode(int path, int pathIndex)
    {
        PathNode node = GameManager.Instance.paths[path].nodes[pathIndex];



        HandlePathNodeAction(node.action, node.nodeText, node.rollCheck, node.rollPassed, node.rollFailed);

        Debug.Log("Finished executing path node: " + node.action.ToString());
    }

    void HandlePathNodeAction(PathNodeAction action, string text, int rollCheck = 0, ProceedAction onPassed = ProceedAction.Nothing, ProceedAction onFailed = ProceedAction.Nothing)
    {
        BoardPiece piece = GameManager.Instance.pieces[turnIndex];

        switch (action)
        {
            case PathNodeAction.GoToKNOWLOVE:
                piece.GoToKnowLove();
                break;
            case PathNodeAction.BackToSingle:

                ScenarioButton btn = new ScenarioButton()
                {
                    text = "Okay",
                    onClick = () =>
                    {
                        piece.GoHome();
                        EndTurn();
                    }
                };

                ShowPrompt(text, new ScenarioButton[] { btn });

                break;
            case PathNodeAction.Scenario:
                if (text.ToLower().Contains("dating"))
                {
                    CardData card = GameManager.Instance.GetRandomCardData("dating");
                    Debug.Log("Got card: " + card.id);
                    GameManager.Instance.DrawCardFromDeck("dating", () =>
                    {
                        Debug.Log("Finished drawing card animation...");
                        ShowCard(card);
                    });
                }
                else if (text.ToLower().Contains("relationship"))
                {
                    CardData card = GameManager.Instance.GetRandomCardData("relationship");
                    Debug.Log("Got card: " + card.id);
                    GameManager.Instance.DrawCardFromDeck("relationship", () =>
                    {
                        Debug.Log("Finished drawing card animation...");
                        ShowCard(card);
                    });
                }
                else if (text.ToLower().Contains("marriage"))
                {
                    CardData card = GameManager.Instance.GetRandomCardData("marriage");
                    Debug.Log("Got card: " + card.id);
                    GameManager.Instance.DrawCardFromDeck("relationship", () =>
                    {
                        Debug.Log("Finished drawing card animation...");
                        ShowCard(card);
                    });
                }
                else
                {
                    Debug.LogError("SHOULD NOT BE ABLE TO HIT THIS CASE IN SCENARIO BRANCH: Scenario type was set to = " + text);
                }
                break;
            case PathNodeAction.BackToSingleOrDisregardBecauseList:

                ScenarioButton btn1 = new ScenarioButton()
                {
                    text = "Back to Single",
                    onClick = () =>
                    {
                        piece.GoHome();
                    }
                };

                ScenarioButton btn2 = new ScenarioButton()
                {
                    text = "They Match My List",
                    onClick = () =>
                    {
                        piece.GoHome();
                    }
                };

                ShowPrompt(text, new ScenarioButton[] { btn1, btn2 });
                break;
            case PathNodeAction.AdvanceAndGetAvoidCard:
                Debug.LogError("Advance and Get Avoid going to single card is not added yet. Just advancing instead.");
                HandlePathNodeAction(PathNodeAction.AdvanceToNextPath, text);
                break;
            case PathNodeAction.AdvanceToNextPath:
                if (piece.pathRing == GameManager.PathRing.Dating)
                {
                    piece.pathRing = GameManager.PathRing.Relationship;
                    piece.GoToRelationship();
                }
                else if (piece.pathRing == GameManager.PathRing.Relationship)
                {
                    piece.pathRing = GameManager.PathRing.Marriage;
                    piece.GoToMarriage();
                }

                EndTurn();
                break;
            case PathNodeAction.LoseTurn:
                players[turnIndex].turnBank -= 1;

                ShowPrompt(text, null);

                break;
            case PathNodeAction.Lose2Turns:
                players[turnIndex].turnBank -= 2;
                ShowPrompt(text, null);
                break;
            case PathNodeAction.Lose3Turns:
                players[turnIndex].turnBank -= 3;
                ShowPrompt(text, null);
                break;
            case PathNodeAction.RollAgain:
                players[turnIndex].turnBank += 1;
                ShowPrompt(text, null);
                break;
            case PathNodeAction.RollAgainTwice:
                players[turnIndex].turnBank += 2;
                ShowPrompt(text, null);
                break;
            case PathNodeAction.RollToProceed:
                ScenarioButton rbtn = new ScenarioButton()
                {
                    text = "Roll",
                    onClick = () =>
                    {
                        GameManager.Instance.RollDice(1, fromScenario);
                    }
                };

                ShowPrompt(text, new ScenarioButton[] { rbtn });
                break;
            case PathNodeAction.RollWith2Dice:
                ShowPrompt(text + " NOTE: ROLLING WITH 2 DICE NOT IMPLEMENTED YET", null);
                break;
            case PathNodeAction.Nothing:
            default:
                ShowPrompt(text, null);
                break;
        }
    }

    void HandleProceedAction(int diceScore, int rollCheck, ProceedAction onPassed, ProceedAction onFailed)
    {
        if (diceScore >= rollCheck)
        {
            ExecuteProceedAction(onPassed, () =>
            {
                EndTurn();
            });
        }
        else
        {
            ExecuteProceedAction(onFailed, () =>
            {
                EndTurn();
            });
        }
    }

    void ExecuteProceedAction(ProceedAction action, System.Action OnFinished)
    {
        switch (action)
        {
            case ProceedAction.BackToSingle:
                GameManager.Instance.pieces[turnIndex].GoHome();
                OnFinished?.Invoke();
                break;
            case ProceedAction.GoToRelationship:

                GameManager.Instance.pieces[turnIndex].GoToRelationship(() =>
               {
                   Debug.Log("Finished moving to relationship path.");
                   OnFinished?.Invoke();
               });

                break;
            case ProceedAction.GoToMarriage:
                Debug.Log("MOVING TO MARRIAGE PATH");
                GameManager.Instance.pieces[turnIndex].GoToMarriage(() =>
                {
                    Debug.Log("Finished moving to marriage path.");
                    OnFinished?.Invoke();
                });
                break;
            case ProceedAction.GoToKNOWLOVE:
                Debug.Log("YOU WON THE GAME!!!!!");
                GameManager.Instance.pieces[turnIndex].GoToKnowLove(() =>
                {
                    Debug.Log("Finished moving piece to KnowLove space.");
                    ShowPrompt("YOU WON THE GAME!!!", null);
                });
                break;
            case ProceedAction.LoseATurn:
                players[turnIndex].turnBank -= 1;
                OnFinished?.Invoke();
                break;
            case ProceedAction.LoseTwoTurns:
                players[turnIndex].turnBank -= 2;
                OnFinished?.Invoke();
                break;
            case ProceedAction.LoseThreeTurns:
                players[turnIndex].turnBank -= 3;
                OnFinished?.Invoke();
                break;
            case ProceedAction.Nothing:
            default:
                OnFinished?.Invoke();
                break;
        }
    }

    public class ScenarioButton
    {
        public string text;
        public System.Action onClick;
    }


    [SerializeField]
    RectTransform cardUIObj;

    [SerializeField]
    Button cardUIButton;

    [SerializeField]
    TextMeshProUGUI cardUIText;

    public void ShowCard(CardData card)
    {
        cardUIObj.anchoredPosition = new Vector2(cardUIObj.anchoredPosition.x, -1080f);

        cardUIText.text = card.text + " (" + card.parentheses + ")";

        players[turnIndex].drawnCards.Add(card.id);

        cardUIButton.onClick.RemoveAllListeners();

        if (card.isPrompt)
        {
            Debug.Log("Card is a prompt card");
            cardUIButton.onClick.AddListener(() =>
            {
                HideCard();
                ShowPrompt(card.promptMessage, GetPromptButtons(card.promptButtons));
            });
        }
        else
        {
            cardUIButton.onClick.AddListener(() =>
            {
                HideCard();

                HandlePathNodeAction(card.action, card.parentheses, card.rollCheck, card.rollPassed, card.rollFailed);
            });
        }

        //Bring the card up onto the screen
        cardUIObj.DOAnchorPosY(0f, 0.25f).OnComplete(() =>
        {
            CanvasGroup cg = cardUIObj.GetComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            Debug.Log("Card fully visible.");
        });
    }

    public void HideCard()
    {
        CanvasGroup cg = cardUIObj.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        cardUIObj.DOAnchorPosY(-1080f, 0.25f).OnComplete(() =>
        {
            Debug.Log("Card fully hidden.");
        });
    }

    public ScenarioButton[] GetPromptButtons(CardPromptButton[] cardButtons)
    {
        List<ScenarioButton> buttons = new List<ScenarioButton>();
        for (int i = 0; i < cardButtons.Length; i++)
        {
            ProceedAction action = cardButtons[i].action;
            ScenarioButton btn = new ScenarioButton()
            {
                text = cardButtons[i].text,
                onClick = () =>
                {
                    //GameManager.Instance.pieces[turnIndex].GoHome();
                    ExecuteProceedAction(action, () => { EndTurn(); });
                }
            };

            buttons.Add(btn);
        }

        return buttons.ToArray();
    }

    public void ShowPrompt(string text, ScenarioButton[] buttons)
    {
        promptWindow.anchoredPosition = new Vector2(promptWindow.anchoredPosition.x, -1080f);

        if (buttons == null)
        {
            Debug.Log("No buttons");
            buttons = new ScenarioButton[]
            {
                new ScenarioButton()
                {
                    text = "Okay",
                    onClick = () => { EndTurn(); }
                }
            };
        }

        foreach (Transform child in promptButtonContainer)
            Destroy(child.gameObject);

        foreach (ScenarioButton button in buttons)
        {
            GameObject btnObj = Instantiate(promptButtonPrefab, promptButtonContainer);
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                HidePrompt();
                button.onClick?.Invoke();
            });

            btn.GetComponentInChildren<TextMeshProUGUI>().text = button.text;
        }

        promptWindowText.text = text;

        promptWindow.DOAnchorPosY(0f, 0.5f).OnComplete(() =>
        {
            CanvasGroup cg = promptWindow.GetComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
        });


        Debug.Log("ShowPrompt: " + text);
    }


    [SerializeField]
    RectTransform promptWindow;

    [SerializeField]
    TextMeshProUGUI promptWindowText;

    [SerializeField]
    Transform promptButtonContainer;

    [SerializeField]
    GameObject promptButtonPrefab;

    public void HidePrompt()
    {
        promptWindow.DOAnchorPosY(-1080f, 0.5f).OnComplete(() =>
        {
            CanvasGroup cg = promptWindow.GetComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;
        });

    }

    private void Update()
    {
        CheckTurnState();
    }
}
