using Knowlove.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.XPSystem
{
    public class Statistic : MonoBehaviour
    {
        [SerializeField] private Sprite[] _lockSprite;
        [SerializeField] private Image _imageBronze;
        [SerializeField] private Image _imagesilver;
        [SerializeField] private Image _imageGold;

        private void OnEnable()
        {
            CanvasLoading.Instance.Show();
            UpdateFildText();
        }

        private void UpdateFildText()
        {
            PlayerXP player = InfoPlayer.Instance.PlayerState;

            if (player.isBronzeStatus)
                _imageBronze.sprite = _lockSprite[1];
            else
                _imageBronze.sprite = _lockSprite[0];

            if (player.isSilverStatus)
                _imagesilver.sprite = _lockSprite[1];
            else
                _imagesilver.sprite = _lockSprite[0];

            if (player.isGoldStatus)
                _imageGold.sprite = _lockSprite[1];
            else
                _imageGold.sprite = _lockSprite[0];

            CanvasLoading.Instance.Hide();
        }
    }
}
