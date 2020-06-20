using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
