using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Knowlove
{
    public class SlotMachine : MonoBehaviourPun
    {
        [SerializeField] private GameObject playerNamePrefab;

        [SerializeField] private Transform container;

        [SerializeField] private RectTransform containerRect;

        [SerializeField] private CanvasGroup group;

        public float nameHeight = 100f;

        public int selectedPlayerIndex = 0;

        public void Init()
        {
            int selectedPlayer = Random.Range(0, NetworkManager.Instance.players.Count);
            photonView.RPC(nameof(RPC_StartSpinning), RpcTarget.All, selectedPlayer);
        }

        [PunRPC]
        public void RPC_StartSpinning(int selectedPlayerIndex)
        {
            this.selectedPlayerIndex = selectedPlayerIndex;
            int i = 0;
            foreach (Player player in NetworkManager.Instance.players)
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

        private IEnumerator DoStartSpinning()
        {
            bool finished = false;

            int currentIndex = 0;

            int rollCount = 0;

            bool finalize = false;
            float speed = 5f;

            while (!finished)
            {
                Vector2 targetPosition = finalize ? new Vector2(0f, Mathf.Round(containerRect.anchoredPosition.y / 100) * 100f) : new Vector2(0f, containerRect.anchoredPosition.y + 100f);
                containerRect.anchoredPosition = Vector2.Lerp(containerRect.anchoredPosition, targetPosition, speed * Time.deltaTime);

                if (finalize && speed > 6f)
                    speed -= Time.deltaTime;

                if (containerRect.anchoredPosition.y % 100f == 0 && !finalize)
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

                if (rollCount >= 100 && currentIndex == selectedPlayerIndex)
                    finalize = true;

                yield return null;
            }

            group.DOFade(0f, 1f);
        }
    }
}
