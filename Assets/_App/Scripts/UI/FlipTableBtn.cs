using UnityEngine;
using UnityEngine.SceneManagement;
using Knowlove.FlipTheTableLogic;
using Photon.Pun;
using Photon.Realtime;

namespace Knowlove.UI
{
    public class FlipTableBtn : MonoBehaviour
    {
        public static FlipTableBtn Instance;

        [SerializeField] private GameObject _flipThetableBtn;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
             _flipThetableBtn.gameObject.SetActive(false);      
        }

        public void TurnOn(Player currentPlayer)
        {
            if (PhotonNetwork.LocalPlayer == currentPlayer)
            {
                _flipThetableBtn.gameObject.SetActive(true);
            }
            else
                _flipThetableBtn.gameObject.SetActive(false);
        }

        public void ActiveButton()
        {
            if(FlipTheTable.Instance != null)
                FlipTheTable.Instance.FlipTable();
        }
    }
}
