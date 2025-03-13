namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Options used to control 2FA for the Umbraco back office.
/// </summary>
public interface IBackOfficeTwoFactorOptions
{
    /// <summary>
    ///     Returns the path to a JavaScript module to handle 2FA interaction.
    /// </summary>
    /// <param name="username">The username of the logged-in user.</param>
    /// <returns>Returns the path to a JavaScript module</returns>
    string? GetTwoFactorView(string username);
}
