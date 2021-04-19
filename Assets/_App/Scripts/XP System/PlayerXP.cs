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
        public string[] nameDifferentPlayers = new string[15];
        public bool[] datingCard = new bool[36];
        public bool[] relationshipCard = new bool[76];
        public bool[] marriagepCard = new bool[54];
    }
}
