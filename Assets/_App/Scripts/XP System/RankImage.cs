using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Knowlove.XPSystem
{
    public class RankImage : MonoBehaviour
    {
        private Image _rankImage;

        [SerializeField] private Sprite[] _ranksSprite;

        private void Start()
        {
            _rankImage = GetComponent<Image>();            

            InfoPlayer.Instance.statusPlayer.ChangedPlayerStatus += ChangeRank;
            gameObject.SetActive(false);

            DOVirtual.DelayedCall(6f, () =>
            {
                if (SceneManager.GetActiveScene().buildIndex == 1)
                    ChangeRank();
            });            
        }

        private void OnDestroy()
        {
            InfoPlayer.Instance.statusPlayer.ChangedPlayerStatus -= ChangeRank;
        }

        public void ChangeRank()
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isBronzeStatus && !playerXP.isSilverStatus && !playerXP.isGoldStatus)
            {
                gameObject.SetActive(true);
                _rankImage.sprite = _ranksSprite[0];
            }
            else if (playerXP.isSilverStatus && !playerXP.isGoldStatus)
            {
                gameObject.SetActive(true);
                _rankImage.sprite = _ranksSprite[1];
            }
            else if (playerXP.isGoldStatus)
            {
                gameObject.SetActive(true);
                _rankImage.sprite = _ranksSprite[2];
            }
            else
                gameObject.SetActive(false);
        }
    }
}
