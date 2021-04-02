using DG.Tweening;
using GameBrewStudios;
using GameBrewStudios.Networking;
using Knowlove.UI.Menus;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Knowlove.UI
{
    public class StoreController : MonoBehaviour
    {
        [SerializeField]
        Window_Store storeWindow;

        public static StoreController Instance;

        [SerializeField]
        CanvasGroup canvasGroup;

        [SerializeField]
        TextMeshProUGUI[] walletLabels;

        [SerializeField]
        IAPCard[] iapCards;

        [SerializeField]
        Sprite bronzeBucksIcon, silverBucksIcon, goldBucksIcon;

        private void Awake()
        {
            if (Instance != null) Destroy(this.gameObject);

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            storeWindow.Hide();

            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            User.OnWalletChanged += this.User_OnWalletChanged;
            User.OnInventoryChanged += this.User_OnInventoryChanged;
        }

        private void User_OnInventoryChanged(InventoryItem[] obj)
        {
            UpdateFromPlayerInventory();
        }

        private void User_OnWalletChanged(int obj)
        {
            UpdateFromPlayerWallet();
        }

        public static void Show()
        {
            Instance.UpdateFromPlayerInventory();
            Instance.UpdateFromPlayerWallet();

            Instance.canvasGroup.DOFade(1f, 0.25f).OnComplete(() =>
            {

                Instance.canvasGroup.interactable = true;
                Instance.canvasGroup.blocksRaycasts = true;
                Instance.storeWindow.Show();
            });
        }

        public void Hide()
        {
            Instance.canvasGroup.alpha = 0;
            Instance.canvasGroup.interactable = false;
            Instance.canvasGroup.blocksRaycasts = false;
        }

        public void OnPurchaseSuccess(Product product)
        {
            foreach (PayoutDefinition payout in product.definition.payouts)
            {
                if (payout.type == PayoutType.Currency)
                {
                    User.current.AddCurrency((int)payout.quantity, balance => {
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            PopupDialog.Instance.Show("Your new balance is: " + balance.ToString("n0") + " <sprite=0>");
                        });
                    });

                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        internal void BuyInventoryItem(IAPCard card)
        {
            if (card.isIAP) return;



            CanvasLoading.Instance.Show();
            APIManager.GetUserDetails((user) =>
            {
                CanvasLoading.Instance.Hide();
                bool canPurchase = card.canOwnMultiple ? true : card.amountOwned == 0;

                if (user.wallet >= card.currencyCost && canPurchase)
                {
                    PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
                    {
                    new PopupDialog.PopupButton()
                    {
                        text = "Yes",
                        buttonColor = PopupDialog.PopupButtonColor.Green,
                        onClicked = () =>
                        {
                            CanvasLoading.Instance.Show();
                            APIManager.AddCurrency(-card.currencyCost, balance =>
                            {
                                CanvasLoading.Instance.Hide();
                                User.current.wallet = balance;
                                UpdateFromPlayerWallet();

                                CanvasLoading.Instance.Show();
                                APIManager.AddItem(card.id, card.amountToGive, (inventory) =>
                                {
                                    CanvasLoading.Instance.Hide();
                                    User.current.inventory = inventory;
                                    UpdateFromPlayerInventory();
                                });

                            });
                        }
                    },
                    new PopupDialog.PopupButton()
                    {
                        text = "Nevermind",
                        buttonColor = PopupDialog.PopupButtonColor.Plain,
                        onClicked = () =>{ }
                    }
                    };

                    PopupDialog.Instance.Show("", "Really purchase " + card.title + " for " + card.currencyCost.ToString("n0") + " <sprite=0>?", buttons);



                }
                else if (!canPurchase)
                {
                    PopupDialog.Instance.Show("You already own this item. Visit the My Stuff screen from the main menu to use it.");
                }
                else
                {
                    PopupDialog.Instance.Show("You don't have enough <sprite=0> Know Love Bucks to purchase this item.");
                }
            });




        }

#if UNITY_EDITOR
        [ContextMenu("TEST CURRENCY")]
        public void CurrencyTest()
        {
            User.current.AddCurrency(1, null);
        }

        [ContextMenu("GET ME")]
        public void TestGetMe()
        {
            CanvasLoading.Instance.Show();
            APIManager.GetUserDetails((user) =>
            {
                CanvasLoading.Instance.Hide();
                User.current = user;
                UpdateFromPlayerWallet();
                UpdateFromPlayerInventory();
            });
        }

#endif


        public void UpdateFromPlayerWallet()
        {
            if (walletLabels == null) return;

            int balance = User.current.wallet;
            for (int i = 0; i < walletLabels.Length; i++)
            {
                if (walletLabels[i] != null)
                {
                    walletLabels[i].text = balance.ToString("n0") + " <sprite=0>";
                    walletLabels[i].transform.DOPunchScale(new Vector3(1.5f, 1.5f, 1.5f), 0.5f, 1, 1);
                }
            }
        }

        public void UpdateFromPlayerInventory()
        {
            foreach (IAPCard card in iapCards)
            {
                card.UpdateCard();
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            if (reason == PurchaseFailureReason.UserCancelled) return;

            PopupDialog.Instance.Show($"An error occured while trying to initiate your purchase of \"{product.metadata.localizedTitle}\". Reason: {reason}");
        }

    }
}

