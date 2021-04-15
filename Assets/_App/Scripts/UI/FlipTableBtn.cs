using UnityEngine;
using UnityEngine.SceneManagement;

namespace Knowlove.UI
{
    public class FlipTableBtn : MonoBehaviour
    {
        [SerializeField] private GameObject _flipButton;

        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
                _flipButton.gameObject.SetActive(false);
            else
            {
                _flipButton.gameObject.SetActive(true);
            }                
        }
    }
}
