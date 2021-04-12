using DG.Tweening;
using Knowlove.ActionAndPathLogic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Knowlove.UI
{
    public class AvoidSingleCard : MonoBehaviourPunCallbacks
    {
        [SerializeField] private RectTransform _avoidSingleCardSprite;
        [SerializeField] private Transform playerProgressContainer;

        [SerializeField] private PathNodeActionLogic _pathNodeActionLogic;
        [SerializeField] private ProceedActionLogic _proceedActionLogic;

        private void Start()
        {
            _pathNodeActionLogic.UsedAvoidSingleCard += AvoidSingleCardAnimation;
            _proceedActionLogic.UsedAvoidSingleCard += AvoidSingleCardAnimation;
        }

        private void OnDestroy()
        {
            _pathNodeActionLogic.UsedAvoidSingleCard -= AvoidSingleCardAnimation;
            _proceedActionLogic.UsedAvoidSingleCard -= AvoidSingleCardAnimation;
        }

        internal void AvoidSingleCardAnimation(Player player)
        {
            if (player == null) return;

            photonView.RPC(nameof(RPC_AvoidSingleCardAnimation), RpcTarget.All, player);
        }

        [PunRPC]
        public void RPC_AvoidSingleCardAnimation(Player player)
        {
            Transform targetPlayerBar = null;
            foreach (Transform child in playerProgressContainer)
            {
                TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
                if (text.text.ToLower() == player.NickName.ToLower())
                {
                    targetPlayerBar = child;
                    Debug.Log("FOUND TARGET PLAYER BAR");
                    break;
                }
            }

            if (targetPlayerBar == null)
                return;

            _avoidSingleCardSprite.anchoredPosition = new Vector2(0f, -1080f);
            _avoidSingleCardSprite.localScale = new Vector3(0.1f, .1f, .1f);
            DOTween.Kill(_avoidSingleCardSprite);
            _avoidSingleCardSprite.DORotate(new Vector3(0f, 0f, 360f * 3f), 0.22f);
            _avoidSingleCardSprite.DOScale(Vector3.one, 0.25f).OnComplete(() =>
            {
                _avoidSingleCardSprite.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.25f);
            });
            _avoidSingleCardSprite.DOAnchorPos(Vector2.zero, 0.25f).OnComplete(() =>
            {
                Vector2 targetPos = targetPlayerBar != null ? targetPlayerBar.GetComponent<RectTransform>().anchoredPosition : Vector2.zero;
                _avoidSingleCardSprite.DOAnchorPos(targetPos, 0.5f).SetDelay(1f);
                _avoidSingleCardSprite.DOScale(Vector3.zero, 0.5f).SetDelay(1f).OnComplete(() =>
                {
                    Debug.Log("Avoid signle card animation finished");
                });
            });
        }
    }
}
