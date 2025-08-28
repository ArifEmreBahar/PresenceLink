using Oculus.Platform;
using System.Threading.Tasks;
using UnityEngine;

namespace AEB.Integration.Invitation
{
    public class OculusGroupPresence : GroupPresence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SteamGroupPresence"/> class with the specified owner ID.
        /// </summary>
        /// <param name="ownerId">The ID of the presence owner, typically a PlayFab ID.</param>
        public OculusGroupPresence(string ownerId) : base(ownerId) { }

        /// <summary>
        /// Internal flag to track errors during Group Presence setup.
        /// </summary>
        bool setError;

        /// <summary>
        /// Sets the group presence for the specified target PlayFab ID and presence type using Steamworks.
        /// </summary>
        /// <param name="targetPlayfabId">The PlayFab ID of the target user or session owner.</param>
        /// <param name="presenceType">The type of presence (e.g., Normal, InGame, OutOfPlay).</param>
        /// <returns>A completed task, as this method does not involve asynchronous operations.</returns>
        public async override Task Set(string targetPlayfabId, PresenceType presenceType)
        {
            string presence = PRESENCE_TOKEN + SEPARATOR + targetPlayfabId;
            //bool result = UserData.SetRichPresence("connect", PRESENCE_TOKEN + SEPARATOR + targetPlayfabId);

            bool setToPlatform = false;

            string destination = PRESENCE_TOKEN;
            string lobbySessionID = presence;
            string matchSessionID = "";
            bool joinable = true;

#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
            setToPlatform = true;
#endif
            if (PlatformSettings.UseStandalonePlatform)
            {
                setToPlatform = true;
            }

            if (setToPlatform)
            {
                setError = true;

                GroupPresenceOptions groupPresenceOptions = new GroupPresenceOptions();
                groupPresenceOptions.SetDestinationApiName(destination);
                groupPresenceOptions.SetLobbySessionId(lobbySessionID);
                groupPresenceOptions.SetMatchSessionId(matchSessionID);
                groupPresenceOptions.SetIsJoinable(joinable);

                // Temporary workaround until bug fix
                // GroupPresence.Set() can sometimes fail. Wait until it is done, and if it
                // failed, try again.
                while (setError)
                {
                    var tcs = new TaskCompletionSource<bool>();

                    Oculus.Platform.GroupPresence.Set(groupPresenceOptions).OnComplete(message =>
                    {
                        setError = message.IsError;

                        if (setError)
                        {
                            LogError("Failed to setup Group Presence", message.GetError());
                        }
                        else
                        {
                            //LobbySessionID = lobbySessionID;
                            //MatchSessionID = matchSessionID;
                            //IsJoinable = joinable;
                            //Destination = destination;
                            //LaunchType = launchType;

                            Debug.Log("Group Presence set successfully setToPlatform !");
                            Debug.Log(ToString());
                        }

                        tcs.SetResult(true);
                    });

                    await tcs.Task;
                }
            }
            else
            {
                //Destination = destination;
                //LobbySessionID = lobbySessionID;
                //MatchSessionID = matchSessionID;
                //IsJoinable = joinable;
                //LaunchType = launchType;

                Debug.Log("Group Presence set successfully NOT");
                Debug.Log(ToString());
            }


            //UnityEngine.Debug.Log("Set : " + targetPlayfabId + " | " + presenceType + " | R: " + result);

            IsJoinable = true;
            PresenceType = presenceType;
            Current = presence;
        }

        /// <summary>
        /// Logs an error to the Unity console with details from the Oculus SDK error object.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="error">The Oculus error object containing additional details.</param>
        private void LogError(string message, Oculus.Platform.Models.Error error)
        {
            Debug.LogError(message);
            Debug.LogError("ERROR MESSAGE:   " + error.Message);
            Debug.LogError("ERROR CODE:      " + error.Code);
            Debug.LogError("ERROR HTTP CODE: " + error.HttpCode);
        }

    }
}
