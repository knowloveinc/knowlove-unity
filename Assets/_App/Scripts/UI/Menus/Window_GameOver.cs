using UnityEngine;
using DG.Tweening;
using TMPro;
using Knowlove.XPSystem;

namespace Knowlove.UI.Menus
{
    public class Window_GameOver : Window
    {
        string winnerName = "";

        [SerializeField] private TurnManager TurnManager;

        [SerializeField] private TextMeshProUGUI winnerNameLabel;

        [SerializeField] private CanvasGroup[] groups;

        [SerializeField] private GameObject hud;

        private void Awake()
        {
            TurnManager.PlayerWonName += ShowGameOver;
            gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            hud.SetActive(false);
        }

        private void OnDisable()
        {
            hud.SetActive(true);
        }
        private void OnDestroy()
        {
            TurnManager.PlayerWonName -= ShowGameOver;
        }
        public void Init(string winnerName)
        {
            this.winnerName = winnerName;
        }

        public void ShowGameOver(string winnerName)
        {
            if (winnerName.ToLower() == InfoPlayer.Instance.PlayerState.playerName.ToLower())
                InfoPlayer.Instance.PlayerWin();
            else
                InfoPlayer.Instance.PlayerEndGame();

            Init(winnerName);
            Show();
        }

        public override void Show()
        {
            MusicManager.Instance.PlaySong(1);

            CameraManager.Instance.RPC_SetCamera(3);
            base.Show();

            winnerNameLabel.text = this.winnerName;

            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].alpha = 0;
                groups[i].DOFade(1f, 2f).SetDelay(i * 1f);
            }
        }
    }
}

