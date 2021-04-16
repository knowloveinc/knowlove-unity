using UnityEngine;
using UnityEngine.SceneManagement;
using Knowlove.FlipTheTableLogic;

namespace Knowlove.UI
{
    public class FlipTableBtn : MonoBehaviour
    {
        [SerializeField] private GameObject _flipButton;

        private void OnEnable()
        {
            if(StoreController.Instance != null)
            {
                if (SceneManager.GetActiveScene().buildIndex == 0 || StoreController.Instance.IsOpenStore)
                    _flipButton.gameObject.SetActive(false);
                else
                    _flipButton.gameObject.SetActive(true);
            }                  
        }

        public void ActiveButton()
        {
            if(FlipTheTable.Instance != null)
                FlipTheTable.Instance.FlipTable();
        }
    }
}
