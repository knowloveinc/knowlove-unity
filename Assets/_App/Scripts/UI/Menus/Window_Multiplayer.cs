using UnityEngine;

namespace Knowlove.UI.Menus
{
    public class Window_Multiplayer : Window
    {
        [SerializeField] private Window_MatchList matchListWindow;

        [SerializeField] private Window_CreateMatchWizard createMatchWindow;

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void OnFindMatch()
        {
            this.Hide();
            matchListWindow.Show();
        }

        public void OnCreateMatch()
        {
            this.Hide();
            StartWizard();
        }

        public void StartWizard()
        {
            createMatchWindow.Show();
        }
    }
}
