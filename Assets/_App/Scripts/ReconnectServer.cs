using GameBrewStudios.Networking;
using Knowlove.UI;
using UnityEngine;
using DG.Tweening;
using GameBrewStudios;

namespace Knowlove
{
    public class ReconnectServer : MonoBehaviour
    {
        private const string USERNAME_KEY = "SavedUsername", PASSWORD_KEY = "SavedPassw";

        private float _pastTime = 0;
        private float _reconnectTime = 420f;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if(User.current != null)
            {
                DOVirtual.DelayedCall(1f, () =>
                {
                    ReconnectUser();
                });
            }           
        }

        private void Update()
        {
            if(_pastTime < _reconnectTime)
                _pastTime += Time.deltaTime;
            else
            {
                _pastTime = 0f;
                ReconnectUser();
            }
        }

        private void OnDestroy()
        {
            ReconnectUser();
        }

        private void ReconnectUser()
        {
            Debug.Log("Recconect");
            CanvasLoading.Instance.Show();

            string name = PlayerPrefs.GetString(USERNAME_KEY);
            string password = PlayerPrefs.GetString(PASSWORD_KEY);

            APIManager.Authenticate(name, password, (success) =>
            {
                CanvasLoading.Instance.Hide();

                Debug.LogWarning("FINISHED Recconect: " + success);
            });
        }
    }

}