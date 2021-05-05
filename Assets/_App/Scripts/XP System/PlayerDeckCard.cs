using System;


namespace Knowlove.XPSystem
{
    [Serializable]
    public class PlayerDeckCard
    {
        public bool[] datingCard = new bool[36];
        public bool[] relationshipCard = new bool[76];
        public bool[] marriagepCard = new bool[54];

        public bool[] isBuyDatingCard = new bool[36];
        public bool[] isBuyRelationshipCard = new bool[76];
        public bool[] isBuyMarriagepCard = new bool[54];
    }
}
