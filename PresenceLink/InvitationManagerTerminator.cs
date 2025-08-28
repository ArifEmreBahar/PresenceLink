using AEB.Utilities;
using UnityEngine;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Handles account sign-out when the application is about to quit.
    /// </summary>
    public class InvitationManagerTerminator : Singleton<InvitationManagerTerminator>
    {
        #region Unity

        /// <summary>
        /// Subscribes to the Application.wantsToQuit event on start.
        /// </summary>
        void Start()
        {
            Application.wantsToQuit += TerminateInvitation;
        }

        #endregion

        #region Public

        /// <summary>
        /// Signs out the account if AccountManager is initialized and unsubscribes from the quit event.
        /// </summary>
        /// <returns>Returns true to allow the application to quit.</returns>
        public bool TerminateInvitation()
        {
            if (InvitationManager.Initialized)
                InvitationManager.Terminate();

            Application.wantsToQuit -= TerminateInvitation;
            return true;
        }

        #endregion
    }
}

