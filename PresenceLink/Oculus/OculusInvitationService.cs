using Oculus.Platform;
using Oculus.Platform.Models;
using AEB.PlayFab.Authorization;
using System;
using UnityEngine;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// A service for managing Oculus invitations and group presence.
    /// </summary>
    public class OculusInvitationService : IInvitationService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OculusInvitationService"/> class with the specified owner ID.
        /// </summary>
        /// <param name="ownerId">The ID of the owner for this service.</param>
        public OculusInvitationService(string ownerId)
        {
            GroupPresence = new OculusGroupPresence(ownerId);
        }

        #region Fields

        /// <summary>
        /// Indicates whether the service is initialized.
        /// </summary>
        bool isInitialized;

        #endregion

        #region Properties

        /// <summary>
        /// Manages group presence for the service.
        /// </summary>
        public GroupPresence GroupPresence { get; private set; }

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
        /// Event triggered when invitation is sent.
        /// </summary>
        public event Action<Invitation[]> OnInvitationSent;

        #endregion

        #region Public

        /// <summary>
        /// Initializes the Oculus Invitation Service by setting up necessary callbacks.
        /// </summary>
        public void Initialize()
        {
            if (!AccountManager.Initialized || isInitialized) return;

            Oculus.Platform.GroupPresence.SetJoinIntentReceivedNotificationCallback(HandleJoinIntentReceived);
            Oculus.Platform.GroupPresence.SetInvitationsSentNotificationCallback(HandleInvitationsSent);
            AbuseReport.SetReportButtonPressedNotificationCallback(OnReportButtonIntentNotif);

            GroupPresence.SetMine();

            isInitialized = true;
        }

        /// <summary>
        /// Terminates the Oculus Invitation Service.
        /// </summary>
        public void Terminate() { }

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
        /// <param name="message">The join intent message received from the Oculus SDK.</param>
        void HandleJoinIntentReceived(Message<Oculus.Platform.Models.GroupPresenceJoinIntent> message)
        {
            string presenceVal = message.Data.LobbySessionId;

            if (string.IsNullOrEmpty(presenceVal))                                      return;
            if (!GroupPresence.TryExtractPlayFabId(presenceVal, out var playfabId))     return;
            if (string.IsNullOrEmpty(playfabId) || playfabId == GroupPresence.OwnerId)  return;

            var invitation = new Invitation
            {
                Platform = "Oculus",
                PresenceType = PresenceType.InGame,
                SenderId = playfabId,
                SenderDisplayName = "Unknown",
                InvitationId = message.RequestID.ToString(),
                AdditionalData = message.Data.DeeplinkMessage
            };

            OnAnyJoinIntent?.Invoke(invitation);
            OnInGameIntent?.Invoke(invitation);
        }


        void HandleInvitationsSent(Message<LaunchInvitePanelFlowResult> message)
        {
            if (message.Data == null || message.Data.InvitedUsers == null)
            {
                Debug.LogError("[HandleInvitationsSent] Message data or invited users are null.");
                return;
            }

            //NOTE: If you need invited players implement here and invoke event accordingly.

            OnInvitationSent?.Invoke(null);
        }

        /// <summary>
        /// Handles when the report button is pressed in the Oculus overlay.
        /// </summary>
        /// <param name="message">The message from the Oculus SDK related to the report button.</param>
        void OnReportButtonIntentNotif(Message<string> message)
        {
            if (!message.IsError)
            {
                // Inform SDK that we don't handle the request.
                AbuseReport.ReportRequestHandled(ReportRequestResponse.Unhandled);
            }
        }

        #endregion
    }
}
