using UnityEngine;

namespace Knowlove.FlipTheTableLogic
{
    public class FlipObject : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Quaternion _rotationPosition;
        private Rigidbody _rigidbodyComponent;
        private BoxCollider _boxCollider;

        [SerializeField] private bool _isPiece = false;

        [SerializeField] private bool isRightForce = false;
        [SerializeField] private bool isLeftForce = false;

        [SerializeField] private float x = 0;
        [SerializeField] private float y = 0;
        [SerializeField] private float z = 0;

        public Vector3 StartPosition
        {
            get => _startPosition;
        }

        public Quaternion RotationPosition
        {
            get => _rotationPosition;
        }

        public Rigidbody Rigidbody
        {
            get => _rigidbodyComponent;
        }

        private void Awake()
        {
            _rigidbodyComponent = GetComponent<Rigidbody>();
            _startPosition = transform.position;
            _rotationPosition = transform.rotation;

            if (_isPiece)
                _boxCollider = GetComponent<BoxCollider>();
        }

        public void SetActiveKinematic(bool isActive)
        {
            if(gameObject.activeSelf)
                _rigidbodyComponent.isKinematic = isActive;
        }

        public void SetPiecePosition()
        {
            if (_isPiece)
                _startPosition = transform.position;
        }

        public void SetActiveBoxCollider(bool isActive)
        {
            if (_isPiece && gameObject.activeSelf)
                _boxCollider.enabled = isActive;
        }

        public void TakeForceOnObject()
        {
            if (isRightForce)
                _rigidbodyComponent.AddForce(-x, y, z);
            else if (isLeftForce)
                _rigidbodyComponent.AddForce(x, y, z);
        }
    }
}
