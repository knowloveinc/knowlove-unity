using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Knowlove.UI
{
    public class ProgressBar : MonoBehaviourPunCallbacks
    {
        [SerializeField] private ReadyPlayers _readyPlayers;
        [SerializeField] private AvoidSingleCard _avoidSingleCard;

        [SerializeField] private GameObject playerProgressTemplate;
        [SerializeField] private Transform playerProgressContainer;

        private void Start()
        {
            playerProgressTemplate.SetActive(false);
            _readyPlayers.PlayerReadied += BuildProgressBarList;
        }

        private void OnDestroy()
        {
            _readyPlayers.PlayerReadied -= BuildProgressBarList;
        }

        public void BuildProgressBarList()
        {
            photonView.RPC(nameof(RPC_BuildProgressBarList), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_BuildProgressBarList()
        {
            //Destroy everything but the template for a clean slate.
            foreach (Transform child in playerProgressContainer)
            {
                if (child != playerProgressTemplate.transform)
                    Destroy(child.gameObject);
            }

            for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
            {
                GameObject obj = Instantiate(playerProgressTemplate, playerProgressContainer);
                obj.SetActive(true);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = NetworkManager.Instance.players[i].NickName;

                PlayerProgress p = obj.GetComponent<PlayerProgress>();
                p.Init(NetworkManager.Instance.players[i]);
            }

            _avoidSingleCard.SetPlayerBar();
        }
    }
}
