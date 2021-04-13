using DG.Tweening;
using Knowlove.FlipTheTableLogic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Knowlove.UI
{
    public class GameUI : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TurnManager TurnManager;
        [SerializeField] private FlipTheTable _flipTheTable;
        [SerializeField] private ReadyPlayers _readyPlayers;

        [SerializeField] private BottomPanel _bottomPanel;
        [SerializeField] private StatsPanel statsPanel;

        [SerializeField] private TextMeshProUGUI _timerText;

        [SerializeField] private Transform _cardPickerPanel;
        [SerializeField] private RectTransform _listCardContainer;
        [SerializeField] private RectTransform[] _listCardUIObjs;

        [SerializeField] private TextMeshProUGUI _listText;
        [SerializeField] private GameObject _listCardButton;
        [SerializeField] private GameObject _listPanel;

        public ListCard[] listCards;
        public ListCard selectedCard;

        private bool _didReadyUp = false;
        private bool _showBottomAfterClosingListPanel = false;

        private void Start()
        {
            _flipTheTable.StartedFlipTable += SetActiveObject;

            TurnManager.SettedPlayerProperties += SetStats;
            TurnManager.DatingOrHomePathRing += ForceShowListPanel;

            if (PhotonNetwork.IsMasterClient)
            {
                NetworkManager.OnReadyToStart += ShowUserPickCard;
            }
        }

        private void Update()
        {
            SetStats();
            UpdateTimerText();
        }

        private void OnDestroy()
        {
            _flipTheTable.StartedFlipTable -= SetActiveObject;

            TurnManager.SettedPlayerProperties -= SetStats;
            TurnManager.DatingOrHomePathRing -= ForceShowListPanel;

            if (PhotonNetwork.IsMasterClient)
                NetworkManager.OnReadyToStart -= ShowUserPickCard;
        }

        public void SetActiveObject(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        internal void ShowPickCard()
        {
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC(nameof(RPC_ShowPickCard), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_ShowPickCard()
        {
            CanvasLoading.Instance.ForceHide();
            selectedCard = listCards[UnityEngine.Random.Range(0, listCards.Length)];

            ShowCardPickerPanel();
        }

        public void ShowCardPickerPanel()
        {
            //put the container below the screen and swipe it up into view.
            _listCardContainer.anchoredPosition = new Vector3(0f, -1080f, 0f);
            _listCardContainer.DOAnchorPosY(0f, 0.5f);

            _cardPickerPanel.gameObject.SetActive(true);

            for (int i = 0; i < _listCardUIObjs.Length; i++)
            {
                _listCardUIObjs[i].GetComponent<CanvasGroup>().alpha = 1f;
            }
        }

        public void OnListCardSelected(int index)
        {
            _listText.text = selectedCard.text.Replace("\\n", "\n");

            for (int i = 0; i < _listCardUIObjs.Length; i++)
            {
                if (i != index)
                {
                    _listCardUIObjs[i].DOAnchorPosY(-1080f, 0.5f);
                    _listCardUIObjs[i].DOScale(0f, 0.2f);
                }
                else
                {
                    _listCardUIObjs[i].DOAnchorPos(new Vector2(0f, 0f), 0.2f);
                    _listCardUIObjs[i].DORotate(new Vector3(0f, 0f, 0f), 0.2f);
                    _listCardUIObjs[i].DOScale(1.5f, 0.2f);
                    _listCardUIObjs[i].GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetDelay(1f).OnComplete(() =>
                    {
                        _listCardButton.SetActive(true);

                        DOVirtual.DelayedCall(0.3f, () =>
                        {
                            _didReadyUp = false;
                            ShowListPanel(true);
                            _cardPickerPanel.gameObject.SetActive(false);
                        });
                    });
                }
            }
        }

        public void ForceShowListPanel(Player player)
        {
            photonView.RPC(nameof(RPC_ForceShowListPanel), player);
        }

        [PunRPC]
        public void RPC_ForceShowListPanel()
        {
            _showBottomAfterClosingListPanel = true;
            ShowListPanel(true);
        }

        public void ShowListPanel()
        {
            ShowListPanel(true);
        }

        public void ShowListPanel(bool showHelper = false)
        {
            _listPanel.gameObject.SetActive(true);
            _listPanel.transform.Find("helper").gameObject.SetActive(showHelper);
        }

        public void HideListPanel()
        {
            Debug.Log("<color=Red>HideListPanel()</color>");

            _listPanel.gameObject.SetActive(false);

            if (!_didReadyUp)
            {
                _didReadyUp = true;
                _readyPlayers.ReadyUp();
            }

            if (_showBottomAfterClosingListPanel)
            {
                _showBottomAfterClosingListPanel = false;
                int index = NetworkManager.Instance.players.IndexOf(PhotonNetwork.LocalPlayer);
                Debug.Log("<color=Cyan>Current Player Index = " + index + "</color>");
                _bottomPanel.ShowBottomForPlayer(index);
            }
        }

        public void ExitGame()
        {
            PhotonNetwork.LeaveRoom();
        }

        internal void SetStats()
        {
            if (PhotonNetwork.LocalPlayer == null || PhotonNetwork.LocalPlayer.CustomProperties == null || PhotonNetwork.LocalPlayer.CustomProperties.Count < 1)
                return;

            string text = "";

            int dateCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["dateCount"];
            int relationshipCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["relationshipCount"];
            int marriageCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["marriageCount"];
            int yearsElapsed = (int)PhotonNetwork.LocalPlayer.CustomProperties["yearsElapsed"];

            text = $"# Dates: {dateCount}\n# Relationships: {relationshipCount}\n # Marriages: {marriageCount}\n # Years Elapsed: {yearsElapsed}";

            statsPanel.SetText(text);
        }

        private void UpdateTimerText()
        {
            if (TurnManager.Instance.turnState != TurnManager.TurnState.GameOver && TurnManager.Instance.turnState != TurnManager.TurnState.TurnEnding)
            {
                if (TurnManager.Instance.turnTimer <= 0f || TurnManager.Instance.turnTimer > 30f)
                    _timerText.text = "";
                else
                    _timerText.text = TurnManager.Instance.turnTimer.ToString("n0");
            }
            else
                _timerText.text = "";
        }

        private void ShowUserPickCard()
        {
            DOVirtual.DelayedCall(2f, () =>
            {
                CameraManager.Instance.SetCamera(2);

                ShowPickCard();
            });
        }
    }
}