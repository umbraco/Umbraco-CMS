using Umbraco.Cms.Core;

namespace Umbraco.Cms.Core.Security;

public class BackOfficeAuthenticationTypeSettings
{
    public string AuthenticationType { get; set; } = Constants.Security.BackOfficeAuthenticationType;
    public string ExternalAuthenticationType { get; set; } = Constants.Security.BackOfficeExternalAuthenticationType;
    public string TwoFactorAuthenticationType { get; set; } = Constants.Security.BackOfficeTwoFactorAuthenticationType;
    public string TwoFactorRememberMeAuthenticationType { get; set; } = Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType;
}
