using DG.Tweening;
using GameBrewStudios;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI
{
    public class PlayerProgress : MonoBehaviourPun
    {
        public Player target;
        public Slider slider;

        public Image pawnIcon;

        public GameObject hostIcon;

        public Color[] playerColors;

        public GameObject[] avoidCards;

        public TurnManager turnManager;

        public TextMeshProUGUI playerNameLabel;

        int playerIndex = -1;

        bool initialized = false;

        float lastUpdate = 0f;

        private void Update()
        {
            if (target != null)
                UpdateProps(target);
        }

        private void OnDestroy()
        {
            TurnManager.OnReceivedPlayerProps -= this.TurnManager_OnReceivedPlayerProps;
            DOTween.Kill(this);
            DOTween.Kill(slider);
            DOTween.Kill(transform);
        }

        public void Init(Player player)
        {
            initialized = true;
            this.target = player;
            this.slider.SetValueWithoutNotify(0);
            playerNameLabel.text = player.NickName;

            int pIndex = NetworkManager.Instance.players.IndexOf(target);
            this.playerIndex = pIndex;
            if (playerIndex > -1)
                pawnIcon.color = playerColors[playerIndex];
            else
                pawnIcon.color = playerColors[0];

            bool isHost = (PhotonNetwork.CurrentRoom.MaxPlayers > 1 && PhotonNetwork.CurrentRoom.MasterClientId == this.target.ActorNumber);
            hostIcon.SetActive(isHost);

            TurnManager.OnReceivedPlayerProps += this.TurnManager_OnReceivedPlayerProps;

        }

        private void TurnManager_OnReceivedPlayerProps(Player player, ExitGames.Client.Photon.Hashtable changedProps)
        {
            //if (player.NickName == target.NickName)
            //{
            //    target = player;

            //}

            UpdateProps(player);
        }

        private void UpdateProps(Player player)
        {
            if (!initialized)
                return;

            if (playerIndex < 0 || player.ActorNumber != target.ActorNumber)
            {
                Debug.LogError("WHY IS PLAYER INDEX < 0??????");
                return;
            }

            //if(Time.time - lastUpdate >= 0.5f)
            //{
            //    lastUpdate = Time.time;
            ExitGames.Client.Photon.Hashtable playerProperties = target.CustomProperties;

            float progress = playerProperties.ContainsKey("progress") ? (float)playerProperties["progress"] : 0.1f;
            int avoidCardsCount = playerProperties.ContainsKey("avoidSingleCards") ? (int)playerProperties["avoidSingleCards"] : 0;

            DoUpdatePlayerProgress(progress, avoidCardsCount, turnManager.turnIndex);
            //}
        }

        public void DoUpdatePlayerProgress(float progress, int avoidCardsCount, int currentTurnIndex)
        {
            //Debug.Log("DoUpdatePlayerProgress playerIndex = " + playerIndex, gameObject);
            for (int i = 0; i < avoidCards.Length; i++)
            {
                avoidCards[i].SetActive(avoidCardsCount > i);
            }

            //Set the fill amount of the progress slider
            //DOTween.Kill(slider, true);
            //slider.DOValue(Mathf.Clamp(progress, 0.1f, 1f), 0.25f);
            slider.value = Mathf.Clamp(progress, 0.1f, 1f);
            //Set the slider color based on player index so it matches their gamepiece
            slider.fillRect.GetComponent<Image>().color = playerColors[playerIndex];

            pawnIcon.color = playerColors[playerIndex];

            //Set the scale so the current player is enlarged.
            if (playerIndex == currentTurnIndex)
            {
                DOTween.Kill(transform);
                transform.DOScale(1.2f, 0.5f);
                slider.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 48f);
            }
            else
            {
                DOTween.Kill(transform);
                transform.DOScale(1f, 0.35f);
                slider.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 24f);
            }
        }
    }
}