using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.XPSystem
{
    public class RankPopup : MonoBehaviour
    {
        [SerializeField] private GameObject[] _rankPopupWindows;
        [SerializeField] private StatusPlayer _statusPlayer;

        private Image _canvasPanel;

        private void Awake()
        {
            _canvasPanel = GetComponent<Image>();

            _canvasPanel.enabled = false;

            _statusPlayer.RewardedPlayer += ShowPopup;
        }

        private void OnDestroy()
        {
            _statusPlayer.RewardedPlayer -= ShowPopup;
        }

        private void ShowPopup(int number)
        {
            if (_rankPopupWindows.Length != 3)
                return;

            _canvasPanel.enabled = true;
            _rankPopupWindows[number].SetActive(true);
        }
    }
}
