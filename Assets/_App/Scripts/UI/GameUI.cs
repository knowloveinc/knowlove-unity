using DG.Tweening;
using GameBrewStudios;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class GameUI : MonoBehaviourPun
{
    [SerializeField] TurnManager TurnManager;

    [SerializeField] RectTransform topPanel;
    [SerializeField] TextMeshProUGUI topText, topTitle;


    [SerializeField] RectTransform bottomPanel;
    [SerializeField] Button bottomButton;
    [SerializeField] TextMeshProUGUI bottomButtonLabel;

    [SerializeField] StatsPanel statsPanel;

    [SerializeField]
    RectTransform cardUIObj;

    [SerializeField]
    Button cardUIButton;

    [SerializeField]
    TextMeshProUGUI cardUIText;


    [SerializeField]
    GameObject playerProgressTemplate;

    [SerializeField]
    Transform playerProgressContainer;

    [SerializeField]
    RectTransform menuRect;

    [SerializeField]
    TextMeshProUGUI playerSlotText;

    private void Start()
    {
        playerProgressTemplate.SetActive(false);
    }

    public void CloseMenu()
    {
        menuRect.DOAnchorPosX(-menuRect.sizeDelta.x, 0.2f);
    }

    public void OpenMenu()
    {
        menuRect.DOAnchorPosX(0, 0.2f);
    }


    internal void ShowPickCard()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_ShowPickCard", RpcTarget.All);
        }
    }

    [System.Serializable]
    public class ListCard
    {
        public string text;
    }

    public void ShowPlayerSelection(string[] playerNames)
    {
        Debug.Log("SENDING SHOW SLOTS MESSAGE");
        //slotMachine.Init();
        photonView.RPC("RPC_ShowPlayerSelection", RpcTarget.All, playerNames, 0);
    }

    [PunRPC]
    public void RPC_ShowPlayerSelection(string[] playerNames, int notUsed)
    {
        Debug.Log("SHOWING SLOTS" + playerNames.Length);

        
        StartCoroutine(DOShowPlayerNames(playerNames));
    }

    [SerializeField]
    SlotMachine slotMachine;

    IEnumerator DOShowPlayerNames(string[] playerNames)
    {
        Debug.Log("SHOW SLOTS COROUTINE");
        float delay = 0.001f;
        int j = 0;

        CanvasGroup group = playerSlotText.GetComponent<CanvasGroup>();
        group.DOFade(1f, 0.1f);

        if (PhotonNetwork.CurrentRoom.MaxPlayers > 1)
        {

            for (int i = 0; i < 151; i++)
            {
                Debug.Log(playerNames.Length);


                playerSlotText.text = playerNames[j];
                j++;

                if (j >= playerNames.Length)
                    j = 0;

                if (i > 100)
                {
                    delay += 0.001f;
                }

                yield return new WaitForSeconds(delay);
            }

            group.DOFade(0f, 0.5f).SetDelay(2f);

        }
        else
        {
            playerSlotText.text = "Get Ready!";
            playerSlotText.transform.GetChild(0).gameObject.SetActive(false);
            group.DOFade(0f, 0.5f).SetDelay(2f);
        }


        yield return new WaitForSeconds(2f);
        if (PhotonNetwork.IsMasterClient)
        {
            TurnManager.Instance.ReallyStartGame();
        }
    }

    public ListCard[] listCards;


    public ListCard selectedCard;

    [PunRPC]
    public void RPC_ShowPickCard()
    {
        CanvasLoading.Instance.ForceHide();
        selectedCard = listCards[UnityEngine.Random.Range(0, listCards.Length)];

        ShowCardPickerPanel();
    }

    internal void ShowStatsPanelForEveryone()
    {
        statsPanel.ShowForEveryone();
    }

    [SerializeField]
    Transform cardPickerPanel;

    [SerializeField]
    RectTransform listCardContainer;

    [SerializeField]
    RectTransform[] listCardUIObjs;

    public void ShowCardPickerPanel()
    {
        //put the container below the screen and swipe it up into view.
        listCardContainer.anchoredPosition = new Vector3(0f, -1080f, 0f);
        listCardContainer.DOAnchorPosY(0f, 0.5f);

        cardPickerPanel.gameObject.SetActive(true);



        for (int i = 0; i < listCardUIObjs.Length; i++)
        {
            listCardUIObjs[i].GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    [SerializeField]
    TextMeshProUGUI listText;

    [SerializeField]
    GameObject listCardButton;

    public void OnListCardSelected(int index)
    {
        listText.text = selectedCard.text.Replace("\\n", "\n");

        for (int i = 0; i < listCardUIObjs.Length; i++)
        {
            if (i != index)
            {
                listCardUIObjs[i].DOAnchorPosY(-1080f, 0.5f);
                listCardUIObjs[i].DOScale(0f, 0.2f);
            }
            else
            {
                listCardUIObjs[i].DOAnchorPos(new Vector2(0f, 0f), 0.2f);
                listCardUIObjs[i].DORotate(new Vector3(0f, 0f, 0f), 0.2f);
                listCardUIObjs[i].DOScale(1.5f, 0.2f);
                listCardUIObjs[i].GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetDelay(1f).OnComplete(() =>
                {
                    listCardButton.SetActive(true);

                    DOVirtual.DelayedCall(0.3f, () =>
                    {
                        didReadyUp = false;
                        ShowListPanel(true);
                        cardPickerPanel.gameObject.SetActive(false);
                    });
                });
            }
        }

    }

    [SerializeField]
    GameObject listPanel;

    public void ForceShowListPanel(Player player)
    {
        photonView.RPC("RPC_ForceShowListPanel", player);
    }

    bool showBottomAfterClosingListPanel = false;

    [PunRPC]
    public void RPC_ForceShowListPanel()
    {
        showBottomAfterClosingListPanel = true;
        ShowListPanel(true);

    }

    public void ShowListPanel()
    {
        ShowListPanel(true);
    }

    bool didReadyUp = false;

    public void ShowListPanel(bool showHelper = false)
    {
        listPanel.gameObject.SetActive(true);
        listPanel.transform.Find("helper").gameObject.SetActive(showHelper);

    }

    public void HideListPanel()
    {
        Debug.Log("<color=Red>HideListPanel()</color>");

        listPanel.gameObject.SetActive(false);

        if (!didReadyUp)
        {
            didReadyUp = true;
            TurnManager.ReadyUp();
        }

        if (showBottomAfterClosingListPanel)
        {
            showBottomAfterClosingListPanel = false;
            int index = NetworkManager.Instance.players.IndexOf(PhotonNetwork.LocalPlayer);
            Debug.Log("<color=Cyan>Current Player Index = " + index + "</color>");
            ShowBottomForPlayer(index);

        }



    }

    public void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void LeaveMatch()
    {
        PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
        {
            new PopupDialog.PopupButton()
            {
                text = "Yes, Leave",
                onClicked = () =>
                {
                    PhotonNetwork.LeaveRoom();
                },
                buttonColor = PopupDialog.PopupButtonColor.Red

            },
            new PopupDialog.PopupButton()
            {
                text = "Nevermind",
                onClicked = () =>
                {

                }
            }
        };

        PopupDialog.Instance.Show("Really Leave Game?", "If you leave this match, it will end the game for all players. Are you sure you want to leave? (Please be considerate of others.)", buttons);
    }

    public void BuildProgressBarList()
    {
        photonView.RPC("RPC_BuildProgressBarList", RpcTarget.All);
    }
    [PunRPC]
    public void RPC_BuildProgressBarList()
    {
        //Destroy everything but the template for a clean slate.
        foreach (Transform child in playerProgressContainer)
        {
            if (child != playerProgressTemplate.transform)
                Destroy(child.gameObject);
        }

        for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
        {
            GameObject obj = Instantiate(playerProgressTemplate, playerProgressContainer);
            obj.SetActive(true);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = NetworkManager.Instance.players[i].NickName;

            PlayerProgress p = obj.GetComponent<PlayerProgress>();
            p.Init(NetworkManager.Instance.players[i]);

        }
    }



    public void SetTopText(string text, string title = "TURN")
    {
        photonView.RPC("RPC_SetTopText", RpcTarget.All, text, title);
    }

    [PunRPC]
    public void RPC_SetTopText(string text, string title)
    {
        topText.text = text;
        topTitle.text = title;

        if (topTitle.text.ToLower().Contains("turn") && topText.text.ToLower().Contains(PhotonNetwork.LocalPlayer.NickName.ToLower()))
        {
            topText.text = "YOUR TURN";
        }

        if (topText.text.ToLower().Contains("rolled a "))
        {
            topText.text = topText.text.Replace(PhotonNetwork.LocalPlayer.NickName, "You");
        }

        topText.text.Replace("[host]", "");

    }

    private void Update()
    {
        SetStats();
        UpdateTimerText();
    }

    [SerializeField]
    TextMeshProUGUI timerText;

    void UpdateTimerText()
    {
        if (TurnManager.Instance.turnState != TurnManager.TurnState.GameOver && TurnManager.Instance.turnState != TurnManager.TurnState.TurnEnding)
        {
            if (TurnManager.Instance.turnTimer <= 0f || TurnManager.Instance.turnTimer > 30f)
            {
                timerText.text = "";
            }
            else
            {
                timerText.text = TurnManager.Instance.turnTimer.ToString("n0");
            }
        }
        else
        {
            timerText.text = "";
        }
    }

    internal void SetStats()
    {
        if (PhotonNetwork.LocalPlayer == null || PhotonNetwork.LocalPlayer.CustomProperties == null || PhotonNetwork.LocalPlayer.CustomProperties.Count < 1)
            return;

        string text = "";

        int dateCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["dateCount"];
        int relationshipCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["relationshipCount"];
        int marriageCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["marriageCount"];
        int yearsElapsed = (int)PhotonNetwork.LocalPlayer.CustomProperties["yearsElapsed"];

        text = $"# Dates: {dateCount}\n# Relationships: {relationshipCount}\n # Marriages: {marriageCount}\n # Years Elapsed: {yearsElapsed}";

        statsPanel.SetText(text);
    }

    public void ShowBottomForPlayer(int index)
    {

        Debug.Log("NetworkManager.Instance.players.Count = " + NetworkManager.Instance.players.Count);

        for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
        {
            if (i == index)
            {
                Debug.Log("Trying to show bottom for " + NetworkManager.Instance.players[i].NickName);
                photonView.RPC("RPC_ShowBottomForPlayer", NetworkManager.Instance.players[i]);
            }
            else
            {
                photonView.RPC("RPC_HideBottomForPlayer", NetworkManager.Instance.players[i]);
            }
        }


    }

    public void HideBottomForEveryone()
    {
        photonView.RPC("RPC_HideBottomForPlayer", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_ShowBottomForPlayer()
    {
        Debug.Log("SHOWING BOTTOM");
        bottomPanel.anchoredPosition = new Vector2(0, -bottomPanel.sizeDelta.y);
        bottomButton.interactable = false;
        bottomButton.onClick.RemoveAllListeners();

        ExitGames.Client.Photon.Hashtable playerProps = NetworkManager.Instance.players[TurnManager.turnIndex].CustomProperties;
        int diceCount = (int)playerProps["diceCount"];
        NetworkManager.Instance.players[TurnManager.turnIndex].SetCustomProperties(playerProps);


        bottomButton.onClick.AddListener(() =>
        {
            TurnManager.RollDice(diceCount, "board");

            bottomButton.interactable = false;
            bottomPanel.DOAnchorPosY(-bottomPanel.sizeDelta.y, 0.5f);

        });

        bottomButtonLabel.text = "Roll";

        bottomPanel.DOAnchorPosY(0f, 0.5f).OnComplete(() =>
        {
            bottomButton.interactable = true;
        });

    }

    [PunRPC]
    public void RPC_HideBottomForPlayer()
    {
        Debug.Log("HIDING BOTTOM");
        bottomPanel.DOAnchorPosY(-bottomPanel.sizeDelta.y, 0.5f);
    }


    private List<System.Action> promptButtonActions = new List<Action>();


    public void ShowCard(CardData card)
    {
        //Only the master can call this... All players should then get the RPC to show the card


        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("DO NOT RUN ShowCard() ON NON-MASTER CLIENTS");
            return;
        }

        if (card.isPrompt)
        {

            onCardClicked = () =>
            {
                Debug.Log("CARD CLICK RECIEVED ON MASTER, RUNNING ShowPrompt for current user");
                ShowPrompt(card.promptMessage, GetPromptButtons(card.promptButtons), NetworkManager.Instance.players[TurnManager.turnIndex], 0);
            };
        }
        else
        {
            onCardClicked = () =>
            {
                Debug.Log("CARD CLICK RECIEVED ON MASTER, RUNNING HandlePathNodeAction");
                TurnManager.HandlePathNodeAction(card.action, card.parentheses, card.rollCheck, card.rollPassed, card.rollFailed, true);
            };
        }

        waitingClickFromUserNickName = NetworkManager.Instance.players[TurnManager.turnIndex].NickName;
        foreach (Player player in NetworkManager.Instance.players)
        {
            Debug.LogWarning("-------");
            Debug.Log("Player Nickname: " + player.NickName);
            Debug.LogWarning("Player ID: " + player.UserId);
        }
        Debug.Log("Set waiting User ID to " + waitingClickFromUserNickName);


        bool isFancyCard = card.action == PathNodeAction.AdvanceToRelationshipWithProtectionFromSingle;

        photonView.RPC("RPC_ShowCard", RpcTarget.All, card.text + "(" + card.parentheses + ")", NetworkManager.Instance.players[TurnManager.turnIndex], (int)BoardManager.Instance.pieces[TurnManager.turnIndex].pathRing, isFancyCard);
    }

    [PunRPC]
    public void RPC_ShowCard(string cardText, Player targetPlayer, int pathIndex, bool isFancyCard)
    {
        Image cardImage = cardUIObj.GetComponent<Image>();
        cardImage.color = isFancyCard ? new Color(104 / 255f, 54 / 255f, 149 / 255f) : Color.white;

        cardUIObj.transform.Find("Mask/silhouette").gameObject.SetActive(isFancyCard);
        
        cardUIObj.anchoredPosition = new Vector2(cardUIObj.anchoredPosition.x, -1080f);

        cardUIText.text = cardText;

        cardUIText.color = isFancyCard ? Color.white : Color.black;

        cardUIButton.onClick.RemoveAllListeners();

        //Only make the card clickable for the user who is taking their turn right now.
        if (PhotonNetwork.LocalPlayer.NickName == targetPlayer.NickName)
        {
            cardUIButton.onClick.AddListener(() =>
            {
                OnCardClicked();
            });
        }


        CanvasGroup scenarioTextGroup = cardUIObj.Find("SCENARIO").GetComponent<CanvasGroup>();
        TextMeshProUGUI scenarioText = scenarioTextGroup.GetComponent<TextMeshProUGUI>();
        string scenarioTextStr = PhotonNetwork.LocalPlayer.NickName.ToLower() != targetPlayer.NickName.ToLower() ? $"<size=30>{targetPlayer.NickName.Replace("[host]", "").Trim()} Got A</size>\n" : "<size=30>You Got A</size>\n";

        scenarioTextStr += (pathIndex == 0 ? "DATING" : (pathIndex == 1 ? "RELATIONSHIP" : (pathIndex == 2 ? "MARRIAGE" : ""))) + " SCENARIO";

        scenarioText.text = scenarioTextStr;
        scenarioTextGroup.alpha = 0f;
        scenarioTextGroup.DOFade(1f, 0.25f).OnComplete(() =>
        {
            //Bring the card up onto the screen
            cardUIObj.DOAnchorPosY(0f, 0.25f).SetDelay(1f).OnComplete(() =>
            {
                CanvasGroup cg = cardUIObj.GetComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
                Debug.Log("Card fully visible.");
                scenarioTextGroup.alpha = 0f;
            });
        });


    }

    public void OnCardClicked()
    {
        Debug.Log("CLICKED CARD: Calling RPC NOW");
        photonView.RPC("RPC_OnCardClicked", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);

        //Go ahead and hide the card immediately for the player who clicked on it, incase there is any delay in the response returning to them
        HideCard();
    }

    public System.Action onCardClicked = null;

    string waitingClickFromUserNickName = null;

    [PunRPC]
    public void RPC_OnCardClicked(string userNickname)
    {
        Debug.Log("RPC_OnCardClicked()");

        Debug.Log(waitingClickFromUserNickName);

        if (PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(waitingClickFromUserNickName) && userNickname == waitingClickFromUserNickName)
        {
            //The correct user has clicked and we are running on the master client, execute the proper response from the cache
            onCardClicked?.Invoke();

            if (userNickname == waitingClickFromUserNickName)
                photonView.RPC("RPC_HideCard", RpcTarget.All);
        }

        //If the user who clicked is the one we were waiting on, go ahead and hide the card for everyone.

    }

    [PunRPC]
    public void RPC_HideCard()
    {
        HideCard();
    }

    public void HideCard()
    {

        CanvasGroup cg = cardUIObj.GetComponent<CanvasGroup>();

        if (cg.interactable == false) return; //This means we already started hiding it and dont need to do it again.

        cg.interactable = false;
        cg.blocksRaycasts = false;

        cardUIObj.DOAnchorPosY(-1080f, 0.25f).OnComplete(() =>
        {
            Debug.Log("Card fully hidden.");
        });
    }
    public PopupDialog.PopupButton[] GetPromptButtons(CardPromptButton[] cardButtons)
    {
        List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();
        for (int i = 0; i < cardButtons.Length; i++)
        {
            ProceedAction action = cardButtons[i].action;
            PopupDialog.PopupButton btn = new PopupDialog.PopupButton()
            {
                text = cardButtons[i].text,
                onClicked = () =>
                {
                    //GameManager.Instance.pieces[turnIndex].GoHome();
                    TurnManager.ExecuteProceedAction(action, () => { });
                },
                buttonColor = PopupDialog.PopupButtonColor.Plain
            };

            buttons.Add(btn);
        }

        return buttons.ToArray();
    }



    public void ShowPrompt(string text, PopupDialog.PopupButton[] buttons = null, Player player = null, int bgColor = 0, bool autoEndTurn = true)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("ONLY THE MASTER CLIENT SHOULD BE INITIALIZING SHOW PROMPT");
            return;
        }

        if (player == null)
        {
            Debug.LogError("NEVER CALL THIS WITH PLAYER == NULL");
            return;
        }


        PopupDialog.PopupButton[] btns = buttons;
        List<string> buttonTexts = new List<string>();

        bool doEndTurn = autoEndTurn;

        if (btns != null)
        {
            for (int i = 0; i < btns.Length; i++)
            {
                int index = i;

                //Store the text in the list so the end client can see the right language on the buttons
                buttonTexts.Add(btns[index].text);
                //Forcefully add the EndTurn call to every button we store on the master.
                System.Action originalAction = btns[index].onClicked;
                btns[index].onClicked += () =>
                {
                    if (doEndTurn)
                    {
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            Debug.Log("Ending turn after dialog prompt button click.");
                            TurnManager.EndTurn();
                        });
                    }

                    SoundManager.Instance.PlaySound("confirm");
                };
            }
        }
        else
        {
            btns = new PopupDialog.PopupButton[]
            {
                new PopupDialog.PopupButton()
                {
                    text = "Okay",
                    onClicked = () =>
                    {
                        SoundManager.Instance.PlaySound("confirm");

                        if(doEndTurn)
                        {
                            DOVirtual.DelayedCall(1f, () =>
                            {
                                Debug.Log("Ending turn after dialog prompt button click.");
                                TurnManager.EndTurn();
                            });
                        }
                    }
                }
            };
        }


        //Store the buttons here on the master so when the client makes the RPC_PromptResponse call we can reference the proper onClick events.
        currentButtons = btns;

        photonView.RPC("RPC_ShowPrompt", RpcTarget.All, text, buttonTexts.ToArray(), player, bgColor);
    }

    PopupDialog.PopupButton[] currentButtons;

    [PunRPC]
    public void RPC_ShowPrompt(string text, string[] buttonTexts, Player player, int bgColor = 0)
    {
        List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();

        if (buttonTexts == null || buttonTexts.Length == 0)
        {
            buttonTexts = new string[] { "Okay" };
        }

        int i = 0;
        foreach (string buttonText in buttonTexts)
        {
            int index = i;
            buttons.Add(new PopupDialog.PopupButton()
            {
                text = buttonText,
                onClicked = () => { photonView.RPC("RPC_PromptResponse", RpcTarget.All, index, player); }
            });

            i++;
        }

        PopupDialog.Instance.Show("", text, buttons.ToArray(), bgColor, player == PhotonNetwork.LocalPlayer);

        Debug.Log("ShowPrompt: " + text);
    }

    [PunRPC]
    public void RPC_PromptResponse(int buttonIndex, Player player)
    {
        if (PhotonNetwork.IsMasterClient)
        {

            if (currentButtons == null || currentButtons.Length == 0 || buttonIndex >= currentButtons.Length)
            {
                return;
            }

            currentButtons[buttonIndex].onClicked.Invoke();
        }

        if (PhotonNetwork.LocalPlayer != player)
        {
            PopupDialog.Instance.Close();
        }


    }




    internal void AvoidSingleCardAnimation(Player player)
    {
        if (player == null) return;

        photonView.RPC("RPC_AvoidSingleCardAnimation", RpcTarget.All, player);
    }

    [SerializeField]
    RectTransform avoidSingleCardSprite;

    [PunRPC]
    public void RPC_AvoidSingleCardAnimation(Player player)
    {
        Transform targetPlayerBar = null;
        foreach (Transform child in playerProgressContainer)
        {
            TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
            if (text.text.ToLower() == player.NickName.ToLower())
            {
                targetPlayerBar = child;
                Debug.Log("FOUND TARGET PLAYER BAR");
                break;
            }
        }

        if (targetPlayerBar == null)
            return;

        avoidSingleCardSprite.anchoredPosition = new Vector2(0f, -1080f);
        avoidSingleCardSprite.localScale = new Vector3(0.1f, .1f, .1f);
        DOTween.Kill(avoidSingleCardSprite);
        avoidSingleCardSprite.DORotate(new Vector3(0f, 0f, 360f * 3f), 0.22f);
        avoidSingleCardSprite.DOScale(Vector3.one, 0.25f).OnComplete(() =>
        {
            avoidSingleCardSprite.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.25f);
        });
        avoidSingleCardSprite.DOAnchorPos(Vector2.zero, 0.25f).OnComplete(() =>
        {
            Vector2 targetPos = targetPlayerBar != null ? targetPlayerBar.GetComponent<RectTransform>().anchoredPosition : Vector2.zero;
            avoidSingleCardSprite.DOAnchorPos(targetPos, 0.5f).SetDelay(1f);
            avoidSingleCardSprite.DOScale(Vector3.zero, 0.5f).SetDelay(1f).OnComplete(() =>
            {
                Debug.Log("Avoid signle card animation finished");
            });
        });
    }





    [SerializeField]
    Window_GameOver gameOverWindow;

    public void ShowGameOver(string winnerName)
    {
        gameOverWindow.Init(winnerName);
        gameOverWindow.Show();

    }

}
