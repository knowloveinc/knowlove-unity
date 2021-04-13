using UnityEngine;
using Photon.Pun;
using DG.Tweening;

namespace Knowlove
{
    public class Card : MonoBehaviourPun
    {
        private Vector3 _startPosition;

        private void Awake()
        {
            _startPosition = transform.position;
        }

        [ContextMenu("RPC DrawCard")]
        public void DrawToCamera()
        {
            photonView.RPC(nameof(RPC_DrawToCamera), RpcTarget.All);
        }

        [PunRPC]
        public void RPC_DrawToCamera()
        {
            transform.position = _startPosition;
            Vector3 targetPosition = new Vector3(Camera.main.transform.position.x, _startPosition.y, Camera.main.transform.position.z);
            transform.DOMove(targetPosition, 1f).OnComplete(() => 
            {
                transform.position = _startPosition;
            });  
        }  
    }
}