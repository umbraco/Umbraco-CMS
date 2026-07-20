namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Options used to configure back office external login providers
/// </summary>
public class BackOfficeExternalLoginProviderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeExternalLoginProviderOptions"/> class.
    /// </summary>
    /// <param name="autoLinkOptions">Options for automatically linking external sign-in accounts. Defaults to <c>null</c>.</param>
    /// <param name="denyLocalLogin">If set to <c>true</c>, local login is denied. Defaults to <c>false</c>.</param>
    public BackOfficeExternalLoginProviderOptions(
        ExternalSignInAutoLinkOptions? autoLinkOptions = null,
        bool denyLocalLogin = false)
    {
        AutoLinkOptions = autoLinkOptions ?? new ExternalSignInAutoLinkOptions();
        DenyLocalLogin = denyLocalLogin;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BackOfficeExternalLoginProviderOptions"/>.
    /// </summary>
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
