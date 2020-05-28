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

        
        /*===================================================================
                    Begin Helper Functions/Getters/Setters
        =====================================================================*/


        private static User _current;
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
            }
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