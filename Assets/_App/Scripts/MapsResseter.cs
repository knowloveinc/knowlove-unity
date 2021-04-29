using DG.Tweening;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove
{
    public class MapsResseter : MonoBehaviour
    {
        private Vector3 maxPosition;

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
                if (fingers == null || fingers.Count < 1 && (transform.localScale != Vector3.one || transform.localPosition != Vector3.zero))
                {
                    CheckMaxMinScale();

                    if (!gameObject.activeSelf)
                        ReturnStartPos();
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
            if (transform == null)
                return;

            if (transform.localScale.x > 3f || transform.localScale.y > 3f || transform.localScale.z > 3f)
            {
                transform.DOScale(3f, 0.1f).SetId(gameObject);
                ReturnStartPos();
            }
                

            if (transform.localScale.x < 0.5f || transform.localScale.y < 0.5f || transform.localScale.z < 0.5f)
                ReturnStartPos();
        }

        private void ReturnStartPos()
        {
            if (transform == null)
                return;

            transform.DOScale(1f, 0.1f).SetId(gameObject);
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
