using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;
using AEB.PlayFab.Authorization;
using AEB.PlayFab.Friend;
using Steamworks;
using System;
using UnityEngine;
using Valve.VR;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// A service for managing Steam invitations and group presence.
    /// </summary>
    public class SteamInvitationService : IInvitationService
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamInvitationService"/> class with the specified owner ID.
        /// </summary>
        /// <param name="ownerId">The ID of the owner for this service.</param>
        public SteamInvitationService(string ownerId)
        {
            GroupPresence = new SteamGroupPresence(ownerId);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Indicates whether the service is initialized.
        /// </summary>
        bool isInitialized;

        /// <summary>
        /// The key used to identify the overlay.
        /// </summary>
        public const string OVERLAY_KEY = "AEB_OVERLAY_KEY";

        /// <summary>
        /// The name of the overlay.
        /// </summary>
        public const string OVERLAY_NAME = "AEB_OVERLAY";

        /// <summary>
        /// The handle for the overlay. Initially set to an invalid handle.
        /// </summary>
        ulong overlayHandle = OpenVR.k_ulOverlayHandleInvalid;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered for any join intent (e.g., invitation accepted).
        /// </summary>
        public event Action<Invitation> OnAnyJoinIntent;

        /// <summary>
        /// Event triggered when a join intent occurs in an in-game context.
        /// </summary>
        public event Action<Invitation> OnInGameIntent;

        /// <summary>
        /// Event triggered when the app is launched via a join intent.
        /// </summary>
        public event Action<Invitation> OnAppLaunchedByJoinIntent;

        /// <summary>
        /// Event triggered when invitations are sent.
        /// </summary>
        public event Action<Invitation[]> OnInvitationSent;

        #endregion

        #region Properties

        /// <summary>
        /// Manages group presence for the service.
        /// </summary>
        public GroupPresence GroupPresence { get; private set; }

        #endregion

        #region Public

        /// <summary>
        /// Initializes the Steam Invitation Service by setting up necessary callbacks.
        /// </summary>
        public void Initialize()
        {
            if (!AccountManager.Initialized || isInitialized) return;

            GroupPresence.SetMine();

            var initError = EVRInitError.None;
            if (OpenVR.System == null)
            {
                OpenVR.Init(ref initError, EVRApplicationType.VRApplication_OpenXROverlay);
                if (initError != EVRInitError.None)
                    Debug.LogWarning("Failed to initialize OpenVR: " + initError);
            }

            if (OpenVR.Overlay != null)
            {
                var overlayError = OpenVR.Overlay.CreateOverlay(OVERLAY_KEY, OVERLAY_NAME, ref overlayHandle);
                if (overlayError != EVROverlayError.None)
                    Debug.LogWarning("Failed to create overlay: " + overlayError);
            }

            Overlay.Client.EventGameRichPresenceJoinRequested.AddListener(HandleJoinIntentReceived);
            HeathenEngineering.SteamworksIntegration.API.
            Friends.Client.EventFriendRichPresenceUpdate.AddListener(HandleFriendTriesToJoin);

            isInitialized = true;

            Debug.Log("[SteamInvitationService] Service initialized.");
        }

        /// <summary>
        /// Terminates the Steam Invitation Service, cleaning up resources.
        /// </summary>
        public void Terminate()
        {
            if (OpenVR.System != null)
                OpenVR.Shutdown();

            if (overlayHandle != OpenVR.k_ulOverlayHandleInvalid && OpenVR.Overlay != null)
            {
                var error = OpenVR.Overlay.DestroyOverlay(overlayHandle);
                if (error != EVROverlayError.None)
                    Debug.LogWarning("Failed to dispose overlay: " + error);
            }

            isInitialized = false;

            Debug.Log("[SteamInvitationService] Service terminated.");
        }

        /// <summary>
        /// Logs the details of an Invitation object to the Unity console in a single log statement.
        /// </summary>
        /// <param name="invitation">The invitation to debug.</param>
        public void DebugInvitation(Invitation invitation)
        {
            Debug.Log($"[INVITATION DETAILS] Platform: {invitation.Platform}, " +
                      $"Sender ID: {invitation.SenderId}, " +
                      $"Sender Display Name: {invitation.SenderDisplayName}, " +
                      $"Invitation ID: {invitation.InvitationId}, " +
                      $"Additional Data: {invitation.AdditionalData}");
        }

        #endregion

        #region Private

        /// <summary>
        /// Handles when a join intent is received (e.g., an invitation is accepted).
        /// </summary>
        /// <param name="user">The user who sent the invitation.</param>
        /// <param name="presenceVal">The presence value associated with the invitation.</param>
        void HandleJoinIntentReceived(UserData user, string presenceVal)
        {
            if (string.IsNullOrEmpty(presenceVal) || user == UserData.Me) return;

            if (!GroupPresence.TryExtractPlayFabId(presenceVal, out var playfabId))
            {
                Debug.LogWarning("Failed to extract PlayFab ID from session owner ID.");
                return;
            }

            var invitation = new Invitation
            {
                Platform = "Steam",
                PresenceType = PresenceType.InGame,
                SenderId = playfabId,
                SenderDisplayName = user.Nickname,
                InvitationId = presenceVal,
                AdditionalData = presenceVal
            };

            OnAnyJoinIntent?.Invoke(invitation);
            OnInGameIntent?.Invoke(invitation);
        }

        /// <summary>
        /// Handles when a friend tries to join via rich presence.
        /// </summary>
        /// <param name="richPresence">The rich presence update data.</param>
        void HandleFriendTriesToJoin(FriendRichPresenceUpdate richPresence)
        {
            var friend = richPresence.Friend;
            if (friend == UserData.Me) return;
            string connectData = SteamFriends.GetFriendRichPresence(friend.id, GroupPresence.PRESENCE_KEY);
            if (!GroupPresence.TryExtractPlayFabId(connectData, out var playfabId) || playfabId != GroupPresence.OwnerId)
                return;

            OnInvitationSent?.Invoke(null);
        }

        #endregion
    }
}
