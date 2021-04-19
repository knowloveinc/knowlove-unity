using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace Knowlove.UI
{
    public class CardResetter : MonoBehaviour
    {
        [SerializeField] private CardUI _cardUI;
        [SerializeField] private GameObject _maps;

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
                    Debug.LogWarning("Starting return to origin process for card");
                    if (transform is RectTransform && _cardUI.IsShowCard)
                    {
                        RectTransform rt = GetComponent<RectTransform>();
                        rt.DOAnchorPos(Vector2.zero, 0.1f).SetId(gameObject);
                    }
                    else if(_cardUI.IsShowCard && !(transform is RectTransform))
                        transform.DOLocalMove(Vector3.zero, 0.1f).SetId(gameObject);

                    if(!_cardUI.IsShowCard && !_maps.gameObject.activeSelf)
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

