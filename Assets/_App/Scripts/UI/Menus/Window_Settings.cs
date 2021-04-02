using UnityEngine;

namespace Knowlove.UI.Menus
{
    public class Window_Settings : Window
    {
        const string url = "https://knowloveinc.com/our-app/";

        public void OnClickedDonate()
        {
            Application.OpenURL(url);
        }
    }
}