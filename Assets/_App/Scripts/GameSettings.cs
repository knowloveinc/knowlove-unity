using UnityEngine;

namespace Knowlove
{
    public class GameSettings : MonoBehaviour
    {
        public static float Volume
        {
            get
            {
                return PlayerPrefs.GetFloat("Volume", 1f);
            }
            set
            {
                PlayerPrefs.SetFloat("Volume", value);
            }
        }
    }
}

