using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web
{
    internal static class PasswordConfigurationExtensions
    {
        /// <summary>
        /// Returns the configuration of the membership provider used to configure change password editors
        /// </summary>
        /// <param name="passwordConfiguration"></param>
        /// <param name="userService"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetConfiguration(
            this IPasswordConfiguration passwordConfiguration)
        {
            return new Dictionary<string, object>
                {
                    {"minPasswordLength", passwordConfiguration.RequiredLength},

                    // TODO: This doesn't make a ton of sense with asp.net identity and also there's a bunch of other settings
                    // that we can consider with IPasswordConfiguration, but these are currently still based on how membership providers worked.
                    {"minNonAlphaNumericChars", passwordConfiguration.RequireNonLetterOrDigit ? 2 : 0},

                    // TODO: These are legacy settings - we will always allow administrators to change another users password if the user
                    // has permission to the user section to edit them. Similarly, when we have ASP.Net identity enabled for members, these legacy settings
                    // will no longer exist and admins will just be able to change a members' password if they have access to the member section to edit them.
                    {"allowManuallyChangingPassword", true},
                    {"enableReset", false}, // TODO: Actually, this is still used for the member editor, see MemberTabsAndPropertiesMapper.GetPasswordConfig, need to remove that eventually
                    {"enablePasswordRetrieval", false},
                    {"requiresQuestionAnswer", false}


                };
        }

    }
}
