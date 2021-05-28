using DG.Tweening;
using Knowlove.MyStuffInGame;
using Knowlove.UI;
using Knowlove.XPSystem;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove.ActionAndPathLogic
{
    public class BackSingleLogic : MonoBehaviour
    {
        public delegate void ShowedPrompts(string text, PopupDialog.PopupButton[] buttons = null, Player player = null, int bgColor = 0, bool autoEndTurn = true);

        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;
        [SerializeField] private GameStuff _gameStuff;

        private int _turnBank;
        private int _diceCount;
        private int _avoidSingleCards;
        private int _dateCount;
        private int _wallet;
        private string _buttonText;
        private bool _protectedFromSingleInRelationship;

        public event ShowedPrompts ChoicedOfPlayer;

        private int TurnIndex
        {
            get => _turnManager.turnIndex;
        }

        private bool IsNotDaring
        {
            get => !(BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Dating || BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing == PathRing.Home);
        }
        

        public void BackToSingle(string text, bool skipPrompt, BoardPiece piece, Player currentPlayer)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            _buttonText = text;
            _turnBank = (int)playerProperties["turnBank"];
            _diceCount = (int)playerProperties["diceCount"];
            _avoidSingleCards = (int)playerProperties["avoidSingleCards"];
            _dateCount = (int)playerProperties["dateCount"];
            _wallet = (int)playerProperties["wallet"];
            _protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];

            List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();
            
            if (_avoidSingleCards > 0)
                buttons = HasAvoidSingleCard(skipPrompt, currentPlayer, piece);
            else if(_avoidSingleCards <= 0)
                buttons = DontAvoidSingleCard(skipPrompt, currentPlayer, piece);
            else
                DOVirtual.DelayedCall(1f, () => _turnManager.EndTurn());

            if(buttons.Count > 0)
                ChoicedOfPlayer?.Invoke(_buttonText, buttons.ToArray(), currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, false);

            playerProperties["turnBank"] = _turnBank;
            playerProperties["diceCount"] = _diceCount;
            playerProperties["dateCount"] = _dateCount;
            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private List<PopupDialog.PopupButton> HasAvoidSingleCard(bool skipPrompt, Player currentPlayer, BoardPiece piece)
        {
            List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            if (!skipPrompt)
            {
                _buttonText += " [ Avoid Going To Single Card Activated ]";
                PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
                {
                    text = "Okay",
                    onClicked = () =>
                    {
                        _pathNodeActionLogic.UsedAvoidSingleCard?.Invoke(currentPlayer);
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

                        playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
                        currentPlayer.SetCustomProperties(playerProperties);
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

            return buttons;
        }

        private List<PopupDialog.PopupButton> DontAvoidSingleCard(bool skipPrompt, Player currentPlayer, BoardPiece piece)
        {
            List<PopupDialog.PopupButton> buttons = new List<PopupDialog.PopupButton>();
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

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

                if (_wallet >= 0 && IsNotDaring)
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
                                _pathNodeActionLogic.RPC_ShowStore(TurnIndex, isAction, actionJson);
                            });
                        }
                    };

                    buttons.Add(openStoreBtn);
                }
            }
            else
            {
                DOVirtual.DelayedCall(0.25f, () =>
                {
                    _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
                });
            }

            return buttons;
        }
    }
}
