using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using GameBrewStudios.Networking;
using GameBrewStudios;
using Knowlove.UI;

namespace Knowlove.MyStuffInGame
{
    public class GameStuff : MonoBehaviourPunCallbacks
    {
        private Player _currentPlayer;

        private const string idAvoidSingleCard = "avoidSingle";

        private int _avoidSingle;
        private int _wallet;
        private int _amountDeleteCard = -1;


        public int AvoidSingle
        {
            get => _avoidSingle;
        }

        public int Wallet
        {
            get => _wallet;
        }

        private void OnDestroy()
        {
            if (StoreController.Instance.gameStuff != null)
                StoreController.Instance.gameStuff = null;
        }

        [ContextMenu("SpesialCard")]
        public void GetSpecialCard()
        {
            photonView.RPC(nameof(RPC_GetSpecialCard), RpcTarget.AllViaServer);
        }

        [PunRPC]
        public void RPC_GetSpecialCard()
        {
            if (StoreController.Instance.gameStuff == null)
                StoreController.Instance.gameStuff = this;

            _currentPlayer = PhotonNetwork.LocalPlayer;
            ExitGames.Client.Photon.Hashtable playerProperties = _currentPlayer.CustomProperties;

            APIManager.GetUserDetails((user) =>
            {
                for (int i = 0; i < user.inventory.Length; i++)
                {
                    if(user.inventory[i].itemId.ToLower() == "avoidSingle".ToLower())
                        _avoidSingle = user.inventory[i].amount;
                }

                _wallet = user.wallet;

                playerProperties["avoidSingleCards"] = _avoidSingle;
                playerProperties["wallet"] = _wallet;
                _currentPlayer.SetCustomProperties(playerProperties);
            });
        }

        public void DeleteCardFromInventory(int turnIndex)
        {
            photonView.RPC(nameof(RPC_DeleteCardFromInventory), RpcTarget.AllViaServer, turnIndex);
        }

        [PunRPC]
        private void RPC_DeleteCardFromInventory(int turnIndex)
        {
            Player currentPlayer = NetworkManager.Instance.players[turnIndex];

            if(PhotonNetwork.LocalPlayer == currentPlayer)
                DeleteCardFromServer(idAvoidSingleCard);
        }

        private void DeleteCardFromServer(string cardName)
        {
            APIManager.GetUserDetails((user) => 
            {
                APIManager.AddItem(cardName, _amountDeleteCard, (inventory) =>
                {
                    User.current.inventory = inventory;

                    GetSpecialCard();
                });
            });
        }
    }
}
