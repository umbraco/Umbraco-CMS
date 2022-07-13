using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Used to store the external login info
/// </summary>
[Obsolete("Use IExternalLoginServiceWithKey. This will be removed in Umbraco 10")]
public interface IExternalLoginService : IService
{
    /// <summary>
    ///     Returns all user logins assigned
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    IEnumerable<IIdentityUserLogin> GetExternalLogins(int userId);

    /// <summary>
    ///     Returns all user login tokens assigned
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    IEnumerable<IIdentityUserToken> GetExternalLoginTokens(int userId);

    /// <summary>
    ///     Returns all logins matching the login info - generally there should only be one but in some cases
    ///     there might be more than one depending on if an administrator has been editing/removing members
    /// </summary>
    /// <param name="loginProvider"></param>
    /// <param name="providerKey"></param>
    /// <returns></returns>
    IEnumerable<IIdentityUserLogin> Find(string loginProvider, string providerKey);

    /// <summary>
    ///     Saves the external logins associated with the user
    /// </summary>
    /// <param name="userId">
    ///     The user associated with the logins
    /// </param>
    /// <param name="logins"></param>
    /// <remarks>
    ///     This will replace all external login provider information for the user
    /// </remarks>
    void Save(int userId, IEnumerable<IExternalLogin> logins);

    /// <summary>
    ///     Saves the external login tokens associated with the user
    /// </summary>
    /// <param name="userId">
    ///     The user associated with the tokens
    /// </param>
    /// <param name="tokens"></param>
    /// <remarks>
    ///     This will replace all external login tokens for the user
    /// </remarks>
    void Save(int userId, IEnumerable<IExternalLoginToken> tokens);

    /// <summary>
    ///     Deletes all user logins - normally used when a member is deleted
    /// </summary>
    /// <param name="userId"></param>
    void DeleteUserLogins(int userId);
}
