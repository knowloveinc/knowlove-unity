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
                        
            int currentDatingCard = GetCard(player.playerDeckCard.datingCard);
            int currentRelatishipCard = GetCard(player.playerDeckCard.relationshipCard);
            int currentMarriageCard = GetCard(player.playerDeckCard.marriagepCard);

            int currentPlayer = (player.countDifferentPlayers > needPeople) ? needPeople : player.countDifferentPlayers;
            int currentWin = (player.winGame > needWin) ? needWin : player.winGame;
            int currentShare = (player.shareGame > needShare) ? needShare : player.shareGame;

            if (!player.isGoldStatus)
            {
                _playerWithPeople.text = currentPlayer + " / " + needPeople;
                _gameWin.text = currentWin + " / " + needWin;
                _shareApp.text = currentShare + " / " + needShare;
            }
            else
            {
                _playerWithPeople.text = player.countDifferentPlayers.ToString();
                _gameWin.text = player.winGame.ToString();
                _shareApp.text = player.shareGame.ToString();
            }
                

            _datingCard.text = currentDatingCard + " / " + player.playerDeckCard.datingCard.Length;
            _relationshipCard.text = currentRelatishipCard + " / " + player.playerDeckCard.relationshipCard.Length;
            _marriageCard.text = currentMarriageCard + " / " + player.playerDeckCard.marriagepCard.Length;            

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
