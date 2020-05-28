using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameBrewStudios
{

    public class BoardPiece : MonoBehaviour
    {
        public Transform startingPoint;

        public Transform knowLovePoint;

        [SerializeField]
        AudioClip tapSound;

        [SerializeField]
        AudioSource source;

        public bool isMoving = false;

        public GameManager.PathRing pathRing;
        public int pathIndex = -1;

        
        [ContextMenu("Go Home")]
        public void GoHome(System.Action OnFinished = null)
        {
            JumpTo(startingPoint.position, () => { OnFinished?.Invoke(); pathIndex = -1; pathRing = GameManager.PathRing.Home; });
        }

        [ContextMenu("Go to KNOWLOVE")]
        public void GoToKnowLove(System.Action OnFinished = null)
        {
            JumpTo(knowLovePoint.position, OnFinished);
        }

        public void GoToRelationship(System.Action OnFinished = null)
        {
            JumpTo(GameManager.Instance.paths[1].NodesAsVector3()[0], () => { OnFinished?.Invoke(); pathIndex = 0; pathRing = GameManager.PathRing.Relationship; });
        }

        public void GoToMarriage(System.Action OnFinished = null)
        {
            JumpTo(GameManager.Instance.paths[2].NodesAsVector3()[0], () => { OnFinished?.Invoke(); pathIndex = 0; pathRing = GameManager.PathRing.Marriage; });
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


        int jumpPathIndex = 0;
        IEnumerator DoJumpPath(List<Vector3> positions, System.Action OnFinished)
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
                    if (tapSound != null)
                    {
                        source.PlayOneShot(tapSound);
                    }
                });

                while(jumpPathIndex == index)
                {
                    yield return null;
                }
            }

            isMoving = false;
            OnFinished?.Invoke();
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
    }
}