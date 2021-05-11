using Knowlove.XPSystem;
using UnityEngine;

namespace Knowlove.UI
{
    public class ShareLogic : MonoBehaviour
    {
        private string _shareTitle = "KNOWLOVE";
        [SerializeField] private string _shareMessage = "Let’s install this app and play together";
        [SerializeField] private string _shareAndroidURL = "https://play.google.com/store/apps/details?id=com.KnowLove.KnowLove&hl=en_US&gl=US";
        [SerializeField] private string _shareIPhoneURL = "https://apps.apple.com/us/app/know-love-app/id1520135813";

        public virtual void OnShare()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.Android)
            {
                new NativeShare().SetSubject(_shareTitle).SetText(_shareMessage).SetUrl(_shareAndroidURL)
                .SetCallback((result, shareTarget) => AddShare(result))
                .Share();
            }
            else
            {
                new NativeShare().SetSubject(_shareTitle).SetText(_shareMessage).SetUrl(_shareIPhoneURL)
                .SetCallback((result, shareTarget) => AddShare(result))
                .Share();
            }
        }

        private void AddShare(NativeShare.ShareResult result)
        {
            if (result == NativeShare.ShareResult.Shared)
                InfoPlayer.Instance.PlayerShareApp();
        }
    }
}
