using Umbraco.Cms.Core.Models;

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

    /// <summary>
    ///     Gets or sets the look to use for the login button.
    ///     See the UUI documentation for more details: https://uui.umbraco.com/?path=/story/uui-button--looks-and-colors.
    /// </summary>
    public UuiButtonLook ButtonLook { get; set; } = UuiButtonLook.Outline;

    /// <summary>
    ///     Gets or sets the color to use for the login button.
    ///     See the UUI documentation for more details: https://uui.umbraco.com/?path=/story/uui-button--looks-and-colors.
    /// </summary>
    public UuiButtonColor ButtonColor { get; set; } = UuiButtonColor.Default;

    /// <summary>
    ///     Gets or sets the icon to use for the login button.
    ///     The standard icons of the Backoffice is available.
    /// </summary>
    /// <remarks>
    ///     It is possible to add custom icons to your provider by adding the icons to the
    ///     <c>~/App_Plugins/{providerAlias}/icons</c> folder as SVG files. The icon name should be the same as the file name.
    /// </remarks>
    public string Icon { get; set; } = "icon-user";

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
