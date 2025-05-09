﻿using DG.Tweening;
using GameBrewStudios;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviourPun
{
    [SerializeField]
    GameObject playerNamePrefab;

    [SerializeField]
    Transform container;

    [SerializeField]
    RectTransform containerRect;

    public float nameHeight = 100f;

    public int selectedPlayerIndex = 0;

    [SerializeField]
    CanvasGroup group;

    public void Init()
    {
        int selectedPlayer = Random.Range(0, NetworkManager.Instance.players.Count);
        photonView.RPC("RPC_StartSpinning", RpcTarget.All, selectedPlayer);
    }

    [PunRPC]
    public void RPC_StartSpinning(int selectedPlayerIndex)
    {
        this.selectedPlayerIndex = selectedPlayerIndex;
        int i = 0;
        foreach(Player player in NetworkManager.Instance.players)
        {
            GameObject obj = Instantiate(playerNamePrefab, container);
            TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
            label.text = player.NickName;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, i * -nameHeight);

            i++;
        }

        group.DOFade(1f, 1f);

        StartCoroutine(DoStartSpinning());
    }

    IEnumerator DoStartSpinning()
    {
        bool finished = false;

        int currentIndex = 0;

        int rollCount = 0;

        bool finalize = false;
        float speed = 5f;

        while (!finished)
        {
            Vector2 targetPosition = finalize ? new Vector2(0f, Mathf.Round(containerRect.anchoredPosition.y /100) * 100f) : new Vector2(0f, containerRect.anchoredPosition.y + 100f);
            containerRect.anchoredPosition = Vector2.Lerp(containerRect.anchoredPosition, targetPosition, speed * Time.deltaTime);

            if(finalize && speed > 6f)
            {
                speed -= Time.deltaTime;
            }

            if(containerRect.anchoredPosition.y % 100f == 0 && !finalize)
            {
                //At a multiple of 100, move the top sibling to the bottom;
                RectTransform top = containerRect.GetChild(0).GetComponent<RectTransform>();
                top.anchoredPosition = new Vector2(0f, top.anchoredPosition.y - (100f * NetworkManager.Instance.players.Count));
                top.SetAsLastSibling();
                
                currentIndex++;
                if (currentIndex >= NetworkManager.Instance.players.Count)
                    currentIndex = 0;

                rollCount++;
            }

            if(rollCount >= 100)
            {
                if(currentIndex == selectedPlayerIndex)
                {
                    finalize = true;
                }

            }

            yield return null;
        }

        group.DOFade(0f, 1f);
    }

}
