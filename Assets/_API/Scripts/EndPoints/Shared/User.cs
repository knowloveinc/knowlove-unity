using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace GameBrewStudios.Networking
{
    public partial class APIManager
    {

        public static void Connect(System.Action<bool> onComplete)
        {
            ServerRequest.CallAPI("/", HTTPMethod.GET, null, (response) => 
            {
                if (response.hasError)
                {
                    Debug.LogError("Error connecting to API: " + response.Error);
                    onComplete?.Invoke(false);
                }
                else if (response.statusCode == 200)
                {
                    Debug.Log("<color=Green>Successfully connected to API Server.</color>");
                    onComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogError("Error connecting to API: Unknown error..." + response.text);
                    onComplete?.Invoke(false);
                }

            }, false);
        }

        // Start is called before the first frame update
        public static void Authenticate(string email, string password, Action<bool> onComplete)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("email", email);
            body.Add("password", password);


            ServerRequest.CallAPI("/authenticate", HTTPMethod.POST, body, (response) =>
            {
                if (response.hasError)
                {
                    Debug.LogError(response.Error);
                    onComplete?.Invoke(false);
                    return;
                }

                Dictionary<string, object> responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.text);

                if (responseData != null)
                {
                    if (responseData.ContainsKey("token"))
                    {
                        AuthToken.current = new AuthToken((string)responseData["token"], responseData.ContainsKey("refreshToken") ? (string)responseData["refreshToken"] : null);
                    }

                    if (responseData.ContainsKey("user"))
                    {
                        User.current = JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(responseData["user"]));
                    }

                    if (User.current != null && AuthToken.current != null)
                    {
                        onComplete?.Invoke(true);
                        return;
                    }
                }

                onComplete?.Invoke(false);
            }, false);
        }
        public static void Register(string username, string nickname, string password, Action<bool> onComplete)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();

            body.Add("email", username);
            body.Add("displayName", nickname);
            body.Add("password", password);
            

            ServerRequest.CallAPI("/register", HTTPMethod.POST, body, (response) =>
             {
                 if (response.hasError)
                 {
                     Debug.LogError(response.Error);
                     onComplete?.Invoke(false);
                     return;
                 }

                 Dictionary<string, object> responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.text);

                 if (responseData != null)
                 {
                     if (responseData.ContainsKey("token"))
                     {
                         AuthToken.current = new AuthToken((string)responseData["token"], responseData.ContainsKey("refreshToken") ? (string)responseData["refreshToken"] : null);
                     }

                     if (responseData.ContainsKey("user"))
                     {
                         User.current = JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(responseData["user"]));
                     }

                     if (User.current != null && AuthToken.current != null)
                     {
                         
                         onComplete?.Invoke(true);
                         return;
                     }
                 }

                 onComplete?.Invoke(false);
             }, false);
        }
        public static void RefreshToken(Action<ServerResponse> onComplete)
        {
            ServerRequest.CallAPI("/refreshToken", HTTPMethod.GET, null, onComplete, true);
        }

        public static void ForgotPassword(string email, Action<Dictionary<string, object>> onComplete)
        {
            Dictionary<string, object> body = new Dictionary<string, object>() 
            {
                {"email", email }
            };
            ServerRequest.CallAPI("/forgot", HTTPMethod.POST, body, (r) => { ServerRequest.ResponseHandler(r, null, onComplete); }, false);
        }

        public static void ResetPassword(string email, string code, string password, Action<Dictionary<string, object>> onComplete)
        {
            Dictionary<string, object> body = new Dictionary<string, object>()
            {
                {"code", code },
                {"email", email },
                {"password", password }
            };
            ServerRequest.CallAPI("/reset", HTTPMethod.POST, body, (r) => { ServerRequest.ResponseHandler(r, null, onComplete); }, false);
        }

        public static void GetUserDetails(Action<User> onComplete)
        {
            ServerRequest.CallAPI("/me", HTTPMethod.GET, null, (r) => { ServerRequest.ResponseHandler(r, "user", onComplete); }, true);
        }

        public static void AddCurrency(int amount, Action<int> onComplete)
        {
            Dictionary<string, object> body = new Dictionary<string, object>()
            {
                {"amount", amount}
            };

            ServerRequest.CallAPI("/wallet", HTTPMethod.POST, body, (r) => { ServerRequest.ResponseHandler(r, "wallet", onComplete); }, true);
        }

        public static void AddItem(string itemId, int amount, Action<InventoryItem[]> onComplete)
        {
            Dictionary<string, object> body = new Dictionary<string, object>()
            {
                {"itemId", itemId },
                {"amount", amount}
            };

            ServerRequest.CallAPI("/inventory", HTTPMethod.POST, body, (r) => { ServerRequest.ResponseHandler(r, "inventory", onComplete); }, true);
        }

        public class NonNegotiableListResponse
        {
            public bool success;
            public List<string> list;
            public string error;
        }

        public static void UpdateNonNegotiableList(List<string> list, Action<NonNegotiableListResponse> onComplete)
        {
            Debug.Log("Sending call to server to update non negotiable list");
            Dictionary<string, object> body = new Dictionary<string, object>()
            {
                {"nonNegotiableList", list }
            };
            ServerRequest.CallAPI("/nonNegotiableList", HTTPMethod.POST, body, (r) => { ServerRequest.ResponseHandler(r, null, onComplete); }, true);
        }
    }
}