using DG.Tweening;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Knowlove.UI
{
    public class SlotPanel : MonoBehaviourPunCallbacks
    {
        [SerializeField] private SlotMachine _slotMachine;
        [SerializeField] private TextMeshProUGUI playerSlotText;
        [SerializeField] private ReadyPlayers _readyPlayers;

        private void Start()
        {
            _readyPlayers.RPC_PlayerReadied += ShowPlayerSelection;
        }

        private void OnDestroy()
        {
            _readyPlayers.RPC_PlayerReadied -= ShowPlayerSelection;
        }

        public void ShowPlayerSelection(string[] playerNames)
        {
            Debug.Log("SENDING SHOW SLOTS MESSAGE");
            //slotMachine.Init();
            photonView.RPC(nameof(RPC_ShowPlayerSelection), RpcTarget.All, playerNames, 0);
        }

        [PunRPC]
        private void RPC_ShowPlayerSelection(string[] playerNames, int notUsed)
        {
            Debug.Log("SHOWING SLOTS" + playerNames.Length);

            StartCoroutine(DOShowPlayerNames(playerNames));
        }

        private IEnumerator DOShowPlayerNames(string[] playerNames)
        {
            Debug.Log("SHOW SLOTS COROUTINE");
            float delay = 0.001f;
            int j = 0;

            CanvasGroup group = playerSlotText.GetComponent<CanvasGroup>();
            group.DOFade(1f, 0.1f);

            if (PhotonNetwork.CurrentRoom.MaxPlayers > 1)
            {
                for (int i = 0; i < 151; i++)
                {
                    Debug.Log(playerNames.Length);

                    playerSlotText.text = playerNames[j];
                    j++;

                    if (j >= playerNames.Length)
                        j = 0;

                    if (i > 100)
                        delay += 0.001f;

                    yield return new WaitForSeconds(delay);
                }

                group.DOFade(0f, 0.5f).SetDelay(2f);
            }
            else
            {
                playerSlotText.text = "Get Ready!";
                playerSlotText.transform.GetChild(0).gameObject.SetActive(false);
                group.DOFade(0f, 0.5f).SetDelay(2f);
            }

            yield return new WaitForSeconds(2f);

            if (PhotonNetwork.IsMasterClient)
                TurnManager.Instance.ReallyStartGame();
        }
    }
}
