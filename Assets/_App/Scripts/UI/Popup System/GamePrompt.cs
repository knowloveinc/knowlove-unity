using DG.Tweening;
using Knowlove.ActionAndPathLogic;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove.UI
{
    public class GamePrompt : MonoBehaviourPunCallbacks
    {
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;
        [SerializeField] private BackSingleLogic _backSingleLogic;
        [SerializeField] private BackSingleIgnorList _backSingleIgnorList;

        private List<Action> _promptButtonActions = new List<Action>();
        private PopupDialog.PopupButton[] _currentButtons;

        private void Start()
        {
            _backSingleIgnorList.ChoicedOfPlayer += ShowPrompt;
            _backSingleLogic.ChoicedOfPlayer += ShowPrompt;
            _pathNodeActionLogic.ChoicedOfPlayer += ShowPrompt;
            _proceedActionLogic.ChoicedOfPlayer += ShowPrompt;
        }

        private void OnDestroy()
        {
            _backSingleIgnorList.ChoicedOfPlayer -= ShowPrompt;
            _backSingleLogic.ChoicedOfPlayer -= ShowPrompt;
            _pathNodeActionLogic.ChoicedOfPlayer -= ShowPrompt;
            _proceedActionLogic.ChoicedOfPlayer -= ShowPrompt;
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
                                TurnManager.Instance.EndTurn();
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
                                    TurnManager.Instance.EndTurn();
                                });
                            }
                        }
                    }
                };
            }

            //Store the buttons here on the master so when the client makes the RPC_PromptResponse call we can reference the proper onClick events.
            _currentButtons = btns;

            photonView.RPC(nameof(RPC_ShowPrompt), RpcTarget.All, text, buttonTexts.ToArray(), player, bgColor);
        }

        [PunRPC]
        private void RPC_ShowPrompt(string text, string[] buttonTexts, Player player, int bgColor = 0)
        {
            List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();

            if (buttonTexts == null || buttonTexts.Length == 0)
                buttonTexts = new string[] { "Okay" };

            int i = 0;
            foreach (string buttonText in buttonTexts)
            {
                int index = i;
                buttons.Add(new PopupDialog.PopupButton()
                {
                    text = buttonText,
                    onClicked = () => { photonView.RPC(nameof(RPC_PromptResponse), RpcTarget.All, index, player); }
                });

                i++;
            }

            PopupDialog.Instance.Show("", text, buttons.ToArray(), bgColor, player == PhotonNetwork.LocalPlayer);

            Debug.Log("ShowPrompt: " + text);
        }

        [PunRPC]
        private void RPC_PromptResponse(int buttonIndex, Player player)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (_currentButtons == null || _currentButtons.Length == 0 || buttonIndex >= _currentButtons.Length)
                    return;

                _currentButtons[buttonIndex].onClicked.Invoke();
            }

            if (PhotonNetwork.LocalPlayer != player)
                PopupDialog.Instance.Close();
        }
    }
}
