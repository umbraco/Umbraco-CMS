using System;
using System.ComponentModel;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Options used to configure auto-linking external OAuth providers
    /// </summary>
    public class ExternalSignInAutoLinkOptions
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the overload specifying user groups instead and defaultAllowedSections now serves no purpose")]
        public ExternalSignInAutoLinkOptions(
            bool autoLinkExternalAccount = false,
            string defaultUserType = "editor", 
            string[] defaultAllowedSections = null, 
            string defaultCulture = null)
        {
            Mandate.ParameterNotNullOrEmpty(defaultUserType, "defaultUserType");

            _defaultUserGroups = new[] {defaultUserType};
            _autoLinkExternalAccount = autoLinkExternalAccount;
            _defaultCulture = defaultCulture ?? GlobalSettings.DefaultUILanguage;
        }

        /// <summary>
        /// Creates a new <see cref="ExternalSignInAutoLinkOptions"/> instance
        /// </summary>
        /// <param name="autoLinkExternalAccount"></param>
        /// <param name="defaultUserGroups">If null, the default will be the 'editor' group</param>
        /// <param name="defaultCulture"></param>
        public ExternalSignInAutoLinkOptions(
            bool autoLinkExternalAccount = false,
            string[] defaultUserGroups = null,
            string defaultCulture = null)
        {
            _defaultUserGroups = defaultUserGroups ?? new[] { "editor" };
            _autoLinkExternalAccount = autoLinkExternalAccount;
            _defaultCulture = defaultCulture ?? GlobalSettings.DefaultUILanguage;
        }

        private readonly string[] _defaultUserGroups;

        /// <summary>
        /// A callback executed during account auto-linking and before the user is persisted
        /// </summary>
        public Action<BackOfficeIdentityUser, ExternalLoginInfo> OnAutoLinking { get; set; }

        /// <summary>
        /// A callback executed during every time a user authenticates using an external login.
        /// returns a boolean indicating if sign in should continue or not.
        /// </summary>
        public Func<BackOfficeIdentityUser, ExternalLoginInfo, bool> OnExternalLogin { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the overload specifying user groups instead")]
        public string GetDefaultUserType(UmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _defaultUserGroups.Length == 0 ? "editor" : _defaultUserGroups[0];
        }

        /// <summary>
        /// The default User group aliases to use for auto-linking users
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        public string[] GetDefaultUserGroups(UmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _defaultUserGroups;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("The default sections are based on the default user group, this is no longer used")]
        public string[] GetDefaultAllowedSections(UmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return new string[0];
        }

        private readonly bool _autoLinkExternalAccount;

        /// <summary>
        /// For private external auth providers such as Active Directory, which when set to true will automatically
        /// create a local user if the external provider login was successful.
        /// 
        /// For public auth providers this should always be false!!!
        /// </summary>
        public bool ShouldAutoLinkExternalAccount(UmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _autoLinkExternalAccount;
        }
        
        private readonly string _defaultCulture;
       
        /// <summary>
        /// The default Culture to use for auto-linking users
        /// </summary>
        public string GetDefaultCulture(UmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _defaultCulture;
        }
    }
}