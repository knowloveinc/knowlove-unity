﻿using GameBrewStudios;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IAPCard : MonoBehaviour
{
    public string title;
    public string description;

    public Sprite iconSprite;
    public string iconText;

    [Tooltip("If isIAP = true, use the IAPCatalog id for this product, else use the itemId of the item")]
    public string id;

    [SerializeField]
    public int amountToGive;

    [SerializeField]
    TextMeshProUGUI ownedCount;

    /// <summary>
    /// Ignored if isIAP = true;
    /// </summary>
    public int currencyCost;

    /// <summary>
    /// Used to determine if its using currency to purchase or if its an IAP through the app store
    /// </summary>
    public bool isIAP;

    public int amountOwned
    {
        get
        {
            InventoryItem foundItem = User.current.inventory.FirstOrDefault(x => x.itemId == this.id);
            if (foundItem == null)
                return 0;

            return foundItem.amount;
        }
    }
    public bool canOwnMultiple;

    public bool owned
    {
        get
        {
            return !canOwnMultiple && amountOwned >= 1;
        }
    }


    private void Start()
    {
        if(!isIAP)
        {
            IAPButton iapButton = gameObject.GetComponent<IAPButton>();
            if(iapButton != null)
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

    [SerializeField]
    TextMeshProUGUI titleLabel, descLabel, iconTextLabel, priceLabel;

    [SerializeField]
    Image iconImage;

    internal void UpdateCard()
    {

        if (isIAP)
        {
            Product product = CodelessIAPStoreListener.Instance.GetProduct(id);
            titleLabel?.SetText(product.metadata.localizedTitle);
            descLabel?.SetText(product.metadata.localizedDescription);
        }
        else
        {
            titleLabel?.SetText(title);
            descLabel?.SetText(description);
        }

        iconTextLabel?.SetText(iconText);
        if(iconSprite != null)
            iconImage.sprite = iconSprite;

        if(!isIAP)
        {
            Debug.Log("Setting price text: isOwned? " + owned.ToString());
            priceLabel.text = owned ? "OWNED" : currencyCost.ToString("n0") + " <sprite=0>";
        }
        else
        {
            Product product = CodelessIAPStoreListener.Instance.GetProduct(id);
            Debug.Log("IAP COST IS: " + product.metadata.localizedPriceString);
            priceLabel.text = isIAP ? product.metadata.localizedPriceString  + " " + product.metadata.isoCurrencyCode : currencyCost.ToString() + " <sprite=0>";
        }
    }


}
