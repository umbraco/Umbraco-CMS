namespace Umbraco.Cms.Web.BackOffice.Security;

/// <inheritdoc />
public class DefaultBackOfficeTwoFactorOptions : IBackOfficeTwoFactorOptions
{
    /// <summary>
    ///     Gets the view to display for the two factor challenge.
    /// </summary>
    /// <remarks>
    ///     Defaults to null to let the login screen show its default.
    /// </remarks>
    /// <param name="username">The username of the logged-in user.</param>
    /// <returns>A null value to let the login screen show its default.</returns>
    public string? GetTwoFactorView(string username) => null;
}
