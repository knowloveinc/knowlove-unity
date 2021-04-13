using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Knowlove.UI.Menus
{
    public class Setting_VolumeSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;

        [SerializeField] private AudioMixer mixer;

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
}