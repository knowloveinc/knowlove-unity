using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBrewStudios
{
    public class Window_Settings : Window
    {
        const string url = "http://knowloveapp.com/help-us-grow";

        public void OnClickedDonate()
        {
            Application.OpenURL(url);
        }
    }
}