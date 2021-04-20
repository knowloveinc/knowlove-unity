using GameBrewStudios;
using GameBrewStudios.Networking;
using Knowlove.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove.XPSystem
{
    public class StatusPlayer : MonoBehaviour
    {
        private const string idAvoidSingleCard = "avoidSingle";

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

            if (playerXP.countDifferentPlayers <= 5 || playerXP.winGame <= 3 || playerXP.shareGame <= 3)
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

            if (playerXP.countDifferentPlayers <= 10 || playerXP.winGame <= 7 || playerXP.shareGame <= 5)
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

            if (playerXP.countDifferentPlayers <= 15 || playerXP.winGame <= 10 || playerXP.shareGame <= 7)
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
            string text = "";
            string title = "Сongratulate!!!";

            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
                new PopupDialog.PopupButton()
                {
                    text = "Okey",
                    buttonColor = PopupDialog.PopupButtonColor.Plain,
                    onClicked = () =>{ }
                }
            };

            switch (status)
            {
                case "bronze":
                    AddCardFromServer(1);
                    title += " You get Bronze status";
                    text += "You have reached Bronze status and received one \"Avoid To Single\" card.";
                    break;
                case "silver":
                    AddCardFromServer(2);
                    InfoPlayer.Instance.PlayerState.ProtectedFromBackToSingleInMarriagePerGame = true;
                    title += " You get Silver status";
                    text += "You have reached Silver status and received two \"Avoid To Single\" card. \n Cheating Landing spaces in Marriage Phase deactivated per game.";
                    break;
                case "gold":
                    AddCardFromServer(3);
                    InfoPlayer.Instance.PlayerState.ProtectedFromBackToSinglePerGame = true;
                    title += " You get Gold status";
                    text += "You have reached Silver status and received three \"Avoid To Single\" card. \n All Cheating Landing spaces deactivated per game.";
                    break;
                default:
                    break;
            }

            PopupDialog.Instance.Show(title, text, buttons);
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
    }
}
