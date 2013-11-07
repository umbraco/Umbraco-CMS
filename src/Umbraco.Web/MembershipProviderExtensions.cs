using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Umbraco.Core.Security;

namespace Umbraco.Web
{
    internal static class MembershipProviderExtensions
    {
        /// <summary>
        /// Returns the configuration of the membership provider used to configure change password editors
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetConfiguration(
            this MembershipProvider membershipProvider)
        {
            var baseProvider = membershipProvider as MembershipProviderBase;
            
            return new Dictionary<string, object>
                {
                    {"minPasswordLength", membershipProvider.MinRequiredPasswordLength},
                    {"enableReset", membershipProvider.EnablePasswordReset},
                    {"enablePasswordRetrieval", membershipProvider.EnablePasswordRetrieval},
                    {"requiresQuestionAnswer", membershipProvider.RequiresQuestionAndAnswer},
                    {"allowManuallyChangingPassword", baseProvider != null && baseProvider.AllowManuallyChangingPassword}
                    //TODO: Inject the other parameters in here to change the behavior of this control - based on the membership provider settings.
                };
        } 

    }
}
