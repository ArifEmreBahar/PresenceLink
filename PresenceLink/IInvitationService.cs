using System;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Represents an invitation from another user, containing details about the sender and the platform.
    /// </summary>
    public class Invitation
    {
        /// <summary>
        /// Gets or sets the platform where the invitation originated (e.g., "Oculus").
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sender of the invitation.
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// Gets or sets the type of presence associated with the invitation.
        /// </summary>
        public PresenceType PresenceType { get; set; }

        /// <summary>
        /// Gets or sets the display name of the sender of the invitation.
        /// </summary>
        public string SenderDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the invitation.
        /// </summary>
        public string InvitationId { get; set; }

        /// <summary>
        /// Gets or sets additional data related to the invitation, if any.
        /// </summary>
        public string AdditionalData { get; set; }
    }

    /// <summary>
    /// Interface for managing user invitations across platforms, providing events and methods for invitation handling and group presence.
    /// </summary>
    public interface IInvitationService
    {
        /// <summary>
        /// Manages group presence for the service.
        /// </summary>
        GroupPresence GroupPresence { get; }

        /// <summary>
        /// Triggered when invitations are sent.
        /// </summary>
        event Action<Invitation[]> OnInvitationSent;

        /// <summary>
        /// Triggered on any join intent (e.g., invitation acceptance).
        /// </summary>
        event Action<Invitation> OnAnyJoinIntent;

        /// <summary>
        /// Triggered when a join intent occurs during an active session.
        /// </summary>
        event Action<Invitation> OnInGameIntent;

        /// <summary>
        /// Triggered when the app launches due to a join intent.
        /// </summary>
        event Action<Invitation> OnAppLaunchedByJoinIntent;

        /// <summary>
        /// Initializes the invitation service.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Terminates the invitation service.
        /// </summary>
        void Terminate();
    }
}
