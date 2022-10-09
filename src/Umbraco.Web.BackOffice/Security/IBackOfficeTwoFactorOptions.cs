namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Options used to control 2FA for the Umbraco back office
/// </summary>
public interface IBackOfficeTwoFactorOptions
{
    /// <summary>
    ///     Returns the angular view for handling 2FA interaction
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    string? GetTwoFactorView(string username);
}
