using System;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Represents a default implementation of the <see cref="IInvitationService"/> interface.
    /// Note: There is no actual default invitation service. This class should be overridden
    /// by platform-specific services, such as Steam or Oculus.
    /// </summary>
    public class DefaultInvitationService : IInvitationService
    {
        /// <summary>
        /// Gets the <see cref="GroupPresence"/> instance used to manage group presence for the service.
        /// </summary>
        public GroupPresence GroupPresence { get; private set; }

        /// <summary>
        /// Occurs when invitations are sent.
        /// </summary>
        public event Action<Invitation[]> OnInvitationSent;

        /// <summary>
        /// Occurs when a join intent is received from any invitation.
        /// </summary>
        public event Action<Invitation> OnAnyJoinIntent;

        /// <summary>
        /// Occurs when an in-game intent is received from an invitation.
        /// </summary>
        public event Action<Invitation> OnInGameIntent;

        /// <summary>
        /// Occurs when the application is launched by a join intent.
        /// </summary>
        public event Action<Invitation> OnAppLaunchedByJoinIntent;

        /// <summary>
        /// Initializes the service.
        /// </summary>
        public void Initialize() { }

        /// <summary>
        /// Terminates the service, cleaning up resources and unregistering components.
        /// </summary>
        public void Terminate() { }
    }
}
