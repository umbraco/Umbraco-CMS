using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Repository for external logins with Guid as key, so it can be shared for members and users
/// </summary>
public interface IExternalLoginWithKeyRepository : IReadWriteQueryRepository<int, IIdentityUserLogin>,
    IQueryRepository<IIdentityUserToken>
{
    /// <summary>
    ///     Replaces all external login providers for the user/member key
    /// </summary>
    void Save(Guid userOrMemberKey, IEnumerable<IExternalLogin> logins);

    /// <summary>
    ///     Replaces all external login provider tokens for the providers specified for the user/member key
    /// </summary>
    void Save(Guid userOrMemberKey, IEnumerable<IExternalLoginToken> tokens);

    /// <summary>
    ///     Deletes all external logins for the specified the user/member key
    /// </summary>
    void DeleteUserLogins(Guid userOrMemberKey);
}
