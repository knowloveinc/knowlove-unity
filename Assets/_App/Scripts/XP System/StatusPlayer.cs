using GameBrewStudios;
using GameBrewStudios.Networking;
using Knowlove.UI.Menus;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.XPSystem
{
    public class StatusPlayer : MonoBehaviour
    {
        [SerializeField] private Window_GameOver _window_GameOver;
        [SerializeField] private Button[] _rankButton;
        [SerializeField] private Button _winButton;

        public Action ChangedPlayerStatus;
        public Action<int> RewardedPlayer;

        public void CheckPlayerStatus(bool isWinGame = false)
        {
            if (isWinGame)
            {
                _winButton.onClick.RemoveAllListeners();

                _winButton.onClick.AddListener(() =>
                {
                    _window_GameOver.ButtonLeaveRoom();
                });
            }

            CheckBronze(isWinGame);
            CheckSilver(isWinGame);
            CheckGold(isWinGame);            
        }

        private void CheckBronze(bool isWinGame)
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isBronzeStatus)
                return;

            if (playerXP.countDifferentPlayers < 5)
                return;

            if (playerXP.winGame < 3)
                return;

            if (playerXP.shareGame < 3)
                return;

            for (int i = 0; i < playerXP.playerDeckCard.datingCard.Length; i++)
            {
                if (!playerXP.playerDeckCard.datingCard[i])
                    return;
            }

            GetReward("bronze", isWinGame);
        }

        private void CheckSilver(bool isWinGame)
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isSilverStatus)
                return;

            if (playerXP.countDifferentPlayers < 10)
                return;

            if (playerXP.winGame < 7)
                return;

            if (playerXP.shareGame < 5)
                return;

            if (!playerXP.isBronzeStatus)
                return;

            for (int i = 0; i < playerXP.playerDeckCard.relationshipCard.Length; i++)
            {
                if (!playerXP.playerDeckCard.relationshipCard[i])
                    return;
            }

            GetReward("silver", isWinGame);
        }

        private void CheckGold(bool isWinGame)
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isSilverStatus)
                return;

            if (playerXP.countDifferentPlayers < 15)
                return;

            if (playerXP.winGame < 10)
                return;

            if (playerXP.shareGame < 7)
                return;

            if (!playerXP.isBronzeStatus && !playerXP.isSilverStatus)
                return;

            for (int i = 0; i < playerXP.playerDeckCard.marriagepCard.Length; i++)
            {
                if (!playerXP.playerDeckCard.marriagepCard[i])
                    return;
            }

            GetReward("gold", isWinGame);
        }

        private void GetReward(string status, bool isWinGame)
        {
            if (isWinGame)
            {
                ShowWinPanel(status);
                return;
            }                

            switch (status)
            {
                case "bronze":
                    InfoPlayer.Instance.PlayerState.isBronzeStatus = true;
                    RewardedPlayer?.Invoke(0);                    
                    break;
                case "silver":
                    InfoPlayer.Instance.PlayerState.ProtectedFromBackToSingleInMarriagePerGame = true;
                    InfoPlayer.Instance.PlayerState.isSilverStatus = true;
                    RewardedPlayer?.Invoke(1);
                    break;
                case "gold":
                    InfoPlayer.Instance.PlayerState.ProtectedFromBackToSinglePerGame = true;
                    InfoPlayer.Instance.PlayerState.isGoldStatus = true;
                    RewardedPlayer?.Invoke(2);
                    break;
                default:
                    break;
            }

            ChangedPlayerStatus?.Invoke();
            InfoPlayer.Instance.JSONPlayerInfo();

            APIManager.AddItem(status, 1, (inventory) =>
            {
                User.current.inventory = inventory;
            });
        }

        private void ShowWinPanel(string status = "")
        {
            if (_window_GameOver == null)
                return;

            if (TurnManager.Instance.turnState != TurnManager.TurnState.GameOver)
                return;

            _winButton.onClick.RemoveAllListeners();

            _winButton.onClick.AddListener(() =>
            {
                _window_GameOver.gameObject.SetActive(false);
                GetReward(status, false);
            });

            foreach (Button button in _rankButton)
            {
                button.onClick.RemoveAllListeners();

                button.onClick.AddListener(() =>
                {
                    _window_GameOver.ButtonLeaveRoom();
                });
            }
        }
    }
}
