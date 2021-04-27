using Knowlove.UI;
using TMPro;
using UnityEngine;

namespace Knowlove.XPSystem
{
    public class ProgressRank : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerWithPeople;
        [SerializeField] private TextMeshProUGUI _gameWin;
        [SerializeField] private TextMeshProUGUI _datingCard;
        [SerializeField] private TextMeshProUGUI _relationshipCard;
        [SerializeField] private TextMeshProUGUI _marriageCard;       
        [SerializeField] private TextMeshProUGUI _shareApp;

        private void OnEnable()
        {
            CanvasLoading.Instance.Show();
            UpdateFildText();
        }

        private void UpdateFildText()
        {
            PlayerXP player = InfoPlayer.Instance.PlayerState;

            int needWin = 3;
            int needPeople = 5;
            int needShare = 3;

            if (player.isBronzeStatus && player.isSilverStatus)
            {
                needWin = 10;
                needShare = 7;
                needPeople = 15;
            }
            else if (player.isBronzeStatus)
            {
                needWin = 7;
                needShare = 5;
                needPeople = 10;
            }
            

            int currentDatingCard = GetCard(player.datingCard);
            int currentRelatishipCard = GetCard(player.relationshipCard);
            int currentMarriageCard = GetCard(player.marriagepCard);

            _playerWithPeople.text = player.countDifferentPlayers + " / " + needPeople;
            _gameWin.text = player.winGame + " / " + needWin;
            _shareApp.text = player.shareGame + " / " + needShare;

            _datingCard.text = currentDatingCard + " / " + player.datingCard.Length;
            _relationshipCard.text = currentRelatishipCard + " / " + player.relationshipCard.Length;
            _marriageCard.text = currentMarriageCard + " / " + player.marriagepCard.Length;            

            CanvasLoading.Instance.Hide();
        }

        private int GetCard(bool[] deckCard)
        {
            int count = 0;

            foreach (bool card in deckCard)
            {
                if (card)
                    count++;
            }

            return count;
        }
    }
}
