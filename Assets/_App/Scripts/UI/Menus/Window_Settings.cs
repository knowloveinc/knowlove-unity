using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBrewStudios
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