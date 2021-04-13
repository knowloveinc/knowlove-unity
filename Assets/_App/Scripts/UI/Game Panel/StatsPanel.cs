using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Photon.Pun;

namespace Knowlove
{
    public class StatsPanel : MonoBehaviourPun
    {
        [SerializeField] private TurnManager TurnManager;
        [SerializeField] private TextMeshProUGUI label;

        [SerializeField] private RectTransform rect;

        [SerializeField] private Image arrow;

        [SerializeField] private TextMeshProUGUI showHideText;

        [SerializeField] private bool isVisible = false;

        private void Start()
        {
            TurnManager.ShowedStatsForAll += ShowForEveryone;
        }

        private void OnDestroy()
        {
            TurnManager.ShowedStatsForAll -= ShowForEveryone;
        }

        public void SetText(string text)
        {
            label.text = text;
        }

        public void ToggleVisibility()
        {
            float posX = rect.anchoredPosition.x > 0f ? -300f : 64f;
            isVisible = posX > 0;
            DOTween.Kill(rect);
            rect.DOAnchorPosX(posX, 0.5f).OnComplete(() =>
            {
                if (this.isVisible)
                {
                    arrow.transform.localScale = new Vector3(-1, 1, 1);
                    showHideText.text = "HIDE";
                }
                else
                {
                    arrow.transform.localScale = new Vector3(1, 1, 1);
                    showHideText.text = "SHOW";
                }
            });
        }

        public void ShowForEveryone()
        {
            photonView.RPC(nameof(RPC_Show), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_Show()
        {
            float posX = 64f;
            DOTween.Kill(rect);
            rect.DOAnchorPosX(posX, 0.5f).OnComplete(() =>
            {
                this.isVisible = true;
                arrow.transform.localScale = new Vector3(-1, 1, 1);
                showHideText.text = "HIDE";
            });
        }
    }
}
