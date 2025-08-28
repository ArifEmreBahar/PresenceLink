using System.Threading.Tasks;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Represents the type of invitation associated with the current presence.
    /// </summary>
    public enum PresenceType
    {
        /// <summary>
        /// The presence type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// A normal presence type, typically used for default states.
        /// </summary>
        Normal,

        /// <summary>
        /// The presence type indicates the user is in-game.
        /// </summary>
        InGame,

        /// <summary>
        /// The presence type indicates the user is out of play.
        /// </summary>
        OutOfPlay,
    }

    /// <summary>
    /// Represents a base class for managing group presence across different platforms.
    /// Derived classes are expected to implement the <see cref="Set"/> method for their platform-specific logic.
    /// </summary>
    public abstract class GroupPresence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPresence"/> class with the specified owner ID.
        /// </summary>
        /// <param name="ownerId">The ID of the presence owner, typically a PlayFab ID.</param>
        public GroupPresence(string ownerId)
        {
            OwnerId = ownerId;
        }

        #region Fields

        /// <summary>
        /// Gets the presence key, which can be overridden by derived classes.
        /// </summary>
        public virtual string PRESENCE_KEY => "";

        /// <summary>
        /// Constant token used as a prefix for presence strings.
        /// </summary>
        public const string PRESENCE_TOKEN = "AEB";

        /// <summary>
        /// Separator used to construct or parse presence strings.
        /// </summary>
        protected const string SEPARATOR = "#";

        #endregion


        #region Properties

        /// <summary>
        /// The owner ID associated with the presence, typically representing the session owner or creator.
        /// </summary>
        public string OwnerId { get; private set; }

        /// <summary>
        /// The current ID associated with the presence, typically representing the PlayFab ID.
        /// </summary>
        public string Current { get; protected set; }

        /// <summary>
        /// Indicates whether the current presence is joinable.
        /// Derived classes can set this property as needed.
        /// </summary>
        public bool IsJoinable { get; protected set; }

        /// <summary>
        /// The type of presence associated with this instance.
        /// Derived classes can set this based on their initialization logic.
        /// </summary>
        public PresenceType PresenceType { get; protected set; } = PresenceType.Unknown;

        #endregion

        #region Public

        /// <summary>
        /// Constructs a presence string using the provided owner ID.
        /// </summary>
        /// <param name="presenceOwnerId">The ID of the presence owner, such as a PlayFab ID.</param>
        /// <returns>A platform-agnostic presence string.</returns>
        public string GetPresence(string presenceOwnerId) => string.IsNullOrEmpty(presenceOwnerId)
            ? PRESENCE_TOKEN
            : PRESENCE_TOKEN + SEPARATOR + presenceOwnerId;

        /// <summary>
        /// Checks if a given string contains the presence token.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns>True if the string contains the presence token; otherwise, false.</returns>
        public bool HasPresenceToken(string input)
        {
            return !string.IsNullOrEmpty(input) && input.Contains(PRESENCE_TOKEN);
        }

        /// <summary>
        /// Attempts to extract a PlayFab ID from a previously constructed presence string.
        /// </summary>
        /// <param name="presenceString">The presence string to parse.</param>
        /// <param name="playfabId">The extracted PlayFab ID, if any.</param>
        /// <returns>True if a PlayFab ID was successfully extracted; otherwise, false.</returns>
        public bool TryExtractPlayFabId(string presenceString, out string playfabId)
        {
            playfabId = null;

            if (string.IsNullOrEmpty(presenceString))
                return false;

            // Expecting a format like "AEB#<playfabId>"
            string[] parts = presenceString.Split(SEPARATOR);
            if (parts.Length == 2 && parts[0] == PRESENCE_TOKEN && !string.IsNullOrEmpty(parts[1]))
            {
                playfabId = parts[1];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the group presence for the specified target PlayFab ID and presence type.
        /// Derived classes should implement the platform-specific logic here.
        /// </summary>
        /// <param name="targetPlayfabId">The PlayFab ID of the target user or session owner.</param>
        /// <param name="invitationType">The type of invitation or session initiation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task Set(string targetPlayfabId, PresenceType invitationType);

        /// <summary>
        /// Sets the group presence back to the owner.
        /// </summary>
        /// <remarks>
        /// If no owner ID was provided during construction, this method will perform no action.
        /// </remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetMine()
        {
            if (OwnerId == null)
                return Task.CompletedTask;

            return Set(OwnerId, PresenceType.Normal);
        }

        #endregion
    }
}
