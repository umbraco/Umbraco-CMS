using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Options used to configure back office external login providers
/// </summary>
public class BackOfficeExternalLoginProviderOptions
{
    private string _buttonStyle = string.Empty;

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

    /// <summary>
    ///     Gets or sets the style of the login button.
    /// </summary>
    /// <remarks>
    ///     The default look is an outlined button, which has been optimized for the login screen.
    /// </remarks>
    [Obsolete("This is no longer used and will be removed in V15. Please set the ButtonLook and ButtonColor properties instead.")]
    public string ButtonStyle
    {
        get => _buttonStyle;
        set
        {
            _buttonStyle = value;

            // Map cases from buttons.less
            switch (value.ToLowerInvariant())
            {
                case "btn-primary":
                    ButtonColor = UuiButtonColor.Default;
                    ButtonLook = UuiButtonLook.Primary;
                    break;
                case "btn-warning":
                    ButtonColor = UuiButtonColor.Warning;
                    ButtonLook = UuiButtonLook.Primary;
                    break;
                case "btn-danger":
                    ButtonColor = UuiButtonColor.Danger;
                    ButtonLook = UuiButtonLook.Primary;
                    break;
                case "btn-success":
                    ButtonColor = UuiButtonColor.Positive;
                    ButtonLook = UuiButtonLook.Primary;
                    break;
                default:
                    ButtonColor = UuiButtonColor.Default;
                    ButtonLook = UuiButtonLook.Outline;
                    break;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the look to use for the login button.
    ///     See the UUI documentation for more details: https://uui.umbraco.com/?path=/story/uui-button--looks-and-colors.
    /// </summary>
    /// <remarks>
    ///     The default value is <see cref="UuiButtonLook.Outline" />, which has been optimized for the login screen.
    /// </remarks>
    public UuiButtonLook ButtonLook { get; set; } = UuiButtonLook.Outline;

    /// <summary>
    ///     Gets or sets the color to use for the login button.
    ///     See the UUI documentation for more details: https://uui.umbraco.com/?path=/story/uui-button--looks-and-colors.
    /// </summary>
    /// <remarks>
    ///     The default value is <see cref="UuiButtonColor.Default" />, which has been optimized for the login screen.
    /// </remarks>
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
    ///     Gets or sets options used to control how users can be auto-linked/created/updated based on the external login provider
    /// </summary>
    public ExternalSignInAutoLinkOptions AutoLinkOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether when set to true will disable all local user login functionality.
    ///     This is useful if you want to force users to login with an external provider.
    /// </summary>
    public bool DenyLocalLogin { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether when specified this will automatically redirect to the OAuth login provider instead of prompting the user to click
    ///     on the OAuth button first.
    /// </summary>
    /// <remarks>
    ///     This is generally used in conjunction with <see cref="DenyLocalLogin" />. If more than one OAuth provider specifies
    ///     this, the last registered
    ///     provider's redirect settings will win.
    /// </remarks>
    public bool AutoRedirectLoginToExternalProvider { get; set; }

    /// <summary>
    ///     Gets or sets a virtual path to a custom JavaScript module that will be rendered in place of the default OAuth login buttons.
    ///     The view can optionally replace the entire login screen if the <see cref="DenyLocalLogin"/> option is set to true.
    /// </summary>
    /// <remarks>
    ///     If this view is specified it is 100% up to the user to render the html responsible for rendering the link/un-link
    ///     buttons along with showing any errors that occur.
    ///     This overrides what Umbraco normally does by default.
    /// </remarks>
    public string? CustomBackOfficeView { get; set; }
}
