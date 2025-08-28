using AEB.PlayFab.Authorization;
using System;
using UnityEngine;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Manages invitations within the application, including sending and receiving invitations,
    /// handling presence, and initializing the underlying invitation service.
    /// </summary>
    public static class InvitationManager
    {
        #region Fields

        /// <summary>
        /// Indicates whether the invitation manager has been initialized.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Gets the current presence ID from the underlying invitation service.
        /// </summary>
        public static string CurrentPresence => _invitationService.GroupPresence.Current;

        /// <summary>
        /// The service responsible for handling invitation-related logic.
        /// </summary>
        static IInvitationService _invitationService;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when an in-game invitation is accepted.
        /// </summary>
        public static event Action<Invitation> OnInGameInvitationAccepted;

        /// <summary>
        /// Event triggered when one or more invitations are successfully sent.
        /// </summary>
        public static event Action<Invitation[]> OnInGameInvitationSent;

        #endregion

        #region Public

        /// <summary>
        /// Initializes the <see cref="InvitationManager"/> and sets up the necessary invitation service.
        /// </summary>
        public static void Initialize()
        {
            if (Initialized)
            {
                Debug.LogWarning("[InvitationManager] Already initialized.");
                return;
            }

            _invitationService = new ServiceFactory().CreateInvitationService(AccountManager.Account.Credential.PlayfabID);

            _invitationService.Initialize();

            _invitationService.OnInGameIntent += HandleOnInGameIntent;
            _invitationService.OnInvitationSent += HandleOnInvitationSent;

            Initialized = true;
        }

        /// <summary>
        /// Sets the group presence back to the owner's default presence.
        /// </summary>
        public static void SetPresenceMine()
        {
            if (!Initialized || _invitationService == null)
            {
                Debug.LogError("[InvitationManager] Cannot set presence; InvitationManager is not initialized.");
                return;
            }

            _invitationService.GroupPresence.SetMine();
        }

        /// <summary>
        /// Sets the group presence based on the provided invitation details.
        /// </summary>
        /// <param name="invitation">The invitation containing the sender's ID and presence type.</param>
        public static void SetPresence(Invitation invitation)
        {
            if (!Initialized || _invitationService == null)
            {
                Debug.LogError("[InvitationManager] Cannot set presence; InvitationManager is not initialized.");
                return;
            }

            _invitationService.GroupPresence.Set(invitation.SenderId, invitation.PresenceType);
        }

        /// <summary>
        /// Terminates the <see cref="InvitationManager"/>, cleaning up resources and unregistering events.
        /// </summary>
        public static void Terminate()
        {
            if (!Initialized || _invitationService == null)
            {
                Debug.LogWarning("[InvitationManager] Cannot terminate; InvitationManager is not initialized.");
      
            }

            _invitationService.Terminate();

            _invitationService.OnInGameIntent -= HandleOnInGameIntent;
            _invitationService.OnInvitationSent -= HandleOnInvitationSent;

            _invitationService = null;

            Initialized = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the event when a join intent (e.g., an invitation acceptance) is received.
        /// </summary>
        /// <param name="invitation">The invitation that triggered the event.</param>
        static void HandleOnInGameIntent(Invitation invitation)
        {
            OnInGameInvitationAccepted?.Invoke(invitation);
        }

        /// <summary>
        /// Handles the event when one or more invitations are successfully sent.
        /// </summary>
        /// <param name="invitation">The array of invitations that were sent.</param>
        static void HandleOnInvitationSent(Invitation[] invitation)
        {
            OnInGameInvitationSent?.Invoke(invitation);
        }

        #endregion
    }
}
