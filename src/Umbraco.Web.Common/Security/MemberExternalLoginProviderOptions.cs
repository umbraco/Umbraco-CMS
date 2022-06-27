namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     Options used to configure member external login providers
/// </summary>
public class MemberExternalLoginProviderOptions
{
    public MemberExternalLoginProviderOptions(
        MemberExternalSignInAutoLinkOptions? autoLinkOptions = null,
        bool autoRedirectLoginToExternalProvider = false,
        string? customBackOfficeView = null) =>
        AutoLinkOptions = autoLinkOptions ?? new MemberExternalSignInAutoLinkOptions();

    public MemberExternalLoginProviderOptions()
    {
    }

    /// <summary>
    ///     Options used to control how users can be auto-linked/created/updated based on the external login provider
    /// </summary>
    public MemberExternalSignInAutoLinkOptions AutoLinkOptions { get; set; } = new();
}
