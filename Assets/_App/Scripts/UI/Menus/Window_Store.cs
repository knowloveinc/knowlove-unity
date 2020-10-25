using GameBrewStudios;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Store : Window
{
    [SerializeField]
    ScrollRect scrollRect;


    public override void Show()
    {
        if (User.current == null) return;

        scrollRect.content.transform.localPosition = new Vector2(0f, 0f);
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    
}
