using DG.Tweening;
using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove.UI
{
    public class BrowseCardResetter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LeanSelectable _leanSelectable;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {        
            LeanTouch.OnFingerDown += this.LeanTouch_OnFingerDown;
        }

        private void Update()
        {
            if (_canvasGroup.alpha == 1)
                _leanSelectable.IsSelected = true;
            else
                _leanSelectable.IsSelected = false;

            if (DOTween.IsTweening(gameObject, true)) return;
            else
            {
                List<LeanFinger> fingers = LeanTouch.Fingers;
                if (fingers == null || fingers.Count < 1 && (transform.localScale != Vector3.one || transform.localPosition != Vector3.zero))
                {
                    Debug.LogWarning("Starting return to origin process for card");

                    _rectTransform.DOAnchorPos(Vector2.zero, 0.1f).SetId(gameObject);

                    if (_canvasGroup.alpha == 0)
                        ReturnStartPos(1f);

                    CheckMaxMinScale();
                }
            }
        }

        private void OnEnable()
        {
            ReturnStartPos(1f);
        }

        private void OnDestroy()
        {
            LeanTouch.OnFingerDown -= LeanTouch_OnFingerDown;
        }

        private void CheckMaxMinScale()
        {
            if (transform.localScale.x > 1.385f || transform.localScale.y > 1.385f || transform.localScale.z > 1.385f)
                ReturnStartPos(1.385f);


            if (transform.localScale.x < 0.5f || transform.localScale.y < 0.5f || transform.localScale.z < 0.5f)
                ReturnStartPos(1f);
        }

        private void ReturnStartPos(float scale)
        {
            if (transform == null)
                return;

            transform.DOScale(scale, 0.1f).SetId(gameObject);

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

