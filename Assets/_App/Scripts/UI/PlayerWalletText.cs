using GameBrewStudios;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerWalletText : MonoBehaviour
{
    TextMeshProUGUI walletText;

    // Start is called before the first frame update
    void Start()
    {
        walletText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        walletText.text = User.current.wallet.ToString("n0") + " <sprite=0>";
    }
}
