using GameBrewStudios;
using Knowlove.UI.Menus;
using UnityEngine;
using UnityEngine.Audio;

namespace Knowlove.UI
{
    public class StartupCanvas : MonoBehaviour
    {
        [SerializeField] private Window pickModeWindow, loginWindow, settingsWindow, createMatchWindow, matchListWindow, waitingForPlayersWindow;

        [SerializeField] private AudioMixer mixer;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            matchListWindow.Hide();
            settingsWindow.Hide();
            createMatchWindow.Hide();
            waitingForPlayersWindow.Hide();

            mixer.SetFloat("MasterVolume", Mathf.Log(GameSettings.Volume) * 20f);

            if (User.current == null || string.IsNullOrEmpty(User.current._id))
            {
                pickModeWindow.Hide();
                loginWindow.Show();
            }
            else
            {
                loginWindow.Hide();
                pickModeWindow.Show();
            }
        }

        public void QuitApplication()
        {
            PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
            {
                new PopupDialog.PopupButton()
                {
                    text = "Yes, Quit",
                    onClicked = () =>
                    {
                        Application.Quit();
                    },
                    buttonColor = PopupDialog.PopupButtonColor.Red
                },
                new PopupDialog.PopupButton()
                {
                    text = "Cancel",
                    onClicked = () => { },
                    buttonColor = PopupDialog.PopupButtonColor.Plain
                }
            };

            PopupDialog.Instance.Show("Close Know Love", "Are you sure you want to exit Know Love?", buttons);
        }
    }
}
