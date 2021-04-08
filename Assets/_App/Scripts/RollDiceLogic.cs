using DG.Tweening;
using Knowlove.ActionAndPathLogic;
using Knowlove.UI;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using static Knowlove.TurnManager;

namespace Knowlove
{
    public class RollDiceLogic : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;
        [SerializeField] private MoveBoardPiece _moveBoardPiece;

        public void RollDice(int amount, string location)
        {
            photonView.RPC(nameof(RPC_RollDice), RpcTarget.All, amount, location);
        }

        [PunRPC]
        private void RPC_RollDice(int amount, string location)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                CameraManager.Instance.SetCamera(1);
                BoardManager.Instance.RollDice(amount, location);
                StartCoroutine(WaitForDiceToRoll());
            }
        }

        private IEnumerator WaitForDiceToRoll()
        {
            while (!BoardManager.Instance.diceFinishedRolling)
            {
                yield return null;
            }

            OnDiceFinishedRolling(BoardManager.Instance.diceScore, BoardManager.Instance.diceRollLocation);
        }
        private void OnDiceFinishedRolling(int diceScore, string location)
        {
            Debug.Log("OnDiceFinishedRolling()");
            photonView.RPC(nameof(RPC_OnDiceFinished), RpcTarget.All, diceScore, location);
        }

        [PunRPC]
        private void RPC_OnDiceFinished(int diceScore, string location)
        {
            Debug.Log("RPC_OnDiceFinished()");
            if (PhotonNetwork.IsMasterClient)
            {
                if (location == "board")
                {
                    Debug.Log("Handling board roll...");
                    _turnManager.turnState = TurnState.RollFinished;

                    string playerName = NetworkManager.Instance.players[_turnManager.turnIndex].NickName;

                    _gameUI.SetTopText($"{playerName} rolled a {diceScore.ToString("n0")}", "RESULT");
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        CameraManager.Instance.SetCamera(0);
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            _moveBoardPiece.AdvanceGamePieceFoward(diceScore);
                        });
                    });
                }
                else if (location == "scenario")
                {
                    Debug.Log("Handling Scenario roll...");
                    CameraManager.Instance.SetCamera(0);
                    HandleScenarioRollResult(diceScore, _turnManager.currentRollCheck, _turnManager.currentOnPassed, _turnManager.currentOnFailed);
                }
            }
        }

        private void HandleScenarioRollResult(int diceScore, int rollCheck, ProceedAction onPassed, ProceedAction onFailed)
        {
            if (diceScore >= rollCheck)
            {
                _proceedActionLogic.ExecuteProceedAction(onPassed, () =>
                {
                    Debug.Log("PROCEED ACTION FINISHED IN PASSED STATE");
                });
            }
            else
            {
                _proceedActionLogic.ExecuteProceedAction(onFailed, () =>
                {
                    Debug.Log("PROCEED ACTION FINISHED IN FAILED STATE");
                });
            }
        }
    }
}
