using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace Knowlove
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance;

        public AudioSource[] players;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            SceneManager.activeSceneChanged += this.SceneManager_activeSceneChanged;

            for (int i = 0; i < players.Length; i++)
            {
                players[i].volume = 0f;
            }

            PlaySong(0);
        }

        private void SceneManager_activeSceneChanged(Scene current, Scene next)
        {
            if (next == SceneManager.GetSceneByBuildIndex(0))
            {
                PlaySong(0);
            }
        }

        public void PlaySong(int index)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (!players[i].isPlaying && i == index)
                {
                    players[i].Play();
                }
                players[i].DOFade(i == index ? 1f : 0f, 0.5f).SetEase(Ease.Linear);
            }
        }
    }
}
