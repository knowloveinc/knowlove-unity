using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

namespace Knowlove
{
    public class BoardPiece : MonoBehaviourPun
    {
        public Transform startingPoint;

        public Transform knowLovePoint;

        [SerializeField] private AudioClip tapSound;

        [SerializeField] private AudioSource source;        

        public bool isMoving = false;

        public PathRing pathRing;
        public int pathIndex = -1;

        private int jumpPathIndex = 0;

        [ContextMenu("Go Home")]
        public void GoHome( Player player = null, System.Action OnFinished = null)
        {
            JumpTo(startingPoint.position, () => 
            { 
                pathIndex = -1; 
                pathRing = PathRing.Home;

                if (player != null)
                {
                    Debug.Log("<color=Cyan>Resetting progress for player: </color>" + player.NickName);
                    ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
                    float progress = (float)playerProperties["progress"];

                    playerProperties["diceCount"] = 1;
                    progress = 0.1f;
                    playerProperties["progress"] = progress;

                    player.SetCustomProperties(playerProperties);
                }

                OnFinished?.Invoke(); 
            });
        }

        public void GoToKnowLove(Player player = null, System.Action OnFinished = null)
        {
            Debug.Log("Player = " + player);
            if (player != null)
            {
                Debug.LogError("DID I MAKE IT HERE???");
                ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
                playerProperties["progress"] = 1f;
                player.SetCustomProperties(playerProperties);

                TurnManager.Instance.GameOver(player.NickName);
            }

            JumpTo(knowLovePoint.position, OnFinished);
        }

        public void GoToRelationship(Player player, System.Action OnFinished = null)
        {
            if (player != null)
            {
                Debug.Log("UPDATING PLAYER PROGRESS TO RELATIONSHIP");
                ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
                playerProperties["progress"] = 0.666f;
                playerProperties["relationshipCount"] = (int)playerProperties["relationshipCount"] + 1;
                player.SetCustomProperties(playerProperties); ;
            }

            JumpTo(BoardManager.Instance.paths[1].NodesAsVector3()[0], () => { OnFinished?.Invoke(); pathIndex = 0; pathRing = PathRing.Relationship; });
        }

        public void GoToMarriage(Player player, System.Action OnFinished = null)
        {
            if (player != null)
            {
                Debug.Log("UPDATING PLAYER PROGRESS TO RELATIONSHIP");
                ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
                playerProperties["progress"] = 0.90f;
                playerProperties["marriageCount"] = (int)playerProperties["marriageCount"] + 1;
                player.SetCustomProperties(playerProperties);
            }

            JumpTo(BoardManager.Instance.paths[2].NodesAsVector3()[0], () => { OnFinished?.Invoke(); pathIndex = 0; pathRing = PathRing.Marriage; });
        }

        public void JumpPath(List<Vector3> positions, System.Action OnFinished)
        {
            if (isMoving)
            {
                Debug.Log("That piece is already being moved right now.");
                return;
            }
            
            StartCoroutine(DoJumpPath(positions, OnFinished));
        }

        public void JumpTo(Vector3 position, System.Action OnFinished)
        {
            isMoving = true;
            transform.DOJump(position, 0.5f, 1, 0.5f).OnComplete(() =>
            {
                if (tapSound != null)
                {
                    source.PlayOneShot(tapSound);
                }
                isMoving = false;
                OnFinished?.Invoke();
            });
        }

        private IEnumerator DoJumpPath(List<Vector3> positions, System.Action OnFinished)
        {
            isMoving = true;
            jumpPathIndex = 0;

            foreach (Vector3 pos in positions)
            {
                int index = jumpPathIndex;
                transform.DOJump(pos, 0.05f, 1, 0.35f).OnComplete(() =>
                {
                    jumpPathIndex++;
                    pathIndex++;
                    if (pathIndex >= BoardManager.Instance.paths[(int)pathRing].nodes.Count)
                    {
                        pathIndex = 0;
                        TurnManager.Instance.PlayerDidElapseOneYear();
                    }

                    if (tapSound != null)
                        SoundManager.Instance.PlaySound(tapSound.name);
                });

                while(jumpPathIndex == index)
                    yield return null;
            }

            isMoving = false;
            OnFinished?.Invoke();
        }
    }
}