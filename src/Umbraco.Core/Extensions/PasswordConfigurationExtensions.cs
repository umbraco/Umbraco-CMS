// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions;

public static class PasswordConfigurationExtensions
{
    /// <summary>
    ///     Returns the configuration of the membership provider used to configure change password editors
    /// </summary>
    /// <param name="passwordConfiguration"></param>
    /// <param name="allowManuallyChangingPassword"></param>
    /// <returns></returns>
    public static IDictionary<string, object> GetConfiguration(
        this IPasswordConfiguration passwordConfiguration,
        bool allowManuallyChangingPassword = false) =>
        new Dictionary<string, object>
        {
            { "minPasswordLength", passwordConfiguration.RequiredLength },

            // TODO: This doesn't make a ton of sense with asp.net identity and also there's a bunch of other settings
            // that we can consider with IPasswordConfiguration, but these are currently still based on how membership providers worked.
            { "minNonAlphaNumericChars", passwordConfiguration.GetMinNonAlphaNumericChars() },

            // A flag to indicate if the current password box should be shown or not, only a user that has access to change other user/member passwords
            // doesn't have to specify the current password for the user/member. A user changing their own password must specify their current password.
            { "allowManuallyChangingPassword", allowManuallyChangingPassword },
        };

    public static int GetMinNonAlphaNumericChars(this IPasswordConfiguration passwordConfiguration) =>
        passwordConfiguration.RequireNonLetterOrDigit ? 2 : 0;
}
