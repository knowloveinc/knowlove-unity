using UnityEngine;
using Photon.Pun;
using DG.Tweening;

namespace Knowlove
{
    public class Card : MonoBehaviourPun
    {
        Vector3 startPosition;

        private void Awake()
        {
            startPosition = transform.position;
        }

        [ContextMenu("RPC DrawCard")]
        public void DrawToCamera()
        {
            photonView.RPC(nameof(RPC_DrawToCamera), RpcTarget.All);
        }

        [PunRPC]
        public void RPC_DrawToCamera()
        {
            transform.position = startPosition;
            Vector3 targetPosition = new Vector3(Camera.main.transform.position.x, startPosition.y, Camera.main.transform.position.z);
            transform.DOMove(targetPosition, 1f).OnComplete(() => 
            {
                transform.position = startPosition;
            });

            
        }
        
    }
}