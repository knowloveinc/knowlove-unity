using UnityEngine;
using DG.Tweening;
using TMPro;
using Knowlove.XPSystem;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace Knowlove.UI.Menus
{
    public class Window_GameOver : Window
    {
        [SerializeField] private Button _button;

        [SerializeField] private TurnManager TurnManager;

        [SerializeField] private TextMeshProUGUI winnerNameLabel;

        [SerializeField] private CanvasGroup[] groups;

        [SerializeField] private GameObject hud;

        private string winnerName = "";

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
            {
                InfoPlayer.Instance.PlayerEndGame();
               
                _button.onClick.RemoveAllListeners();

                _button.onClick.AddListener(() => 
                {
                    ButtonLeaveRoom();
                });
            }

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

        public void ButtonLeaveRoom()
        {
            LeaveRoom(PhotonNetwork.LocalPlayer);
        }

        private void LeaveRoom(Player player)
        {
            NetworkManager.Instance.isLeave = true;
            NetworkManager.Instance.isReconnect = false;

            if (PhotonNetwork.LocalPlayer == player)
                PhotonNetwork.LeaveRoom();
        }
    }
}

