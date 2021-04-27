using GameBrewStudios;
using TMPro;
using UnityEngine;

namespace Knowlove.UI
{
    public class PlayerWalletText : MonoBehaviour
    {
        private TextMeshProUGUI walletText;

        // Start is called before the first frame update
        void Start()
        {
            walletText = GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if(walletText != null && User.current != null)
                walletText.text = User.current.wallet.ToString("n0") + " <sprite=0>";
        }
    }
}
