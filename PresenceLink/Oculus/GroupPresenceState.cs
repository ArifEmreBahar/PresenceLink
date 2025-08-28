using Oculus.Platform;
using System.Threading.Tasks;
using UnityEngine;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Represents the state for Group Presence functionality, handling settings and platform-specific integration.
    /// </summary>
    public class GroupPresenceState
    {
        #region Fields

        /// <summary>
        /// Constant token identifier for Group Presence.
        /// </summary>
        public const string DESTINATION = "AEB";

        /// <summary>
        /// The destination API name for Group Presence.
        /// </summary>
        public string Destination { get; private set; }

        /// <summary>
        /// The lobby session ID associated with the Group Presence.
        /// </summary>
        public string LobbySessionID { get; private set; }

        /// <summary>
        /// The match session ID associated with the Group Presence.
        /// </summary>
        public string MatchSessionID { get; private set; }

        /// <summary>
        /// Indicates whether the session is joinable.
        /// </summary>
        public bool IsJoinable { get; private set; }

        public LaunchType LaunchType { get; private set; }

        /// <summary>
        /// Internal flag to track errors during Group Presence setup.
        /// </summary>
        private bool setError;

        #endregion

        #region Public

        /// <summary>
        /// Configures and sets the Group Presence state with the provided PlayFab ID.
        /// Handles retry logic in case of errors.
        /// </summary>
        /// <param name="playfabId">The PlayFab ID used to configure Group Presence.</param>
        public async Task Set(string targetPlayfabId, LaunchType launchType)
        {
            bool setToPlatform = false;

            string destination = DESTINATION;
            string lobbySessionID = DESTINATION + "-" + targetPlayfabId;
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
                            LobbySessionID = lobbySessionID;
                            MatchSessionID = matchSessionID;
                            IsJoinable = joinable;
                            Destination = destination;
                            LaunchType = launchType;

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
                Destination = destination;
                LobbySessionID = lobbySessionID;
                MatchSessionID = matchSessionID;
                IsJoinable = joinable;
                LaunchType = launchType;

                Debug.Log("Group Presence set successfully NOT");
                Debug.Log(ToString());
            }
        }

        /// <summary>
        /// Returns a string representation of the current Group Presence state.
        /// </summary>
        /// <returns>A formatted string describing the Group Presence state.</returns>
        public override string ToString()
        {
            return $"------GROUP PRESENCE STATE------\n" +
                   $"Destination:      {Destination}\n" +
                   $"Lobby Session ID: {LobbySessionID}\n" +
                   $"Match Session ID: {MatchSessionID}\n" +
                   $"Joinable:         {IsJoinable}\n" +
                   $"Launch Type:      {LaunchType}\n" +
                   $"--------------------------------";
        }

        public bool IsUnknown()
        {
            return LaunchType == LaunchType.Unknown;
        }

        public bool IsGroupLaunch(string lobbySessionId)
        {
            if (lobbySessionId == null) return true;

            return !lobbySessionId.Contains(Destination);
        }

        public bool IsDestination(string name)
        {
            return DESTINATION.Equals(name);
        }

        public bool TryExtractPlayFabId(string input, out string playfabId)
        {
            playfabId = null;

            if (string.IsNullOrEmpty(input))
                return false;

            string destinationTag = Destination + "-";
            if (input.StartsWith(destinationTag))
                playfabId = input.Substring(destinationTag.Length);

            return true;
        }


        #endregion

        #region Private

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

        #endregion
    }
}
