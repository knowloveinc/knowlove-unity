using DG.Tweening;
using Knowlove.MyStuffInGame;
using Knowlove.UI;
using Knowlove.XPSystem;
using Photon.Realtime;
using UnityEngine;

namespace Knowlove.ActionAndPathLogic
{
    public class BackSingleIgnorList : MonoBehaviour
    {
        public delegate void ShowedPrompts(string text, PopupDialog.PopupButton[] buttons = null, Player player = null, int bgColor = 0, bool autoEndTurn = true);

        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;
        [SerializeField] private GameStuff _gameStuff;

        private int _diceCount;
        private int _avoidSingleCards;
        private int _wallet;
        private bool _protectedFromSingleInRelationship;
        private bool _protectedFromSingleInMarriage;
        private bool _protectedFromSingleAllGame;

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

        public void BackToSingleOrDisregardBecauseList(string text, bool skipPrompt, BoardPiece piece, Player currentPlayer)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;

            _diceCount = (int)playerProperties["diceCount"];
            _avoidSingleCards = (int)playerProperties["avoidSingleCards"];
            _wallet = (int)playerProperties["wallet"];
            _protectedFromSingleInRelationship = (bool)playerProperties["protectedFromSingleInRelationship"];

            _protectedFromSingleInMarriage = InfoPlayer.Instance.PlayerState.ProtectedFromBackToSingleInMarriagePerGame;
            _protectedFromSingleAllGame = InfoPlayer.Instance.PlayerState.ProtectedFromBackToSinglePerGame;

            if (_protectedFromSingleAllGame)
                ProtectBackSingle(skipPrompt, text, currentPlayer, " [ You have protected all game ]");
            else if (_protectedFromSingleInMarriage && IsMarriage)
                ProtectBackSingle(skipPrompt, text, currentPlayer, " [ You have protected in marriage game ]");
            else if (_protectedFromSingleInRelationship && IsRelationship)
                ProtectBackSingle(skipPrompt, text, currentPlayer, "Your mate matches your non-negotiable list.");
            else if (!skipPrompt)
            {
                if (_avoidSingleCards < 1 && _wallet > 0 && IsNotDaring)
                    StorOpen(piece, currentPlayer);   
                else if (_avoidSingleCards < 1 && IsNotDaring)
                    DontHaveCardBackSingle(piece, currentPlayer);
                else if (_avoidSingleCards > 0)
                    HasCardBackSingle(piece, currentPlayer);
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

            playerProperties["diceCount"] = _diceCount;
            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void ProtectBackSingle(bool skipPrompt, string text, Player currentPlayer, string newText)
        {
            if (!skipPrompt)
            {
                text = newText;
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

        private void StorOpen(BoardPiece piece, Player currentPlayer)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;
            string dialogText = "You settled for someone who didnt meet your non-negotiable list requirements. (Back to single.)";

            PopupDialog.PopupButton btn = new PopupDialog.PopupButton()
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
                        _pathNodeActionLogic.RPC_ShowStore(TurnIndex, isAction, actionJson);
                    });
                }
            };

            _diceCount = 1;
            ChoicedOfPlayer?.Invoke(dialogText, new PopupDialog.PopupButton[] { btn, openStoreBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, false);

            playerProperties["diceCount"] = _diceCount;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void DontHaveCardBackSingle(BoardPiece piece, Player currentPlayer)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;
            string dialogText = "You settled for someone who didnt meet your non-negotiable list requirements. (Back to single.)";

            PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
            {
                text = "Okay",
                onClicked = () =>
                {
                    _protectedFromSingleInRelationship = false;
                    piece.GoHome(currentPlayer);

                    playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
                    currentPlayer.SetCustomProperties(playerProperties);
                }
            };

            _diceCount = 1;
            ChoicedOfPlayer?.Invoke(dialogText, new PopupDialog.PopupButton[] { yesBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing);

            playerProperties["diceCount"] = _diceCount;
            playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
            currentPlayer.SetCustomProperties(playerProperties);
        }

        private void HasCardBackSingle(BoardPiece piece, Player currentPlayer)
        {
            ExitGames.Client.Photon.Hashtable playerProperties = currentPlayer.CustomProperties;
            string dialogText = "You settled for someone who didnt meet your non-negotiable list requirements. (Back to single.)" + "\n\n[Activated Avoid Going to Single Card]";

            PopupDialog.PopupButton yesBtn = new PopupDialog.PopupButton()
            {
                text = "Okay",
                onClicked = () =>
                {
                    _pathNodeActionLogic.UsedAvoidSingleCard?.Invoke(currentPlayer);
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

                    playerProperties["protectedFromSingleInRelationship"] = _protectedFromSingleInRelationship;
                    currentPlayer.SetCustomProperties(playerProperties);
                }
            };

            ChoicedOfPlayer?.Invoke(dialogText, new PopupDialog.PopupButton[] { yesBtn, noBtn }, currentPlayer, 1 + (int)BoardManager.Instance.pieces[TurnIndex].pathRing, true);           
        }
    }
}