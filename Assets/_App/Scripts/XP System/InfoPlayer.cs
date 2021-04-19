using GameBrewStudios.Networking;
using Photon.Pun;
using UnityEngine;

namespace Knowlove.XPSystem 
{
    public class InfoPlayer : MonoBehaviourPunCallbacks
    {
        public static InfoPlayer Instance;

        [SerializeField] private PlayersState _playersState;

        private int _currentPlayer;

        private bool _isHaveUser = false;

        public PlayerXP PlayerState
        {
            get => _playersState.playerXPs[_currentPlayer];
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(this.gameObject);

            if (Instance == null)
                Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

        public void СheckAvailabilityPlayer()
        {
            if (_isHaveUser)
                return;
            else
                _isHaveUser = true;

            if (_playersState.playerXPs.Count == 0)
                CreateNewPlayer();
            else
            {
                APIManager.GetUserDetails((user) => 
                { 
                    for(int i = 0; i < _playersState.playerXPs.Count; i++)
                    {
                        if(_playersState.playerXPs[i].playerName == user.displayName)
                        {
                            _currentPlayer = i;
                            return;
                        }
                    }

                    CreateNewPlayer();
                });
            }                
        }

        public void CreateNewPlayer()
        {
            APIManager.GetUserDetails((user) =>
            {
                PlayerXP player = new PlayerXP();
                player.playerName = user.displayName;

                _playersState.playerXPs.Add(player);

                _currentPlayer = _playersState.playerXPs.Count - 1;
            });
        }

        public void MarkCard(int idCard)
        {
            if(idCard <= 35)
                _playersState.playerXPs[_currentPlayer].datingCard[idCard] = true;
            else if(idCard > 35 && idCard < 112)
                _playersState.playerXPs[_currentPlayer].datingCard[idCard - 36] = true;
            else if(idCard >= 112)
                _playersState.playerXPs[_currentPlayer].datingCard[idCard - 112] = true;
        }

        public void MarkAllCard()
        {
            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].datingCard.Length; i++)
                _playersState.playerXPs[_currentPlayer].datingCard[i] = true;

            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].relationshipCard.Length; i++)
                _playersState.playerXPs[_currentPlayer].relationshipCard[i] = true;

            for (int i = 0; i < _playersState.playerXPs[_currentPlayer].marriagepCard.Length; i++)
                _playersState.playerXPs[_currentPlayer].marriagepCard[i] = true;
        }

        public void PlayerWin()
        {
            _playersState.playerXPs[_currentPlayer].winGame += 1;
        }

        public void CheckPlayWithThisPlayers()
        {
            photonView.RPC(nameof(RPC_CheckPlayWithThisPlayers), RpcTarget.AllViaServer);
        }

        [PunRPC]
        private void RPC_CheckPlayWithThisPlayers()
        {
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
        }
    }
}

