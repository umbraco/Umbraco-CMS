namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Options used to configure back office external login providers
/// </summary>
public class BackOfficeExternalLoginProviderOptions
{
    public BackOfficeExternalLoginProviderOptions(
        ExternalSignInAutoLinkOptions? autoLinkOptions = null,
        bool denyLocalLogin = false)
    {
        AutoLinkOptions = autoLinkOptions ?? new ExternalSignInAutoLinkOptions();
        DenyLocalLogin = denyLocalLogin;
    }

    public BackOfficeExternalLoginProviderOptions()
    {
    }

    /// <summary>
    ///     Gets or sets options used to control how users can be auto-linked/created/updated based on the external login provider
    /// </summary>
    public ExternalSignInAutoLinkOptions AutoLinkOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether when set to true will disable all local user login functionality.
    ///     This is useful if you want to force users to login with an external provider.
    /// </summary>
    public bool DenyLocalLogin { get; set; }
}
