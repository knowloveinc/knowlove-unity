using ChartboostSDK;
using GameBrewStudios;
using Knowlove.UI.Menus;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Knowlove.UI
{
    public class StartupCanvas : MonoBehaviour
    {
        [SerializeField]
        Window pickModeWindow, loginWindow, settingsWindow, createMatchWindow, matchListWindow, waitingForPlayersWindow;

        [SerializeField]
        AudioMixer mixer;

        private void Start()
        {


            matchListWindow.Hide();
            settingsWindow.Hide();
            createMatchWindow.Hide();
            waitingForPlayersWindow.Hide();


            mixer.SetFloat("MasterVolume", Mathf.Log(GameSettings.Volume) * 20f);


            if (User.current == null || string.IsNullOrEmpty(User.current._id))
            {
                pickModeWindow.Hide();
                loginWindow.Show();
            }
            else
            {
                loginWindow.Hide();
                pickModeWindow.Show();
            }



#if UNITY_ANDROID
        string appId = CBSettings.getSelectAndroidAppId();
        string appSecret = CBSettings.getSelectAndroidAppSecret();
#else
            string appId = CBSettings.getIOSAppId();
            string appSecret = CBSettings.getIOSAppSecret();
#endif

            CBDataUseConsent consent = CBGDPRDataUseConsent.NoBehavioral;
            Chartboost.addDataUseConsent(consent);

            Chartboost.CreateWithAppId(appId, appSecret);
            StartCoroutine(WaitForChartboost());
        }

        IEnumerator WaitForChartboost()
        {
            Debug.Log("Waiting for Chartboost to finish initialization...?  " + !Chartboost.isInitialized());
            yield return new WaitUntil(() => initialized);

            //Chartboost.showRewardedVideo(CBLocation.Startup);

            Debug.Log("CHARTBOOST IS READY!!!");
        }

        public static void PlayRewardedVideo()
        {
            Debug.Log("PLAYING REWARDED VIDEO");
            //Chartboost.showRewardedVideo(CBLocation.Default);

            Chartboost.showInterstitial(CBLocation.Default);
        }

        public void PlayChartboostRewardedVideo()
        {
            PlayRewardedVideo();
        }


        public void QuitApplication()
        {
            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
            new PopupDialog.PopupButton()
            {
                text = "Yes, Quit",
                onClicked = () =>
                {
                    Application.Quit();
                },
                buttonColor = PopupDialog.PopupButtonColor.Red
            },
            new PopupDialog.PopupButton()
            {
                text = "Cancel",
                onClicked = () =>
                {

                },
                buttonColor = PopupDialog.PopupButtonColor.Plain
            }
            };
            PopupDialog.Instance.Show("Close Know Love", "Are you sure you want to exit Know Love?", buttons);
        }




        void SetupDelegates()
        {
            // Listen to all impression-related events
            Chartboost.didInitialize += didInitialize;
            //Chartboost.didFailToLoadInterstitial += didFailToLoadInterstitial;
            //Chartboost.didDismissInterstitial += didDismissInterstitial;
            //Chartboost.didCloseInterstitial += didCloseInterstitial;
            //Chartboost.didClickInterstitial += didClickInterstitial;
            //Chartboost.didCacheInterstitial += didCacheInterstitial;
            //Chartboost.shouldDisplayInterstitial += shouldDisplayInterstitial;
            Chartboost.didFailToRecordClick += didFailToRecordClick;
            Chartboost.didFailToLoadRewardedVideo += didFailToLoadRewardedVideo;
            Chartboost.didDismissRewardedVideo += didDismissRewardedVideo;
            Chartboost.didCloseRewardedVideo += didCloseRewardedVideo;
            Chartboost.didClickRewardedVideo += didClickRewardedVideo;
            Chartboost.didCacheRewardedVideo += didCacheRewardedVideo;
            Chartboost.shouldDisplayRewardedVideo += shouldDisplayRewardedVideo;
            Chartboost.didCompleteRewardedVideo += didCompleteRewardedVideo;
            Chartboost.didPauseClickForConfirmation += didPauseClickForConfirmation;
            Chartboost.willDisplayVideo += willDisplayVideo;
#if UNITY_IPHONE
            //Chartboost.didDisplayInterstitial += didDisplayInterstitial;
            Chartboost.didDisplayRewardedVideo += didDisplayRewardedVideo;
            //Chartboost.didCompleteAppStoreSheetFlow += didCompleteAppStoreSheetFlow;
#endif
        }

        private void willDisplayVideo(CBLocation obj)
        {
            Debug.Log("willDisplayVideo " + obj.ToString());
        }

        private void didPauseClickForConfirmation()
        {
            Debug.Log("didPauseClickForConfirmation");
        }

        private void didDisplayRewardedVideo(CBLocation obj)
        {
            Debug.Log("didDisplayRewardedVideo " + obj.ToString());
        }

        private void didCompleteRewardedVideo(CBLocation arg1, int arg2)
        {
            Debug.Log("didCompleteRewardedVideo " + arg1.ToString() + "  " + arg2);
        }

        private bool shouldDisplayRewardedVideo(CBLocation arg)
        {
            return true;
        }

        private void didCacheRewardedVideo(CBLocation obj)
        {
            Debug.Log("Cached reward video");
        }

        private void didClickRewardedVideo(CBLocation obj)
        {
            Debug.Log("Clicked reward video");
        }

        private void didCloseRewardedVideo(CBLocation obj)
        {
            Debug.Log("Closed reward video");
        }

        private void didDismissRewardedVideo(CBLocation obj)
        {
            Debug.Log("Dismissed reward video");
        }

        private void didFailToLoadRewardedVideo(CBLocation arg1, CBImpressionError arg2)
        {
            Debug.Log("Failed to load reward video");
        }

        private void didFailToRecordClick(CBLocation arg1, CBClickError arg2)
        {
            Debug.Log("Failed to record click");
        }

        bool initialized = false;

        private void didInitialize(bool initialized)
        {
            this.initialized = initialized;

        }
    }
}
