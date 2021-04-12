using Knowlove.UI;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove
{
    public class ReadyPlayers : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TurnManager _turnManager;

        private int _playersReady = 0;

        public Action PlayerReadied;
        public Action<string[]> RPC_PlayerReadied;

        private void Start()
        {
            _playersReady = 0;
        }

        public void ReadyUp()
        {
            PlayerReadied?.Invoke();
            photonView.RPC(nameof(RPC_ReadyUp), RpcTarget.All);
        }

        [PunRPC]
        public void RPC_ReadyUp()
        {
            if (PhotonNetwork.IsMasterClient && _playersReady < PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                _playersReady++;

                if (_playersReady >= PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //if(players == null || players.Count == 0)
                    //{
                    //    players = NetworkManager.Instance.players;
                    //}

                    int firstPlayerIndex = UnityEngine.Random.Range(0, NetworkManager.Instance.players.Count);
                    _turnManager.turnIndex = firstPlayerIndex;

                    List<string> playerNames = new List<string>();
                    int j = firstPlayerIndex;

                    Debug.Log("First player index = " + firstPlayerIndex);
                    Debug.Log("Player Count: " + NetworkManager.Instance.players.Count);
                    for (int i = 0; i < NetworkManager.Instance.players.Count; i++)
                    {

                        Debug.Log("Adding " + NetworkManager.Instance.players[j].NickName);
                        playerNames.Add(NetworkManager.Instance.players[j].NickName);
                        j++;
                        if (j >= NetworkManager.Instance.players.Count)
                            j = 0;
                    }

                    Debug.Log("Sending " + playerNames.Count + " names for slot machine thing");
                    RPC_PlayerReadied?.Invoke(playerNames.ToArray());
                }
            }
        }
    }
}
