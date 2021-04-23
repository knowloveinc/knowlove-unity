using Knowlove.ActionAndPathLogic;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using static Knowlove.TurnManager;

namespace Knowlove
{
    public class MoveBoardPiece : MonoBehaviourPunCallbacks
    {
        public int countMovePiece = 1;

        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;

        public Action<string, string> ShowedStatePiece;

        [ContextMenu("Move On count path")]
        public void TestMove()
        {
            AdvanceGamePieceFoward(countMovePiece);
        }

        public void AdvanceGamePieceFoward(int spaces)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _turnManager.turnState = TurnState.MovingBoardPiece;
                ShowedStatePiece?.Invoke("Moving...", "PLEASE WAIT");
                StartCoroutine(WaitForGamepieceToMove());
                BoardManager.Instance.photonView.RPC("RPC_MoveBoardPiece", RpcTarget.All, _turnManager.turnIndex, spaces);
            }
        }

        private IEnumerator WaitForGamepieceToMove()
        {
            BoardManager.Instance.movingBoardPiece = true;

            while (BoardManager.Instance.movingBoardPiece == true)
            { yield return null; }

            GameManager_OnGamePieceFinishedMoving();
        }

        private void GameManager_OnGamePieceFinishedMoving()
        {
            _turnManager.turnState = TurnState.PieceMoved;
            ShowedStatePiece?.Invoke("", "");
            ExecutePathNode((int)BoardManager.Instance.pieces[_turnManager.turnIndex].pathRing, BoardManager.Instance.pieces[_turnManager.turnIndex].pathIndex);
        }

        public void ExecutePathNode(int path, int pathIndex)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Tried running ExecutePathNode on non-master client");
                return;
            }

            PathNode node = BoardManager.Instance.paths[path].nodes[pathIndex];

            _pathNodeActionLogic.HandlePathNodeAction(node.action, node.nodeText, node.rollCheck, node.rollPassed, node.rollFailed);

            Debug.Log("Finished executing path node: " + node.action.ToString());
        }
    }
}
