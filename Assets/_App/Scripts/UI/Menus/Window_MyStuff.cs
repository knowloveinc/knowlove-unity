﻿using DG.Tweening;
using GameBrewStudios.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class InventoryItemIconDefinition
{
    public string id;
    public string displayName;
    public Sprite icon;
    public bool hideStackCount;
}

public class Window_MyStuff : Window
{
    [SerializeField]
    Window_ListEditor listEditorWindow;

    [SerializeField]
    GameObject loadingIndicator;

    [SerializeField]
    GameObject listEntryPrefab;

    [SerializeField]
    Transform container;

    [SerializeField]
    InventoryItemIconDefinition defaultIcon;

    public List<InventoryItemIconDefinition> inventoryIcons;

    public static Window_MyStuff Instance;


    private void Awake()
    {
        Instance = this;
    }

    public override void Show()
    {
        base.Show();
        
        DOVirtual.DelayedCall(1f, () => 
        {
            Populate();
        });

    }

    public override void Hide()
    {
        base.Hide();
    }

    private int index = 0;

    public void Populate()
    {
        foreach(Transform child in container)
        {
            if (child.gameObject.activeSelf)
                Destroy(child.gameObject);
        }

        loadingIndicator.SetActive(true);
        APIManager.GetUserDetails((user) => 
        {

            index = 0;
            //Add the Browse Cards entry
            CreateEntry("browseCards", 1);

            //Cycle through purchased items in the players inventory
            for(int i = 0; i < user.inventory.Length; i++)
            {
               _ = CreateEntry(user.inventory[i].itemId, user.inventory[i].amount);
            }

            loadingIndicator.SetActive(false);

        });
    }


    MyStuffEntry CreateEntry(string id, int count)
    {
        //Find the definition for the icon
        InventoryItemIconDefinition browseItem = inventoryIcons.FirstOrDefault(x => x.id.ToLower() == id.ToLower());

        //if (browseItem == null) throw new System.NotImplementedException(id + " Icon definition not found");

        if(browseItem == null)
        {
            browseItem = new InventoryItemIconDefinition() { id = "id", displayName = "Unknown: " + id, icon = defaultIcon.icon };
        }

        //Create the new gameobject from a prefab
        GameObject browseEntry = Instantiate(listEntryPrefab, container, false);
        browseEntry.gameObject.name = (index).ToString("0000");
        
        //Initialize the entry to make it display its icon and text properly, as well as to make the item clickable.
        MyStuffEntry entry = browseEntry.GetComponent<MyStuffEntry>();
        entry.Init(browseItem, count, this);

        return entry;
    }

    

    public void OpenListEditor()
    {
        listEditorWindow.Show();
    }
}
