using Knowlove.UI;
using UnityEngine;
using DG.Tweening;

namespace Knowlove
{
    public class FlipTheTable : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _startPosition;

        [SerializeField] private GameUI _gameUI;

        [SerializeField] private float x = 1200f;
        [SerializeField] private float y = 1200f;
        [SerializeField] private float z = 500f;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _startPosition = transform.position;
        }

        [ContextMenu("Flip")]
        public void FlipTable()
        {
            CameraManager.Instance.SetCamera(2);
            _gameUI.gameObject.SetActive(false);

            DOVirtual.DelayedCall(2f, () => 
            {
                _rigidbody.AddForce(new Vector3(x, y, z));
            }); 
        }
    }
}
