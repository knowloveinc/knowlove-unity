using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace GameBrewStudios.Networking
{
    public partial class APIManager
    {

        /// <summary>
        /// Returns all leaderboard entries with shortCode
        /// </summary>
        public static void GetLeaderboardEntries(string shortCode, Action<LeaderboardEntry[]> onComplete)
        {
            ServerRequest.CallAPI("/leaderboard/" + shortCode, HTTPMethod.GET, null, (response) => ServerRequest.ResponseHandler(response, "entries", onComplete), true);
        }

        /// <summary>
        /// Returns all leaderboard entries with shortCode but only if they belong to a team member from teamId
        /// </summary>
        public static void GetLeaderboardEntries(string shortCode, string teamId, Action<LeaderboardEntry[]> onComplete)
        {
            ServerRequest.CallAPI("/leaderboard/" + shortCode + "/" + teamId, HTTPMethod.GET, null, (response) =>
            {
                if (response.hasError)
                {
                    Debug.LogError("GetLeaderboardEntries Error: " + response.Error);
                    onComplete?.Invoke(null);
                    return;
                }

                Dictionary<string, object> responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.text);

                if (responseData != null && responseData.ContainsKey("entries"))
                {
                    LeaderboardEntry[] entries = JsonConvert.DeserializeObject<LeaderboardEntry[]>(JsonConvert.SerializeObject(responseData["entries"]));

                    onComplete?.Invoke(entries);
                    return;
                }

                Debug.LogWarning("GetLeaderboardEntries failed somehow. Server Response was: " + response.text);
                onComplete?.Invoke(null);

            }, true);
        }

        internal static void UpdateQuestion(object current)
        {
            throw new NotImplementedException();
        }
    }
}