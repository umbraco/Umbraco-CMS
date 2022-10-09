namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Used to persist external login data for a user
/// </summary>
public interface IExternalLogin
{
    /// <summary>
    ///     Gets the login provider
    /// </summary>
    string LoginProvider { get; }

    /// <summary>
    ///     Gets the provider key
    /// </summary>
    string ProviderKey { get; }

    /// <summary>
    ///     Gets the user data
    /// </summary>
    string? UserData { get; }
}
