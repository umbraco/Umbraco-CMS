using System;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Options used to configure auto-linking external OAuth providers
    /// </summary>
    public class ExternalSignInAutoLinkOptions
    {
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
            _defaultUserGroups = defaultUserGroups ?? new[] { Constants.Security.EditorGroupAlias };
            _autoLinkExternalAccount = autoLinkExternalAccount;
            _defaultCulture = defaultCulture ?? /*Current.Configs.Global().DefaultUILanguage TODO reintroduce config value*/ "en-US";
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


        /// <summary>
        /// The default User group aliases to use for auto-linking users
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        public string[] GetDefaultUserGroups(IUmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _defaultUserGroups;
        }

        private readonly bool _autoLinkExternalAccount;

        /// <summary>
        /// For private external auth providers such as Active Directory, which when set to true will automatically
        /// create a local user if the external provider login was successful.
        ///
        /// For public auth providers this should always be false!!!
        /// </summary>
        public bool ShouldAutoLinkExternalAccount(IUmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _autoLinkExternalAccount;
        }

        private readonly string _defaultCulture;

        /// <summary>
        /// The default Culture to use for auto-linking users
        /// </summary>
        public string GetDefaultCulture(IUmbracoContext umbracoContext, ExternalLoginInfo loginInfo)
        {
            return _defaultCulture;
        }
    }
}
