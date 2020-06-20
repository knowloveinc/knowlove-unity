using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Photon.Pun;

public class Window_GameOver : Window
{
    string winnerName = "";

    [SerializeField]
    TextMeshProUGUI winnerNameLabel;

    [SerializeField]
    CanvasGroup[] groups;

    [SerializeField]
    GameObject hud;
    private void OnEnable()
    {
        hud.SetActive(false);
    }

    private void OnDisable()
    {
        hud.SetActive(true);
    }
    public void Init(string winnerName)
    {
        this.winnerName = winnerName;
    }

    public override void Show()
    {
        MusicManager.Instance.PlaySong(1);
        
        CameraManager.Instance.RPC_SetCamera(3);
        base.Show();

        winnerNameLabel.text = this.winnerName;
        for(int i = 0; i < groups.Length; i++)
        {
            groups[i].alpha = 0;
            groups[i].DOFade(1f, 2f).SetDelay(i * 1f);
        }


    }
}
