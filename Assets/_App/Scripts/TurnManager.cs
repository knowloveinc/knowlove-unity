using DG.Tweening;
using Knowlove.ActionAndPathLogic;
using Knowlove.MyStuffInGame;
using Knowlove.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Knowlove
{
    public class TurnManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public static TurnManager Instance;

        public GameUI gameUI;
        public TurnState turnState = TurnState.TurnEnding;
        public ProceedAction currentOnPassed, currentOnFailed;

        public int turnIndex = 0;
        public int currentRollCheck;
        public float turnTimer = 30f;

        [SerializeField] private GameStuff _gameStuff;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;

        private bool _didRoll = false;

        public ProceedActionLogic ProceedActionLogic
        {
            get => _proceedActionLogic;
        }

        public PathNodeActionLogic PathNodeActionLogic
        {
            get => _pathNodeActionLogic;
        }

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

        private void Start()
        {
            Instance = this;
            turnIndex = 0;

            Debug.Log("TurnManager.Start()");
            gameUI.RPC_HideBottomForPlayer();

            if (PhotonNetwork.IsMasterClient)
            {
                NetworkManager.OnReadyToStart += ShowUserPickCard;
            }
        }

        private void Update()
        {
            CheckTurnState();
            UpdateTurnTimer();
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
                _gameStuff.GetSpecialCard();
            }
            else
                Debug.LogError("Current Room == null ????");
        }

        private void StartTurn()
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
            _didRoll = false;
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
                    Debug.Log("TurnBank == 0");

                StartTurn();
            }
        }

        public void EndTurn(bool isSkip = false)
        {
            Debug.Log("<color=Red>#####################  END OF TURN ####################</color>");
            turnState = TurnState.TurnEnding;

            if (isSkip)
                gameUI.SetTopText("Skipping " + NetworkManager.Instance.players[turnIndex].NickName);
            else
                gameUI.SetTopText("Turn ending..");

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

        public void GameOver(string playerName)
        {
            gameUI.HideBottomForEveryone();
            photonView.RPC(nameof(RPC_GameOver), RpcTarget.All, playerName);
        }

        [PunRPC]
        public void RPC_GameOver(string playerName)
        {
            gameUI.ShowGameOver(playerName);
        }

        [ContextMenu("Make Current Player Win")]
        public void MakeCurrentPlayerWin()
        {
            if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

            _proceedActionLogic.ExecuteProceedAction(ProceedAction.GoToKNOWLOVE, () => { });
        }

        [ContextMenu("SendToRelationship")]
        public void SendToRelationship()
        {
            if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

            _proceedActionLogic.ExecuteProceedAction(ProceedAction.GoToRelationship, () => { });
        }

        [ContextMenu("SendToMarriage")]
        public void SendToMarriage()
        {
            if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

            _proceedActionLogic.ExecuteProceedAction(ProceedAction.GoToMarriage, () => { });
        }

        [ContextMenu("Back to Single")]
        public void SendToSingle()
        {
            if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

            _proceedActionLogic.ExecuteProceedAction(ProceedAction.BackToSingle, () => { });
        }

        [ContextMenu("AdvanceToNextPath")]
        public void SendToAdvance()
        {
            if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

            _pathNodeActionLogic.HandlePathNodeAction(PathNodeAction.AdvanceToNextPath, "Advanced by Debug Code");
        }

        [ContextMenu("AdvanceToNextPath And Get Card")]
        public void SendToAdvanceWithCard()
        {
            if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

            _pathNodeActionLogic.HandlePathNodeAction(PathNodeAction.AdvanceAndGetAvoidCard, "Advanced by Debug Code and you got an avoid card");
        }

        [ContextMenu("Give Two Dice")]
        public void GiveTwoDice()
        {
            if (!Application.isEditor || !PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable props = NetworkManager.Instance.players[turnIndex].CustomProperties;
            props["diceCount"] = 2;
            NetworkManager.Instance.players[turnIndex].SetCustomProperties(props);
        }

        public void CallAction(ProceedAction action, bool isProceedAction)
        {
            photonView.RPC(nameof(RPC_CallAction), RpcTarget.MasterClient, action, isProceedAction);
        }

        [PunRPC]
        private void RPC_CallAction(ProceedAction action, bool isProceedAction)
        {
            PathNode node = BoardManager.Instance.paths[(int)BoardManager.Instance.pieces[turnIndex].pathRing].nodes[BoardManager.Instance.pieces[turnIndex].pathIndex];

            if (isProceedAction)
                _proceedActionLogic.ExecuteProceedAction(action, () => { });
            else
                _pathNodeActionLogic.HandlePathNodeAction(node.action, node.nodeText, node.rollCheck, node.rollPassed, node.rollFailed);
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

        private void UpdateTurnTimer()
        {
            if (PhotonNetwork.IsMasterClient && turnState != TurnState.GameOver && turnState != TurnState.TurnEnding)
            {

                if (NetworkManager.Instance.players[turnIndex].IsInactive)
                    turnTimer -= Time.deltaTime;
                else
                    turnTimer = 31f;

                if (turnTimer <= 0)
                    EndTurn();
            }
        }

        private void CheckTurnState()
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

        private void ShowUserPickCard()
        {
            DOVirtual.DelayedCall(2f, () =>
            {
                CameraManager.Instance.SetCamera(2);

                gameUI.ShowPickCard();
            });
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
}

