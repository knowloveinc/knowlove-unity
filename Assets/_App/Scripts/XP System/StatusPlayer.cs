using GameBrewStudios;
using GameBrewStudios.Networking;
using System;
using UnityEngine;

namespace Knowlove.XPSystem
{
    public class StatusPlayer : MonoBehaviour
    {
        private const string idAvoidSingleCard = "avoidSingle";

        public Action ChangedPlayerStatus;
        public Action<int> RewardedPlayer;

        public void CheckPlayerStatus()
        {
            CheckBronze();
            CheckSilver();
            CheckGold();
        }

        private void CheckBronze()
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isBronzeStatus)
                return;

            if (playerXP.countDifferentPlayers < 5 && playerXP.winGame < 3 /*|| playerXP.shareGame < 3*/)
                return;

            for (int i = 0; i < playerXP.datingCard.Length; i++)
            {
                if (!playerXP.datingCard[i])
                    return;
            }

            InfoPlayer.Instance.PlayerState.isBronzeStatus = true;

            GetReward("bronze");
        }

        private void CheckSilver()
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isSilverStatus)
                return;

            if (playerXP.countDifferentPlayers < 10 && playerXP.winGame < 7 /*|| playerXP.shareGame < 5*/)
                return;

            if (!playerXP.isBronzeStatus)
                return;

            for (int i = 0; i < playerXP.relationshipCard.Length; i++)
            {
                if (!playerXP.relationshipCard[i])
                    return;
            }

            InfoPlayer.Instance.PlayerState.isSilverStatus = true;

            GetReward("silver");
        }

        private void CheckGold()
        {
            PlayerXP playerXP = InfoPlayer.Instance.PlayerState;

            if (playerXP.isSilverStatus)
                return;

            if (playerXP.countDifferentPlayers < 15 && playerXP.winGame < 10 /*|| playerXP.shareGame < 7*/)
                return;

            if (!playerXP.isBronzeStatus && !playerXP.isSilverStatus)
                return;

            for (int i = 0; i < playerXP.marriagepCard.Length; i++)
            {
                if (!playerXP.marriagepCard[i])
                    return;
            }

            InfoPlayer.Instance.PlayerState.isSilverStatus = true;

            GetReward("gold");
        }

        private void GetReward(string status)
        {
            switch (status)
            {
                case "bronze":
                    AddCardFromServer(1);
                    InfoPlayer.Instance.PlayerState.isBronzeStatus = true;
                    RewardedPlayer?.Invoke(0);
                    break;
                case "silver":
                    AddCardFromServer(2);
                    InfoPlayer.Instance.PlayerState.ProtectedFromBackToSingleInMarriagePerGame = true;
                    InfoPlayer.Instance.PlayerState.isSilverStatus = true;
                    RewardedPlayer?.Invoke(1);
                    break;
                case "gold":
                    AddCardFromServer(3);
                    InfoPlayer.Instance.PlayerState.ProtectedFromBackToSinglePerGame = true;
                    InfoPlayer.Instance.PlayerState.isGoldStatus = true;
                    RewardedPlayer?.Invoke(2);
                    break;
                default:
                    break;
            }

            ChangedPlayerStatus?.Invoke();
        }

        private void AddCardFromServer(int amound)
        {
            APIManager.GetUserDetails((user) =>
            {
                APIManager.AddItem(idAvoidSingleCard, amound, (inventory) =>
                {
                    User.current.inventory = inventory;
                });
            });
        }

        [ContextMenu("Do Bronze")]
        private void DoBronze()
        {
            GetReward("bronze");
        }

        [ContextMenu("Do Gold")]
        private void DoGold()
        {
            GetReward("gold");
        }
    }
}
