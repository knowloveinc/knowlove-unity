using UnityEngine.SceneManagement;
using GameBrewStudios.Networking;
using Photon.Pun;
using DG.Tweening;
using UnityEngine;
using Knowlove.UI;
using System;

namespace Knowlove.XPSystem 
{
    public class InfoPlayer : MonoBehaviourPunCallbacks
    {
        public static InfoPlayer Instance;

        private const string _playersStatePrefsName = "PlayersState";
        private const int _maxDifferentPlayers = 15;

        [SerializeField] public StatusPlayer statusPlayer;

        [SerializeField] private PlayersState _playersState;

        private int _currentPlayer;
        private bool _isHaveUser;

        public Action SettedPlayer;

        public PlayerXP PlayerState
        {
            get => _playersState.playerXPs[_currentPlayer];
            set => _playersState.playerXPs[_currentPlayer] = value;
        }

        private void Awake()
        {
            Instance = this;
            _isHaveUser = false;

            gameObject.AddComponent<PhotonView>();

            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                DOVirtual.DelayedCall(3f, () =>
                {
                    FromJSONPlayerInfo();
                });
            }              
        }

        private void OnDestroy()
        {
            PlayerPrefs.DeleteKey("IsSaveDate");
        }

        public void СheckAvailabilityPlayer()
        {
            bool isFinish = false;

            DOVirtual.DelayedCall(7f, () => 
            {
                if(!isFinish)
                    CanvasLoading.Instance.ForceHide();
            });

            if (_isHaveUser)
            {
                CanvasLoading.Instance.Hide();
                return;
            }                
            else
                _isHaveUser = true;            

            if (_playersState.playerXPs.Count == 0)
            {
                CreateNewPlayer();
                return;
            }

            APIManager.GetUserDetails((user) =>
            {
                for (int i = 0; i < _playersState.playerXPs.Count; i++)
                {
                    if (_playersState.playerXPs[i].playerName.ToLower() == user.displayName.ToLower())
                    {
                        _currentPlayer = i;
                        SettedPlayer?.Invoke();
                        isFinish = true;
                        CanvasLoading.Instance.Hide();
                        return;
                    }
                }

                CreateNewPlayer();
            });
        }

        public void CreateNewPlayer()
        {
            APIManager.GetUserDetails((user) =>
            {
                PlayerXP player = new PlayerXP();
                player.playerName = user.displayName;

                _playersState.playerXPs.Add(player);

                _currentPlayer = _playersState.playerXPs.Count - 1;

                JSONPlayerInfo();
                SettedPlayer?.Invoke();
                CanvasLoading.Instance.Hide();
            });           
        }

        public void MarkCard(int idCard)
        {
            if(idCard <= 35)
                PlayerState.playerDeckCard.datingCard[idCard] = true;
            else if(idCard > 35 && idCard < 112)
                PlayerState.playerDeckCard.relationshipCard[idCard - 36] = true;
            else if(idCard >= 112)
                PlayerState.playerDeckCard.marriagepCard[idCard - 112] = true;

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public void MarkAllCard()
        {
            for (int i = 0; i < PlayerState.playerDeckCard.datingCard.Length; i++)
            {
                if (!PlayerState.playerDeckCard.datingCard[i])
                {
                    PlayerState.playerDeckCard.datingCard[i] = true;
                    PlayerState.playerDeckCard.isBuyDatingCard[i] = true;
                }
            }
                
            for (int i = 0; i < PlayerState.playerDeckCard.relationshipCard.Length; i++)
            {
                if (!PlayerState.playerDeckCard.relationshipCard[i])
                {
                    PlayerState.playerDeckCard.relationshipCard[i] = true;
                    PlayerState.playerDeckCard.isBuyRelationshipCard[i] = true;
                }
            }
                
            for (int i = 0; i < PlayerState.playerDeckCard.marriagepCard.Length; i++)
            {
                if (!PlayerState.playerDeckCard.marriagepCard[i])
                {
                    PlayerState.playerDeckCard.marriagepCard[i] = true;
                    PlayerState.playerDeckCard.isBuyMarriagepCard[i] = true;
                }
            }                

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public bool CheckMarkAllCard()
        {
            for (int i = 0; i < PlayerState.playerDeckCard.datingCard.Length; i++)
            {
                if (PlayerState.playerDeckCard.datingCard[i] != true)
                    return false;
            }
                

            for (int i = 0; i < PlayerState.playerDeckCard.relationshipCard.Length; i++)
            {
                if (PlayerState.playerDeckCard.relationshipCard[i] != true)
                    return false;
            }
                

            for (int i = 0; i < PlayerState.playerDeckCard.marriagepCard.Length; i++)
            {
                if (PlayerState.playerDeckCard.marriagepCard[i] != true)
                    return false;
            }

            return true;
        }

        public void PlayerWin()
        {
            PlayerState.winGame += 1;
            PlayerState.completedGame += 1;

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public void PlayerEndGame()
        {
            PlayerState.completedGame += 1;

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public void PlayerShareApp()
        {
            PlayerState.shareGame += 1;

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public void CheckPlayWithThisPlayers()
        {
            photonView.RPC(nameof(RPC_CheckPlayWithThisPlayers), RpcTarget.AllViaServer);
        }
        
        [PunRPC]
        private void RPC_CheckPlayWithThisPlayers()
        {
            if (PlayerState.countDifferentPlayers >= _maxDifferentPlayers)
                return;

            for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
            {
                bool isHave = true;

                for (int j = 0; j <= PlayerState.countDifferentPlayers; j++)
                {
                    if (NetworkManager.Instance.players[i].NickName == PlayerState.nameDifferentPlayers[j])
                        isHave = false;
                }

                if (isHave && NetworkManager.Instance.players[i].NickName != PhotonNetwork.NickName)
                {
                    PlayerState.nameDifferentPlayers[PlayerState.countDifferentPlayers] = NetworkManager.Instance.players[i].NickName;
                    PlayerState.countDifferentPlayers += 1;                   
                }
            }

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public void JSONPlayerInfo()
        {
            string info = JsonUtility.ToJson(_playersState);

            PlayerPrefs.SetString(_playersStatePrefsName, info);
        }

        public void FromJSONPlayerInfo()
        {
            if (PlayerPrefs.HasKey(_playersStatePrefsName))
            {
                string info = PlayerPrefs.GetString(_playersStatePrefsName);
                JsonUtility.FromJsonOverwrite(info, _playersState);

                DOVirtual.DelayedCall(1f, () =>
                {
                    //CanvasLoading.Instance.Hide();
                    СheckAvailabilityPlayer();
                });              
            }
            else
                СheckAvailabilityPlayer();
        }
    }
}

