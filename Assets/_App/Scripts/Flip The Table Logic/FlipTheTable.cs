using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using System;

namespace Knowlove.FlipTheTableLogic
{
    public class FlipTheTable : MonoBehaviourPunCallbacks
    {
        private Rigidbody _rigidbody;
        private Vector3 _startPosition;

        [SerializeField] private FlipObject[] _flipObjects;

        [SerializeField] private float x = 1200f;
        [SerializeField] private float y = 1200f;
        [SerializeField] private float z = 500f;
        [SerializeField] private float speed = 1f;
        [SerializeField] private int cameraNumber = 4;

        private bool[] _isMoveFinish;
        private bool _isMove;

        public Action<bool> StartedFlipTable;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _startPosition = transform.position;
            _isMove = false;
            _isMoveFinish = new bool[_flipObjects.Length];

            for(int i = 0; i < _isMoveFinish.Length; i++)
            {
                _isMoveFinish[i] = false;
            }
        }

        private void FixedUpdate()
        {
            if (_isMove)
                CollectTable();
        }

        [ContextMenu("Flip")]
        public void FlipTable()
        {
            if(!_isMove)
                photonView.RPC(nameof(RPC_FlipTable), RpcTarget.AllBufferedViaServer);
        }

        [ContextMenu("CollectFlip")]
        public void CollectTable()
        {
            photonView.RPC(nameof(RPC_CollectTable), RpcTarget.AllBufferedViaServer);
        }

        [PunRPC]
        private void RPC_FlipTable()
        {
            StartedFlipTable?.Invoke(false);
            CameraManager.Instance.SetCamera(cameraNumber);

            for (int i = 0; i < _flipObjects.Length - 2; i++)
            {
                _flipObjects[i].SetActiveKinematic(false);
                _flipObjects[i].SetPiecePosition();
            }

            DOVirtual.DelayedCall(0.5f, () => 
            {
                for (int i = _flipObjects.Length - 1; i > _flipObjects.Length - 3; i--)
                {
                    _flipObjects[i].SetActiveKinematic(false);
                    _flipObjects[i].Rigidbody.AddForce(new Vector3(x, y, z));
                }

                for (int i = 0; i < _flipObjects.Length - 2; i++)
                {
                    _flipObjects[i].TakeForceOnObject();
                }
            }); 
        }

        [PunRPC]
        private void RPC_CollectTable()
        {
            CameraManager.Instance.SetCamera(0);

            for (int i = _flipObjects.Length - 1; i > _flipObjects.Length - 3; i--)
            {
                _flipObjects[i].transform.position = _flipObjects[i].StartPosition;
                _flipObjects[i].transform.rotation = _flipObjects[i].RotationPosition;
                CheckFinishMove(i);
            }

            _isMove = true;
            Physics.IgnoreLayerCollision(10, 10);

            for (int i = 0; i < _flipObjects.Length; i++)
            {
                _flipObjects[i].SetActiveKinematic(true);
            }

            for (int i = 0; i < _flipObjects.Length - 2; i++)
            {
                if (Vector3.Distance(_flipObjects[i].transform.position, _flipObjects[i].StartPosition) < 0.001f)
                {
                    CheckFinishMove(i);
                    continue;
                }

                _flipObjects[i].transform.position = Vector3.MoveTowards(_flipObjects[i].transform.position, _flipObjects[i].StartPosition, speed * Time.deltaTime);
                _flipObjects[i].transform.rotation = Quaternion.RotateTowards(_flipObjects[i].transform.rotation, _flipObjects[i].RotationPosition, speed * Time.deltaTime * 250);
            }
        }

        private void CheckFinishMove(int count)
        {
            _isMoveFinish[count] = true;

            for(int i = 0; i < _isMoveFinish.Length; i++)
            {
                if (!_isMoveFinish[i])
                    return;
            }

            for (int i = 0; i < _isMoveFinish.Length; i++)
            {
                _isMoveFinish[i] = false;
            }

            StartedFlipTable?.Invoke(true);
            _isMove = false;
            Physics.IgnoreLayerCollision(10, 10, false);
        }
    }
}