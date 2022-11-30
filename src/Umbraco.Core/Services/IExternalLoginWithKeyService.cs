using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services;

public interface IExternalLoginWithKeyService : IService
{
    /// <summary>
    ///     Returns all user logins assigned
    /// </summary>
    IEnumerable<IIdentityUserLogin> GetExternalLogins(Guid userOrMemberKey);

    /// <summary>
    ///     Returns all user login tokens assigned
    /// </summary>
    IEnumerable<IIdentityUserToken> GetExternalLoginTokens(Guid userOrMemberKey);

    /// <summary>
    ///     Returns all logins matching the login info - generally there should only be one but in some cases
    ///     there might be more than one depending on if an administrator has been editing/removing members
    /// </summary>
    IEnumerable<IIdentityUserLogin> Find(string loginProvider, string providerKey);

    /// <summary>
    ///     Saves the external logins associated with the user
    /// </summary>
    /// <param name="userOrMemberKey">
    ///     The user or member key associated with the logins
    /// </param>
    /// <param name="logins"></param>
    /// <remarks>
    ///     This will replace all external login provider information for the user
    /// </remarks>
    void Save(Guid userOrMemberKey, IEnumerable<IExternalLogin> logins);

    /// <summary>
    ///     Saves the external login tokens associated with the user
    /// </summary>
    /// <param name="userId">
    ///     The user or member key associated with the logins
    /// </param>
    /// <param name="tokens"></param>
    /// <remarks>
    ///     This will replace all external login tokens for the user
    /// </remarks>
    void Save(Guid userOrMemberKey, IEnumerable<IExternalLoginToken> tokens);

    /// <summary>
    ///     Deletes all user logins - normally used when a member is deleted
    /// </summary>
    void DeleteUserLogins(Guid userOrMemberKey);
}
