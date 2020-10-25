using GameBrewStudios.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameBrewStudios
{



    /// <summary>
    /// This class represents an exact replica of the UserSchema on the server, with some added helper functions for ease of use
    /// </summary>
    

    [System.Serializable]
    public class UserSimple
    {
        /// <summary>
        /// Represents a Mongo ObjectID from the User collection in the database
        /// </summary>
        public string _id;

        /// <summary>
        /// Email address of the user, should assume validity as the server is responsible for checking it
        /// </summary>
        public string email;

        /// <summary>
        /// Username of the user, used only for display in the Chat feature we add later (so users can message eachother with @username) and to provide an alternative means of login instead of using the email address
        /// </summary>
        public string username;

        /// <summary>
        /// When displaying the name of a user anywhere in the app (besides chat) always use the Display Name property
        /// </summary>
        public string displayName;

        /// <summary>
        /// Not currently used for anything, but will be used for checking access permissions later on. Defaults to: 'player'
        /// </summary>
        public string role;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemId;
        public int amount;
    }

    [System.Serializable]
    public class User
    {
        public static event System.Action OnUserNotLoggedIn;


        /// <summary>
        /// Represents a Mongo ObjectID from the User collection in the database
        /// </summary>
        public string _id;

        /// <summary>
        /// Email address of the user, should assume validity as the server is responsible for checking it
        /// </summary>
        public string email;

        /// <summary>
        /// When displaying the name of a user anywhere in the app (besides chat) always use the Display Name property
        /// </summary>
        public string displayName;


        public int wallet;
        public InventoryItem[] inventory;
        
        /*===================================================================
                    Begin Helper Functions/Getters/Setters
        =====================================================================*/


        private static User _current;

        public List<string> nonNegotiableList = new List<string>();


        /// <summary>
        /// After successful login, you should create an Instance of the user class, and then store it into this static variable for easily accessing the local users data throughout the app code.
        /// </summary>
        public static User current
        {
            get
            {
                if (_current == null)
                {
                    OnUserNotLoggedIn?.Invoke();
                }

                return _current;
            }
            set
            {
                _current = value;
                Debug.LogError("CURRENT USER UPDATED");
            }
        }

        public static event System.Action<int> OnWalletChanged;
        public static event System.Action<InventoryItem[]> OnInventoryChanged;

        internal void AddCurrency(int amount, System.Action<int> callback)
        {
            CanvasLoading.Instance.Show();
            APIManager.AddCurrency(amount, (verifiedAmount) => 
            {
                CanvasLoading.Instance.Hide();
                User.current.wallet = verifiedAmount;
                Debug.Log("User new wallet = " + User.current.wallet);
                OnWalletChanged?.Invoke(User.current.wallet);
                callback?.Invoke(User.current.wallet);
            });
        }

        internal void AddItemToInventory(string itemId, int amount)
        {
            CanvasLoading.Instance.Show();
            APIManager.AddItem(itemId, amount, (inventory) =>
            {
                CanvasLoading.Instance.Hide();
                User.current.inventory = inventory;
                OnInventoryChanged?.Invoke(User.current.inventory);
            });
        }
    }

    [System.Serializable]
    public class SupportTicket
    {
        public string _id;
        public string user;
        public string email;
        public string message;
        public string category;
        public string status; //Open, Closed, etc
    }
}