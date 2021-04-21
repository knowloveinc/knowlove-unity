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

                    RectTransform rt = GetComponent<RectTransform>();
                    rt.DOAnchorPos(Vector2.zero, 0.1f).SetId(gameObject);

                    if (_canvasGroup.alpha == 0)
                        ReturnStartPos();

                    CheckMaxMinScale();
                }
            }
        }

        private void OnEnable()
        {
            ReturnStartPos();
        }

        private void OnDestroy()
        {
            LeanTouch.OnFingerDown -= LeanTouch_OnFingerDown;
        }

        private void CheckMaxMinScale()
        {
            if (transform.localScale.x > 2f || transform.localScale.y > 2f || transform.localScale.z > 2f)
                ReturnStartPos();


            if (transform.localScale.x < 0.5f || transform.localScale.y < 0.5f || transform.localScale.z < 0.5f)
                ReturnStartPos();
        }

        private void ReturnStartPos()
        {
            transform.DOScale(1f, 0.1f).SetId(gameObject);

            if (transform is RectTransform)
            {
                RectTransform rt = GetComponent<RectTransform>();
                rt.DOAnchorPos(Vector2.zero, 0.1f).SetId(gameObject);
            }
            else
                transform.DOLocalMove(Vector3.zero, 0.1f).SetId(gameObject);
        }

        private void LeanTouch_OnFingerDown(LeanFinger finger)
        {
            if (transform == null) return;

            DOTween.Kill(transform);
            Debug.LogWarning("Fingers on screen, canceling any tweens for card that might be running");
        }
    }
}

