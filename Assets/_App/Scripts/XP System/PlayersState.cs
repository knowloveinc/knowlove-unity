using System.Collections.Generic;
using UnityEngine;

namespace Knowlove.XPSystem
{
    [CreateAssetMenu(fileName = "PlayersState", menuName = "XP System", order = 1)]
    public class PlayersState : ScriptableObject
    {
        public List<PlayerXP> playerXPs = new List<PlayerXP>(); 
    }
}
