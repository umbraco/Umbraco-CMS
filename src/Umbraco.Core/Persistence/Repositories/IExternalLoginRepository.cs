using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IExternalLoginRepository : IReadWriteQueryRepository<int, IIdentityUserLogin>,
    IQueryRepository<IIdentityUserToken>
{
    /// <summary>
    ///     Replaces all external login providers for the user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="logins"></param>
    [Obsolete("Use method that takes guid as param from IExternalLoginWithKeyRepository")]
    void Save(int userId, IEnumerable<IExternalLogin> logins);

    /// <summary>
    ///     Replaces all external login provider tokens for the providers specified for the user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tokens"></param>
    [Obsolete("Use method that takes guid as param from IExternalLoginWithKeyRepository")]
    void Save(int userId, IEnumerable<IExternalLoginToken> tokens);

    [Obsolete("Use method that takes guid as param from IExternalLoginWithKeyRepository")]
    void DeleteUserLogins(int memberId);
}
