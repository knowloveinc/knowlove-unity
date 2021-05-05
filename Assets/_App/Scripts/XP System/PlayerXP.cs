using System;

namespace Knowlove.XPSystem
{
    [Serializable]
    public class PlayerXP
    {
        public string playerName;
        public int winGame;
        public int completedGame;
        public int shareGame;
        public int countDifferentPlayers;
        public bool isBronzeStatus;
        public bool isSilverStatus;
        public bool isGoldStatus;
        public bool ProtectedFromBackToSinglePerGame;
        public bool ProtectedFromBackToSingleInMarriagePerGame;
        public string[] nameDifferentPlayers = new string[15];

        public PlayerDeckCard playerDeckCard = new PlayerDeckCard();
    }
}
