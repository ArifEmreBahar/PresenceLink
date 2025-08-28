using UnityEngine;

namespace AEB.Integration.Invitation
{
    public class InvitationManagerInitializator : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Determines whether the <see cref="InvitationManager"/> should be initialized automatically when the game starts.
        /// Set to <c>true</c> by default to enable automatic initialization.
        /// </summary>
        public bool InitializeOnStart = true;

        #endregion

        #region Unity

        public void Start()
        {
            if (!InitializeOnStart) return;

            Initialize();
        }

        #endregion

        #region Public

        /// <summary>
        /// Initializes the <see cref="InvitationManager"/> with the appropriate authentication provider.
        /// This method ensures that initialization occurs only once and prevents redundant sign-in attempts.
        /// </summary>
        public void Initialize()
        {
            if (!InvitationManager.Initialized)
                InvitationManager.Initialize();
        }

        #endregion
    }
}
