namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Options used as named options for 2fa providers
/// </summary>
public class TwoFactorLoginViewOptions
{
    /// <summary>
    ///     Gets or sets the path of the view to show when setting up this 2fa provider
    /// </summary>
    [Obsolete("Register the view in the backoffice instead. This will be removed in version 15.")]
    public string? SetupViewPath { get; set; }
}
