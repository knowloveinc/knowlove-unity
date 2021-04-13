using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Knowlove.UI
{
    public class RefreshableScrollRect : ScrollRect
    {
        private const string _activityIndicatorStartLoadingName = "Loading";

        public GameObject loadingAnimatorParent;

        public bool isDragging = false;

        [SerializeField] private Animator loadingAnimator;
        [SerializeField] private UnityEvent onRefreshed = new UnityEvent();

        [SerializeField] private float refreshDistance = 150f;

        private float startPosition;
        private float progress;
        private bool isPulled;
        private bool refreshInProgress;
        private Vector2 stopPosition;

        /// <summary>
        /// Progress until refreshing begins. (0-1)
        /// </summary>
        public float Progress
        {
            get => progress; 
        }

        /// <summary>
        /// Refreshing?
        /// </summary>
        public bool IsRefreshing
        {
            get => refreshInProgress; 
        }

        protected override void Start()
        {
            base.Start();

            startPosition = GetYPosition();
            stopPosition = new Vector2(content.anchoredPosition.x, startPosition - refreshDistance);
            
            onValueChanged.AddListener(OnScroll);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (!isPulled)
                return;

            if (!refreshInProgress)
                return;

            content.anchoredPosition = stopPosition;
        }

        /// <summary>
        /// Call When Refresh is End.
        /// </summary>
        public void EndRefreshing()
        {
            isPulled = false;
            refreshInProgress = false;
            //loadingAnimator.SetBool(_activityIndicatorStartLoadingName, false);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            EndRefreshing();

            isDragging = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            isDragging = false;
        }

        private void OnScroll(Vector2 normalizedPosition)
        {
            float distance = startPosition - GetYPosition();

            CheckDistance(distance);
        }

        private void CheckDistance(float distance)
        {
            //When a scroll rect is pulled down, its value is positive, so ignore negative values because thats normal scrolling.
            if (distance < 0f)
                return;

            if (refreshInProgress && Mathf.Abs(distance) < 1f)
                refreshInProgress = false;

            if (isPulled && isDragging)
                return;

            progress = distance / refreshDistance;

            if (progress < 1f)
                return;

            //Show loading animation
            if (isDragging)
                isPulled = true; //loadingAnimator.SetBool(_activityIndicatorStartLoadingName, true);

            //Fire onRefreshed event
            if (isPulled && !isDragging)
            {
                refreshInProgress = true;
                onRefreshed.Invoke();
            }

            progress = 0f;
        }

        private float GetYPosition()
        {
            return content.anchoredPosition.y;
        }
    }
}
