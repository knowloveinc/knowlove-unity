using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace Knowlove.UI
{
    public class CardResetter : MonoBehaviour
    {
        [SerializeField] private CardUI _cardUI;

        private RectTransform _rectTransform;
        private RectTransform _cardRectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _cardRectTransform = _cardUI.GetComponent<RectTransform>();
        }

        private void Start()
        {
            LeanTouch.OnFingerDown += this.LeanTouch_OnFingerDown;
        }

        private void Update()
        {
            if (DOTween.IsTweening(gameObject, true)) return;
            else
            {
                List<LeanFinger> fingers = LeanTouch.Fingers;
                if (fingers == null || fingers.Count < 1 && (transform.localScale != Vector3.one || transform.localPosition != Vector3.zero) && _cardUI.IsShowCard)
                {
                    _rectTransform.DOAnchorPos(Vector2.zero, 0.1f).SetId(gameObject);

                    CheckMaxMinScale();
                }

                if (!_cardUI.IsShowCard && transform.position.y != -1080f && transform != null)
                {
                    transform.DOScale(1f, 0.1f).SetId(gameObject);
                    _cardRectTransform.DOAnchorPosY(-1080f, 0.25f).OnComplete(() => { });
                }
            }
        }

        private void OnDestroy()
        {
            LeanTouch.OnFingerDown -= LeanTouch_OnFingerDown;
        }

        private void CheckMaxMinScale()
        {
            if (transform == null)
                return;

            if (transform.localScale.x > 3f || transform.localScale.y > 3f || transform.localScale.z > 3f)
            {                
                transform.DOScale(3f, 0.1f).SetId(gameObject);
                _rectTransform.DOAnchorPos(Vector2.zero, 0.1f).SetId(gameObject);
            }

            if (transform.localScale.x < 0.5f || transform.localScale.y < 0.5f || transform.localScale.z < 0.5f)
                ReturnStartPos();
        }

        private void ReturnStartPos()
        {
            if (transform == null)
                return;

            transform.DOScale(1f, 0.1f).SetId(gameObject);

            _rectTransform.DOAnchorPos(Vector2.zero, 0.1f).SetId(gameObject);
        }

        private void LeanTouch_OnFingerDown(LeanFinger finger)
        {
            if (transform == null) return;

            DOTween.Kill(transform);
            Debug.LogWarning("Fingers on screen, canceling any tweens for card that might be running");
        }
    }
}

