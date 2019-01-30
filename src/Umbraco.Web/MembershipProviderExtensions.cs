using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web
{
    internal static class MembershipProviderExtensions
    {
        /// <summary>
        /// Returns the configuration of the membership provider used to configure change password editors
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <param name="userService"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetConfiguration(
            this MembershipProvider membershipProvider, IUserService userService)
        {
            var baseProvider = membershipProvider as MembershipProviderBase;

            var canReset = membershipProvider.CanResetPassword(userService);

            return new Dictionary<string, object>
                {
                    {"minPasswordLength", membershipProvider.MinRequiredPasswordLength},
                    {"enableReset", canReset},
                    {"enablePasswordRetrieval", membershipProvider.EnablePasswordRetrieval},
                    {"requiresQuestionAnswer", membershipProvider.RequiresQuestionAndAnswer},
                    {"allowManuallyChangingPassword", baseProvider != null && baseProvider.AllowManuallyChangingPassword},
                    {"minNonAlphaNumericChars", membershipProvider.MinRequiredNonAlphanumericCharacters}
                    // TODO: Inject the other parameters in here to change the behavior of this control - based on the membership provider settings.
                };
        }

    }
}
