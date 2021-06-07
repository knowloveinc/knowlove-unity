using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Knowlove.XPSystem
{
    public class RankImage : MonoBehaviour
    {
        [SerializeField] private AnimationClip[] _ranks;
        [SerializeField] private GameObject[] posotions;

        private Animator _animatorRanks;

        private void Awake()
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                gameObject.SetActive(true);

                if ((Screen.width / Screen.height) <= 1.4)
                    transform.position = posotions[0].transform.position;
                else
                    transform.position = posotions[1].transform.position;

                gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            _animatorRanks = GetComponent<Animator>();           

            InfoPlayer.Instance.statusPlayer.ChangedPlayerStatus += ChangeRank;
            InfoPlayer.Instance.SettedPlayer += ChangeRank;
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
            InfoPlayer.Instance.SettedPlayer -= ChangeRank;
        }

        public void ChangeRank()
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isBronzeStatus && !playerXP.isSilverStatus && !playerXP.isGoldStatus)
            {
                gameObject.SetActive(true);
                _animatorRanks.CrossFade(_ranks[0].name, 0);
            }
            else if (playerXP.isSilverStatus && !playerXP.isGoldStatus)
            {
                gameObject.SetActive(true);
                _animatorRanks.CrossFade(_ranks[1].name, 0);
            }
            else if (playerXP.isGoldStatus)
            {
                gameObject.SetActive(true);
                _animatorRanks.CrossFade(_ranks[2].name, 0);
            }
            else
                gameObject.SetActive(false);
        }
    }
}
