using ChartboostSDK;
using GameBrewStudios;
using UnityEngine;

namespace Knowlove.UI
{
    public class RewardVideoButton : MonoBehaviour
    {
        public static float lastPlayTime = 0f;

        public GameObject buttonChild;

        private void Start()
        {
            Chartboost.didCompleteRewardedVideo += this.Chartboost_didDisplayRewardedVideo;
        }
        private void Update()
        {
            buttonChild.SetActive(lastPlayTime == 0f || Time.time - lastPlayTime >= (60f * 30f));
        }

        private void Chartboost_didDisplayRewardedVideo(CBLocation obj, int reward)
        {
            lastPlayTime = Time.time;
            User.current.AddCurrency(reward, (wallet) =>
            {
                Debug.Log("ADDED " + reward + " TO WALLET FOR TOTAL OF: " + wallet);
            });
        }

        public void OnClick()
        {
            StartupCanvas.PlayRewardedVideo();
        }
    }
}