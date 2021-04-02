using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using GameBrewStudios.Networking;
using GameBrewStudios;

namespace Knowlove.MyStuffInGame
{
    public class GameStuff : MonoBehaviourPunCallbacks
    {
        private Player _currentPlayer;

        private const string idAvoidSingleCard = "avoidSingle";
        private const string idFlipTheTableCard = "tableFlip";

        private int _avoidSingle;
        private int _flipTheTable;
        private int _amountDeleteCard = -1;


        public int AvoidSingle
        {
            get => _avoidSingle;
        }

        public int FlipTheTable
        {
            get => _flipTheTable;
        }

        [ContextMenu("SpesialCard")]
        public void GetSpecialCard()
        {
            _currentPlayer = PhotonNetwork.LocalPlayer;
            ExitGames.Client.Photon.Hashtable playerProperties = _currentPlayer.CustomProperties;

            APIManager.GetUserDetails((user) =>
            {
                _avoidSingle = user.inventory[0].amount;
                _flipTheTable = user.inventory[1].amount;

                playerProperties["avoidSingleCards"] = _avoidSingle;
                playerProperties["flipTheTable"] = _flipTheTable;
                _currentPlayer.SetCustomProperties(playerProperties);
            });
        }

        public void DeleteCardFromInventory(int idCard)
        {
            if (idCard == 0)
                DeleteCardFromServer(idAvoidSingleCard);
            else if (idCard == 1)
                DeleteCardFromServer(idFlipTheTableCard);
        }

        private void DeleteCardFromServer(string cardName)
        {
            APIManager.AddItem(cardName, _amountDeleteCard, (inventory) =>
            {
                User.current.inventory = inventory;

                GetSpecialCard();
            });
        }
    }
}
