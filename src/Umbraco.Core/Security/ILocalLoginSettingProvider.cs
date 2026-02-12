namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A setting provider for local logins.
/// </summary>
/// <remarks>
///     This cannot be an app setting since it's specified by the external login providers.
/// </remarks>
public interface ILocalLoginSettingProvider
{
    /// <summary>
    ///     Determines whether local login should be denied.
    /// </summary>
    /// <returns><c>true</c> if local login is denied; otherwise, <c>false</c>.</returns>
    bool HasDenyLocalLogin();
}
