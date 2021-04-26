using GameBrewStudios;
using GameBrewStudios.Networking;
using System.Linq;
using UnityEngine.Purchasing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Knowlove.XPSystem;

namespace Knowlove.UI
{
    public class IAPCard : MonoBehaviour
    {
        public string title;
        public string description;

        public Sprite iconSprite;
        public string iconText;

        [Tooltip("If isIAP = true, use the IAPCatalog id for this product, else use the itemId of the item")]
        public string id;

        [SerializeField] public int amountToGive;

        [SerializeField] private TextMeshProUGUI ownedCount;

        [SerializeField] private TextMeshProUGUI titleLabel, descLabel, iconTextLabel, priceLabel;

        [SerializeField] private Image iconImage;

        /// <summary>
        /// Ignored if isIAP = true;
        /// </summary>
        public int currencyCost;

        /// <summary>
        /// Used to determine if its using currency to purchase or if its an IAP through the app store
        /// </summary>
        public bool isIAP;
        public bool canOwnMultiple;

        public int amountOwned
        {
            get
            {
                if(id == "cards" && isHasAllCards)
                    return 1;

                InventoryItem foundItem = User.current.inventory.FirstOrDefault(x => x.itemId == this.id);
                if (foundItem == null)
                    return 0;

                return foundItem.amount;
            }
        }

        public bool owned
        {
            get => !canOwnMultiple && amountOwned >= 1;
        }

        public bool isHasAllCards
        {
            get => InfoPlayer.Instance.CheckMarkAllCard();
        }

        private void Start()
        {
            if (!isIAP)
            {
                IAPButton iapButton = gameObject.GetComponent<IAPButton>();
                if (iapButton != null)
                    Destroy(iapButton);

                Button btn = GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    StoreController.Instance.BuyInventoryItem(this);
                });
            }
            else
            {
                Product product = CodelessIAPStoreListener.Instance.GetProduct(id);
            }               
        }

        internal void UpdateCard()
        {
            if (isIAP)
            {
                Product product = CodelessIAPStoreListener.Instance.GetProduct(id);
                titleLabel?.SetText(title);
                descLabel?.SetText("Get it now!");
            }
            else
            {
                titleLabel?.SetText(title);
                descLabel?.SetText(description);
                ShowCountCard();
            }

            iconTextLabel?.SetText(iconText);

            if (iconSprite != null)
                iconImage.sprite = iconSprite;

            if (!isIAP)
            {
                Debug.Log("Setting price text: isOwned? " + owned.ToString());

                if(id == "cards")
                    priceLabel.text = isHasAllCards ? "OWNED" : currencyCost.ToString("n0") + " <sprite=0>";
                else
                    priceLabel.text = owned ? "OWNED" : currencyCost.ToString("n0") + " <sprite=0>";
            }
            else
            {
                Product product = CodelessIAPStoreListener.Instance.GetProduct(id);
                Debug.Log("IAP COST IS: " + product.metadata.localizedPriceString);
                priceLabel.text = isIAP ? product.metadata.localizedPrice + " " + product.metadata.isoCurrencyCode : currencyCost.ToString() + " <sprite=0>";
            }
        }

        private void ShowCountCard()
        {
            if (ownedCount != null)
            {
                APIManager.GetUserDetails((user) =>
                {
                    for (int i = 0; i < user.inventory.Length; i++)
                    {
                        if (user.inventory[i].itemId.ToLower() == "avoidSingle".ToLower())
                        {
                            if(user.inventory[i].amount > 0)
                            {
                                ownedCount.text = "Owned: " + user.inventory[i].amount;
                                ownedCount.gameObject.SetActive(true);
                            }
                            else
                                ownedCount.gameObject.SetActive(false);
                        }  
                    }

                    if(user.inventory.Length == 0)
                        ownedCount.gameObject.SetActive(false);
                });
            }
        }
    }
}

