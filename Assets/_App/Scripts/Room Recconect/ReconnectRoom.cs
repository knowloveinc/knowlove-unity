using Knowlove.UI;
using Photon.Pun;
using System;
using System.Linq;
using UnityEngine;

namespace Knowlove.RoomReconnect
{
    public class ReconnectRoom : MonoBehaviourPunCallbacks
    {
        public static ReconnectRoom Instance;

        private const string _playerProperties = "Properties";

        public Action SettedPlayerProperties;

        private int _owner;

        private void Awake()
        {
            Instance = this;
            _owner = PhotonNetwork.CurrentRoom.MasterClientId;
        }
     
        public void RPC_PlayerReconnectSetProperties()
        {
            SettedPlayerProperties?.Invoke();
            photonView.RPC(nameof(PlayerReconnectSetProperties), RpcTarget.All);
        }

        [PunRPC]
        private void PlayerReconnectSetProperties()
        {
            ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;

            PlayerCustomValue playerCustomValue = JsonUtility.FromJson<PlayerCustomValue>(PlayerPrefs.GetString(_playerProperties));

            playerProperties["avoidSingleCards"] = playerCustomValue.avoidSingleCard;
            playerProperties["wallet"] = playerCustomValue.wallet;
            playerProperties["turnBank"] = playerCustomValue.turnBank;
            playerProperties["protectedFromSingleInRelationship"] = playerCustomValue.protectedFromSingleInRelationship;
            playerProperties["diceCount"] = playerCustomValue.diceCount;
            playerProperties["progress"] = playerCustomValue.progress;
            playerProperties["dateCount"] = playerCustomValue.dateCount;
            playerProperties["relationshipCount"] = playerCustomValue.relationshipCount;
            playerProperties["marriageCount"] = playerCustomValue.marriageCount;
            playerProperties["yearsElapsed"] = playerCustomValue.yearsElapsed;

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            NetworkManager.Instance.players = PhotonNetwork.PlayerList.ToList();

            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.MasterClientId != _owner)
            {
                if (TurnManager.Instance.turnIndex == 0)
                    TurnManager.Instance.turnIndex = PhotonNetwork.CurrentRoom.PlayerCount - 1;
                else
                    TurnManager.Instance.turnIndex -= 1;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                BoardManager.Instance.SetDiceRigidbody();
                TurnManager.Instance.EndTurn();
            }
            else
                BoardManager.Instance.DestroyDiceRigidBody();
                

            CanvasLoading.Instance.Hide();
        }
    }
}
