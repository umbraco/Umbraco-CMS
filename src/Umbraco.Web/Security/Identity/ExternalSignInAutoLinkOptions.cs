using System;
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
    public sealed class ExternalSignInAutoLinkOptions
    {
        public ExternalSignInAutoLinkOptions(
            bool autoLinkExternalAccount = false,
            string defaultUserType = "editor", 
            string[] defaultAllowedSections = null, 
            string defaultCulture = null)
        {
            Mandate.ParameterNotNullOrEmpty(defaultUserType, "defaultUserType");

            _defaultUserType = defaultUserType;
            _defaultAllowedSections = defaultAllowedSections ?? new[] { "content", "media" };
            _autoLinkExternalAccount = autoLinkExternalAccount;
            _defaultCulture = defaultCulture ?? GlobalSettings.DefaultUILanguage;
        }

        private readonly string _defaultUserType;

        /// <summary>
        /// A callback executed during account auto-linking and before the user is persisted
        /// </summary>
        public Action<BackOfficeIdentityUser, ExternalLoginInfo> OnAutoLinking { get; set; }

        /// <summary>
        /// The default User Type alias to use for auto-linking users
        /// </summary>
        public string GetDefaultUserType(UmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _defaultUserType;
        }

        private readonly string[] _defaultAllowedSections;

        /// <summary>
        /// The default allowed sections to use for auto-linking users
        /// </summary>
        public string[] GetDefaultAllowedSections(UmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _defaultAllowedSections;
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