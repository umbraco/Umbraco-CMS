using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

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
            return new Dictionary<string, object>
                {
                    {"minPasswordLength", membershipProvider.MinRequiredPasswordLength},
                    {"enableReset", membershipProvider.EnablePasswordReset},
                    {"enablePasswordRetrieval", membershipProvider.EnablePasswordRetrieval},
                    {"requiresQuestionAnswer", membershipProvider.RequiresQuestionAndAnswer}
                    //TODO: Inject the other parameters in here to change the behavior of this control - based on the membership provider settings.
                };
        } 

    }
}
