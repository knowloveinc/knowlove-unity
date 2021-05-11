using GameBrewStudios;
using GameBrewStudios.Networking;
using UnityEngine;
using UnityEngine.Advertisements;

namespace Knowlove.UI
{
    public class RewardVideoButton : MonoBehaviour
    {
        public static float lastPlayTime = 0f;

        public GameObject buttonChild;

        [SerializeField] private int _reward = 200;

        private void Start()
        {
            if (Advertisement.isSupported && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.Android))
                Advertisement.Initialize("4125675", false);
            else if(Advertisement.isSupported && (Application.platform == RuntimePlatform.IPhonePlayer))
                Advertisement.Initialize("4125674", false);
        }

        private void Update()
        {
            buttonChild.SetActive(lastPlayTime == 0f || Time.time - lastPlayTime >= (60f * 30f));
        }

        public void OnClick()
        {
            if (Advertisement.IsReady())
            {
                ShowOptions showOptions = new ShowOptions();

                showOptions.resultCallback += RewardPlayer;

                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.Android)
                    Advertisement.Show("Rewarded_Android", showOptions);
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    Advertisement.Show("Rewarded_iOS", showOptions);
            }
        }

        private void RewardPlayer(ShowResult result)
        {
            if(result == ShowResult.Finished)
            {
                APIManager.GetUserDetails((user) => 
                {
                    APIManager.AddCurrency(_reward, balance => 
                    {
                        User.current.wallet = balance;
                        StoreController.Instance.UpdateFromPlayerWallet();
                    });
                });
            }
        }
    }
}