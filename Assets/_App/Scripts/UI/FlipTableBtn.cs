using UnityEngine;
using UnityEngine.SceneManagement;
using Knowlove.FlipTheTableLogic;
using Photon.Pun;

namespace Knowlove.UI
{
    public class FlipTableBtn : MonoBehaviour
    {
        [SerializeField] private GameObject _flipThetableBtn;

        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0 || StoreController.Instance.IsOpenStore)
                _flipThetableBtn.gameObject.SetActive(false);
            else if(TurnManager.Instance != null && GamePrompt.Instance != null)
            {
                if (PhotonNetwork.LocalPlayer == GamePrompt.Instance.currentPlayer)
                {
                    _flipThetableBtn.gameObject.SetActive(true);
                }
                else
                    _flipThetableBtn.gameObject.SetActive(false);
            }                  
        }

        public void ActiveButton()
        {
            if(FlipTheTable.Instance != null)
                FlipTheTable.Instance.FlipTable();
        }
    }
}
