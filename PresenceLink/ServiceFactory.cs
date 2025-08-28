using System;
using UnityEngine;

namespace AEB.Integration.Invitation
{
    /// <summary>
    /// Factory class responsible for creating instances of services based on the current platform.
    /// </summary>
    public class ServiceFactory
    {
        #region Fields

        private const string OculusInvitationServiceTypeName = "AEB.PlayFab.Authorization.OculusInvitationService";
        private const string SteamInvitationServiceTypeName = "AEB.PlayFab.Authorization.SteamInvitationService";
        private const string OculusAccountInfoServiceTypeName = "AEB.PlayFab.Authorization.OculusAccountInfoService";
        private const string SteamAccountInfoServiceTypeName = "AEB.PlayFab.Authorization.SteamAccountInfoService";

        #endregion

        #region Public

        public IInvitationService CreateInvitationService(string ownerId)
        {
            IInvitationService service = new DefaultInvitationService();

            //#if         UNITY_EDITOR
            //            //service = CreateServiceViaReflection<IInvitationService>(nameof(DefaultInvitationService));
            //#elif       UNITY_STANDALONE_WIN
            //            service = CreateServiceViaReflection<IInvitationService>(SteamInvitationServiceTypeName);
            //#elif       UNITY_ANDROID
            //            //service = CreateServiceViaReflection<IInvitationService>(OculusInvitationServiceTypeName);
            //#else
            //            Debug.LogWarning("No account Invitation service defined for this platform. Using default service.");
            //#endif
            return service ?? new DefaultInvitationService();
        }

        #endregion

        #region Private

        /// <summary>
        /// Generic method to create a service via reflection.
        /// </summary>
        static T CreateServiceViaReflection<T>(string typeName) where T : class
        {
            Type serviceType = Type.GetType(typeName);

            if (serviceType != null)
            {
                try
                {
                    object instance = Activator.CreateInstance(serviceType);
                    if (instance is T serviceInstance)
                    {
                        Debug.Log($"Successfully created instance of type: {typeName}");
                        return serviceInstance;
                    }
                    else
                    {
                        Debug.LogError($"Type {typeName} does not implement {typeof(T)}.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error creating instance of type {typeName}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Type {typeName} not found.");
            }

            return null;
        }

        #endregion
    }
}
