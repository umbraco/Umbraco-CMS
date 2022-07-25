namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Used to persist an external login token for a user
/// </summary>
public interface IExternalLoginToken
{
    /// <summary>
    ///     Gets the login provider
    /// </summary>
    string LoginProvider { get; }

    /// <summary>
    ///     Gets the name of the token
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the value of the token
    /// </summary>
    string Value { get; }
}
