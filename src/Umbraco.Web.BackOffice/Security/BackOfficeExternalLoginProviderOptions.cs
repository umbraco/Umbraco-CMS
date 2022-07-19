namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Options used to configure back office external login providers
/// </summary>
public class BackOfficeExternalLoginProviderOptions
{
    public BackOfficeExternalLoginProviderOptions(
        string buttonStyle,
        string icon,
        ExternalSignInAutoLinkOptions? autoLinkOptions = null,
        bool denyLocalLogin = false,
        bool autoRedirectLoginToExternalProvider = false,
        string? customBackOfficeView = null)
    {
        ButtonStyle = buttonStyle;
        Icon = icon;
        AutoLinkOptions = autoLinkOptions ?? new ExternalSignInAutoLinkOptions();
        DenyLocalLogin = denyLocalLogin;
        AutoRedirectLoginToExternalProvider = autoRedirectLoginToExternalProvider;
        CustomBackOfficeView = customBackOfficeView;
    }

    public BackOfficeExternalLoginProviderOptions()
    {
    }

    public string ButtonStyle { get; set; } = "btn-openid";

    public string Icon { get; set; } = "fa fa-user";

    /// <summary>
    ///     Options used to control how users can be auto-linked/created/updated based on the external login provider
    /// </summary>
    public ExternalSignInAutoLinkOptions AutoLinkOptions { get; set; } = new();

    /// <summary>
    ///     When set to true will disable all local user login functionality
    /// </summary>
    public bool DenyLocalLogin { get; set; }

    /// <summary>
    ///     When specified this will automatically redirect to the OAuth login provider instead of prompting the user to click
    ///     on the OAuth button first.
    /// </summary>
    /// <remarks>
    ///     This is generally used in conjunction with <see cref="DenyLocalLogin" />. If more than one OAuth provider specifies
    ///     this, the last registered
    ///     provider's redirect settings will win.
    /// </remarks>
    public bool AutoRedirectLoginToExternalProvider { get; set; }

    /// <summary>
    ///     A virtual path to a custom angular view that is used to replace the entire UI that renders the external login
    ///     button that the user interacts with
    /// </summary>
    /// <remarks>
    ///     If this view is specified it is 100% up to the user to render the html responsible for rendering the link/un-link
    ///     buttons along with showing any errors
    ///     that occur. This overrides what Umbraco normally does by default.
    /// </remarks>
    public string? CustomBackOfficeView { get; set; }
}
