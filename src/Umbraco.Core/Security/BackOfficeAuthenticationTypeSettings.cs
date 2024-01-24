using Umbraco.Cms.Core;

namespace Umbraco.Cms.Core.Security;

public class BackOfficeAuthenticationTypeSettings
{
    public string AuthenticationType { get; set; } = Constants.Security.NewBackOfficeAuthenticationType;
    public string ExternalAuthenticationType { get; set; } = Constants.Security.NewBackOfficeExternalAuthenticationType;
    public string TwoFactorAuthenticationType { get; set; } = Constants.Security.NewBackOfficeTwoFactorAuthenticationType;
    public string TwoFactorRememberMeAuthenticationType { get; set; } = Constants.Security.NewBackOfficeTwoFactorRememberMeAuthenticationType;
}
