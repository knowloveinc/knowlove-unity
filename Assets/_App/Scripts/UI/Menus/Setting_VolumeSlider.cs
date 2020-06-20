using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Setting_VolumeSlider : MonoBehaviour
{
    [SerializeField]
    Slider slider;

    [SerializeField]
    AudioMixer mixer;

    private void OnEnable()
    {
        Debug.Log("Volume slider updated.");
        slider.value = Mathf.Clamp(GameSettings.Volume, 0f, 1f);
    }

    public void OnChanged(float val)
    {
        GameSettings.Volume = val;
        mixer.SetFloat("MasterVolume", Mathf.Log(val) * 20);
    }
}
