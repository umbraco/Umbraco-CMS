namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Options used as named options for 2fa providers
/// </summary>
public class TwoFactorLoginViewOptions
{
    /// <summary>
    ///     Gets or sets the path of the view to show when setting up this 2fa provider
    /// </summary>
    public string? SetupViewPath { get; set; }
}
