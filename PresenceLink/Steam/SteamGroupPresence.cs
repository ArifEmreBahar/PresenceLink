using HeathenEngineering.SteamworksIntegration;
using System.Threading.Tasks;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Represents a Steam-specific implementation of <see cref="GroupPresence"/> for managing group presence using Steamworks.
    /// </summary>
    public class SteamGroupPresence : GroupPresence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SteamGroupPresence"/> class with the specified owner ID.
        /// </summary>
        /// <param name="ownerId">The ID of the presence owner, typically a PlayFab ID.</param>
        public SteamGroupPresence(string ownerId) : base(ownerId) { }

        public override string PRESENCE_KEY => "connect";

        /// <summary>
        /// Sets the group presence for the specified target PlayFab ID and presence type using Steamworks.
        /// </summary>
        /// <param name="targetPlayfabId">The PlayFab ID of the target user or session owner.</param>
        /// <param name="presenceType">The type of presence (e.g., Normal, InGame, OutOfPlay).</param>
        /// <returns>A completed task, as this method does not involve asynchronous operations.</returns>
        public override Task Set(string targetPlayfabId, PresenceType presenceType)
        {
            string presence = PRESENCE_TOKEN + SEPARATOR + targetPlayfabId;
            bool result = UserData.SetRichPresence("connect", PRESENCE_TOKEN + SEPARATOR + targetPlayfabId);
            UnityEngine.Debug.Log("Set : " + targetPlayfabId + " | " + presenceType + " | R: " + result);
            IsJoinable = true;
            PresenceType = presenceType;
            Current = presence;

            return Task.CompletedTask;
        }
    }
}
