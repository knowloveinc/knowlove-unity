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
                DOVirtual.DelayedCall(4f, () =>
                {
                    FromJSONPlayerInfo();
                });
            }              
        }

        private void OnApplicationQuit()
        {
            JSONPlayerInfo();
        }

        private void OnDestroy()
        {
            JSONPlayerInfo();
            PlayerPrefs.DeleteKey("IsSaveDate");
        }

        public void СheckAvailabilityPlayer()
        {
            if (_isHaveUser)
            {
                CanvasLoading.Instance.Hide();
                return;
            }                
            else
                _isHaveUser = true;

            PlayerPrefs.SetInt("IsSaveDate", 0);

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
                _playersState.playerXPs[_currentPlayer].datingCard[idCard] = true;
            else if(idCard > 35 && idCard < 112)
                _playersState.playerXPs[_currentPlayer].relationshipCard[idCard - 36] = true;
            else if(idCard >= 112)
                _playersState.playerXPs[_currentPlayer].marriagepCard[idCard - 112] = true;

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public void MarkAllCard()
        {
            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].datingCard.Length; i++)
                _playersState.playerXPs[_currentPlayer].datingCard[i] = true;

            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].relationshipCard.Length; i++)
                _playersState.playerXPs[_currentPlayer].relationshipCard[i] = true;

            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].marriagepCard.Length; i++)
                _playersState.playerXPs[_currentPlayer].marriagepCard[i] = true;

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public bool CheckMarkAllCard()
        {
            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].datingCard.Length; i++)
            {
                if (_playersState.playerXPs[_currentPlayer].datingCard[i] != true)
                    return false;
            }
                

            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].relationshipCard.Length; i++)
            {
                if (_playersState.playerXPs[_currentPlayer].relationshipCard[i] != true)
                    return false;
            }
                

            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].marriagepCard.Length; i++)
            {
                if (_playersState.playerXPs[_currentPlayer].marriagepCard[i] != true)
                    return false;
            }

            return true;
        }

        public void PlayerWin()
        {
            _playersState.playerXPs[_currentPlayer].winGame += 1;
            _playersState.playerXPs[_currentPlayer].completedGame += 1;

            statusPlayer.CheckPlayerStatus();
            JSONPlayerInfo();
        }

        public void PlayerEndGame()
        {
            _playersState.playerXPs[_currentPlayer].completedGame += 1;

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
            if (_playersState.playerXPs[_currentPlayer].countDifferentPlayers >= _maxDifferentPlayers)
                return;

            for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
            {
                bool isHave = true;

                for (int j = 0; j <= _playersState.playerXPs[_currentPlayer].countDifferentPlayers; j++)
                {
                    if (NetworkManager.Instance.players[i].NickName == _playersState.playerXPs[_currentPlayer].nameDifferentPlayers[j])
                        isHave = false;
                }

                if (isHave && NetworkManager.Instance.players[i].NickName != PhotonNetwork.NickName)
                {
                    _playersState.playerXPs[_currentPlayer].nameDifferentPlayers[_playersState.playerXPs[_currentPlayer].countDifferentPlayers] = NetworkManager.Instance.players[i].NickName;
                    _playersState.playerXPs[_currentPlayer].countDifferentPlayers += 1;                   
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

                DOVirtual.DelayedCall(1.5f, () =>
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

