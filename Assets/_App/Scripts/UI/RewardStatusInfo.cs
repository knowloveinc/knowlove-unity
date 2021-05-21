using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Knowlove.UI
{
    public class RewardStatusInfo : MonoBehaviour
    {
        [SerializeField] private GameObject[] _rankInfo;

        public void ShowRankPopup(string rank)
        {
            switch (rank)
            {
                case "bronze":
                    _rankInfo[0].SetActive(true);
                    _rankInfo[1].SetActive(false);
                    _rankInfo[2].SetActive(false);
                    break;
                case "silver":
                    _rankInfo[0].SetActive(false);
                    _rankInfo[1].SetActive(true);
                    _rankInfo[2].SetActive(false);
                    break;
                case "gold":
                    _rankInfo[0].SetActive(false);
                    _rankInfo[1].SetActive(false);
                    _rankInfo[2].SetActive(true);
                    break;
            }

            gameObject.SetActive(true);
        }
    }
}
