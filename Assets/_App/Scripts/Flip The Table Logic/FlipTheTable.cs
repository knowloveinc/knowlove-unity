using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using System;
using Knowlove.UI;
using Photon.Realtime;
using GameBrewStudios;

namespace Knowlove.FlipTheTableLogic
{
    public class FlipTheTable : MonoBehaviourPunCallbacks
    {
        private static FlipTheTable _instance;

        private Rigidbody _rigidbody;
        private Vector3 _startPosition;

        [SerializeField] private CardUI _cardUI;
        [SerializeField] private FlipObject[] _flipObjects;

        [SerializeField] private float x = 0f;
        [SerializeField] private float y = 5500f;
        [SerializeField] private float z = 0f;
        [SerializeField] private float speed = 1f;
        [SerializeField] private int cameraNumber = 4;

        private bool[] _isMoveFinish;
        private bool _isMove;
        private bool _isPopup;
        private bool _isFlip;
        private bool[] _players;

        public Action<bool> StartedFlipTable;

        public static FlipTheTable Instance
        {
            get => _instance;
        }

        public bool IsMove
        {
            get => _isMove;
        }

        private void Start()
        {
            _instance = this;

            _isPopup = false;
            _isFlip = false;
            _rigidbody = GetComponent<Rigidbody>();
            _startPosition = transform.position;
            _isMove = false;
            _isMoveFinish = new bool[_flipObjects.Length];

            for(int i = 0; i < _isMoveFinish.Length; i++)
            {
                _isMoveFinish[i] = false;
            }

            _players = new bool[NetworkManager.Instance.players.Count];
        }

        private void FixedUpdate()
        {
            if (_isMove)
                CollectTable();
        }

        public void FlipTable()
        {
            photonView.RPC(nameof(StartFlipTable), RpcTarget.MasterClient);                    
        }

        [PunRPC]
        public void StartFlipTable()
        {
            if (!_isFlip)
            {
                _isFlip = true;
                photonView.RPC(nameof(RPC_FlipTable), RpcTarget.All);
            }            
        }

        [PunRPC]
        private void RPC_FlipTable()
        {
            StartedFlipTable?.Invoke(false);
            PopupDialog.Instance.gameObject.SetActive(false);

            DOVirtual.DelayedCall(0.5f, () =>
            {
                CameraManager.Instance.SetCamera(cameraNumber);
                SetPlayersArray(false);

                DOVirtual.DelayedCall(0.5f, () =>
                {
                    CheckSomeAction();
                });

                DOVirtual.DelayedCall(0.7f, () =>
                {
                    for (int i = 0; i < _flipObjects.Length - 2; i++)
                    {
                        _flipObjects[i].SetPiecePosition();
                        _flipObjects[i].SetActiveKinematic(false);
                    }

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

                DOVirtual.DelayedCall(5.5f, () =>
                {
                    CollectTable();

                    for (int i = _flipObjects.Length - 1; i > _flipObjects.Length - 3; i--)
                    {
                        _flipObjects[i].transform.position = _flipObjects[i].StartPosition;
                        _flipObjects[i].transform.rotation = _flipObjects[i].RotationPosition;
                        CheckFinishMove(i);
                    }

                    DOVirtual.DelayedCall(7f, () =>
                    {
                        CheckBackTable();
                    });
                });
            });           
        }

        private void CollectTable()
        {
            CameraManager.Instance.SetCamera(0);

            _isMove = true;
            Physics.IgnoreLayerCollision(10, 10);

            for (int i = 0; i < _flipObjects.Length; i++)
            {
                _flipObjects[i].SetActiveKinematic(true);
            }

            MoveObject();
        }

        private void MoveObject()
        {
            for (int i = 0; i < _flipObjects.Length - 2; i++)
            {
                if (Vector3.Distance(_flipObjects[i].transform.position, _flipObjects[i].StartPosition) < 0.001f)
                {
                    CheckFinishMove(i);
                    continue;
                }

                _flipObjects[i].transform.position = Vector3.MoveTowards(_flipObjects[i].transform.position, _flipObjects[i].StartPosition, speed * Time.deltaTime * 1.5f);
                _flipObjects[i].transform.rotation = Quaternion.RotateTowards(_flipObjects[i].transform.rotation, _flipObjects[i].RotationPosition, speed * Time.deltaTime * 400);
            }
        }

        private void CheckSomeAction()
        {
            if (PopupDialog.Instance.canvasGroup.alpha == 1)
            {
                _isPopup = true;
                PopupDialog.Instance.canvasGroup.alpha = 0;
            }

            if (StoreController.Instance.IsOpenStore)
                StoreController.Instance.gameObject.SetActive(false);
        }

        private void ReturnAction()
        {
            if (_cardUI.IsShowCard)
                _cardUI.LeanSelectable.Select();

            if (_isPopup)
            {
                _isPopup = false;
                PopupDialog.Instance.canvasGroup.alpha = 1;
            }

            if (StoreController.Instance.IsOpenStore)
                StoreController.Instance.gameObject.SetActive(true);
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

            _isMove = false;
            Physics.IgnoreLayerCollision(10, 10, false);

            int number = 0;

            foreach(Player player in NetworkManager.Instance.players)
            {
                if (player == PhotonNetwork.LocalPlayer)
                    break;
                else
                    number++;
            }

            photonView.RPC(nameof(WaitAllPlayer), RpcTarget.MasterClient, number);
        }

        [PunRPC]
        private void WaitAllPlayer(int number)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            _players[number] = true;

            for(int i = 0; i < _players.Length; i++)
            {
                if (!_players[i])
                    return;
            }

            _isFlip = false;
            photonView.RPC(nameof(EndWait), RpcTarget.All);
        }

        [PunRPC]
        private void EndWait()
        {
            ReturnAction();
            PopupDialog.Instance.gameObject.SetActive(true);
            StartedFlipTable?.Invoke(true);
        }

        private void SetPlayersArray(bool isWait)
        {
            for(int i = 0; i < _players.Length; i++)
            {
                _players[i] = isWait;
            }
        }

        private void CheckBackTable()
        {
            if (!PopupDialog.Instance.gameObject.activeSelf)
            {
                for (int i = 0; i < _flipObjects.Length - 2; i++)
                {
                    _flipObjects[i].transform.position = _flipObjects[i].StartPosition;
                    _flipObjects[i].transform.rotation = _flipObjects[i].RotationPosition;

                    CheckFinishMove(i);
                }
            }
        }
    }
}