using Photon.Pun;
using AEB.Menu.Main;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Manages the interaction between the invitation system and the main menu,
    /// specifically handling in-game invitation acceptance and sending logic.
    /// </summary>
    public class MainInvitationSocket : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Reference to the main menu's state machine for managing menu states.
        /// </summary>
        [SerializeField] MenuStateMachine _mainMenu;

        /// <summary>
        /// Flag to prevent concurrent attempts to join or switch rooms.
        /// </summary>
        bool _attempting = false;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when there is no room available for the inviter.
        /// </summary>
        public event Action<Invitation> OnNoInviterRoom;

        /// <summary>
        /// Event triggered when joining an inviter's room fails.
        /// </summary>
        public event Action<Invitation> OnJoinInvitorRoomFailed;

        /// <summary>
        /// Event triggered when an invitation is sent but no room has been created.
        /// </summary>
        public event Action<Invitation> OnNoRoomInvitationSent;

        #endregion

        #region Unity

        void OnEnable()
        {
            InvitationManager.OnInGameInvitationAccepted += HandleOnInGameInvitationAccepted;
            InvitationManager.OnInGameInvitationSent += HandleOnInGameInvitationSent;
            _mainMenu.onPlayerLeftRoom += HandleOnPlayerLeftRoom;
            _mainMenu.onLeftRoom += HandleOnLeftRoom;
            _mainMenu.onJoinRoomFailed += HandleOnJoinRoomFailed;
        }

        void OnDisable()
        {
            InvitationManager.OnInGameInvitationAccepted -= HandleOnInGameInvitationAccepted;
            InvitationManager.OnInGameInvitationSent -= HandleOnInGameInvitationSent;
            _mainMenu.onPlayerLeftRoom -= HandleOnPlayerLeftRoom;
            _mainMenu.onLeftRoom -= HandleOnLeftRoom;
        }

        #endregion

        #region Private

        /// <summary>
        /// Handles the logic when an in-game invitation is sent.
        /// </summary>
        /// <param name="invitation">The invitation containing details of the sender.</param>
        void HandleOnInGameInvitationSent(Invitation[] invitation)
        {
            // NOTE: Currently, we don't do anything with invited players, so it's just null. Once implemented, remove the commented-out part.

            //if (invitation == null)
            //{
            //    Debug.LogError("[MainInvitationSocket] Invalid invitation or sender ID.");
            //    return;
            //}

            if (!_mainMenu.InRoom)
            {
                Invitation noRoomMine = new();
                noRoomMine.AdditionalData = "Invitation sent. Please create a room before the invited player accepts.";
                OnNoRoomInvitationSent?.Invoke(noRoomMine);
            }
        }

        /// <summary>
        /// Handles the logic for when an in-game invitation is accepted.
        /// Retrieves the room name and transitions the player to the appropriate room state.
        /// </summary>
        /// <param name="invitation">The invitation containing details of the sender.</param>
        async void HandleOnInGameInvitationAccepted(Invitation invitation)
        {
            if (_attempting) return;
            _attempting = true;

            if (invitation == null || string.IsNullOrEmpty(invitation.SenderId)) return;
            InvitationManager.SetPresence(invitation);

            string roomName = default;
            int attepmt = 7;
            int waitTime = 1000;

            while (attepmt >= 0)
            {
                roomName = await _mainMenu.GetRoomName(invitation.SenderId);
                if (string.IsNullOrEmpty(roomName)) { 
                    await Task.Delay(waitTime);
                }
                else break;

                attepmt--;
            }

            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Name == roomName) return;

            if (string.IsNullOrEmpty(roomName))
            {
                InvitationManager.SetPresenceMine();
                _attempting = false;
                OnNoInviterRoom?.Invoke(invitation);
                return;
            }

            Action switchToRoomState = null;
            switchToRoomState = () =>
            {
                if (_mainMenu.InRoom &&
                    _mainMenu.CurrentState != null &&
                    _mainMenu.CurrentState.StateKey.Equals(MenuStateMachine.EMenuState.QuickMatch))
                {
                    _mainMenu.SwicthToRoomState();
                }

                _mainMenu.onJoinedRoom -= switchToRoomState;
                _mainMenu.onJoinRoomFailed -= switchToRoomState;

                _attempting = false;
            };

            _mainMenu.onJoinedRoom += switchToRoomState;
            _mainMenu.onJoinRoomFailed += switchToRoomState;

            _mainMenu.JoinRoom(roomName);
        }


        /// <summary>
        /// Handles the event when a player leaves the room.
        /// </summary>
        /// <param name="player">The player who left the room.</param>
        void HandleOnPlayerLeftRoom(global::Photon.Realtime.Player player)
        {
            InvitationManager.SetPresenceMine();
        }


        /// <summary>
        /// Handles the event when the player leaves the room.
        /// Updates the player's presence status to their own current status.
        void HandleOnLeftRoom()
        {
            InvitationManager.SetPresenceMine();
        }

        /// <summary>
        /// Handles the logic when joining a room fails.
        /// </summary>
        /// <param name="message">The error message describing the failure.</param>
        void HandleOnJoinRoomFailed()
        {
            InvitationManager.SetPresenceMine();
            OnJoinInvitorRoomFailed?.Invoke(new());
        }

        #endregion
    }
}
