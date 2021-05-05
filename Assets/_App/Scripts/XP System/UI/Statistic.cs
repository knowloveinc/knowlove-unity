using Knowlove.UI;
using TMPro;
using UnityEngine;

namespace Knowlove.XPSystem
{
    public class Statistic : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _datingCard;
        [SerializeField] private TextMeshProUGUI _relationshipCard;
        [SerializeField] private TextMeshProUGUI _marriageCard;
        [SerializeField] private TextMeshProUGUI _gameWin;
        [SerializeField] private TextMeshProUGUI _gameComplete;

        private void OnEnable()
        {
            CanvasLoading.Instance.Show();
            UpdateFildText();
        }

        private void UpdateFildText()
        {
            PlayerXP player = InfoPlayer.Instance.PlayerState;

            int currentDatingCard = GetCard(player.playerDeckCard.datingCard);
            int currentRelatishipCard = GetCard(player.playerDeckCard.relationshipCard);
            int currentMarriageCard = GetCard(player.playerDeckCard.marriagepCard);

            _datingCard.text = currentDatingCard + " / " + player.playerDeckCard.datingCard.Length;
            _relationshipCard.text = currentRelatishipCard + " / " + player.playerDeckCard.relationshipCard.Length;
            _marriageCard.text = currentMarriageCard + " / " + player.playerDeckCard.marriagepCard.Length;
            _gameWin.text = player.winGame.ToString();
            _gameComplete.text = player.completedGame.ToString();

            CanvasLoading.Instance.Hide();
        }

        private int GetCard(bool[] deckCard)
        {
            int count = 0;

            foreach(bool card in deckCard)
            {
                if (card)
                    count++;
            }

            return count;
        }
    }
}
