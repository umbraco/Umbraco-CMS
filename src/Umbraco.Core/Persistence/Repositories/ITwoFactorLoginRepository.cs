using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="ITwoFactorLogin" /> entities.
/// </summary>
public interface ITwoFactorLoginRepository : IReadRepository<int, ITwoFactorLogin>, IWriteRepository<ITwoFactorLogin>
{
    /// <summary>
    ///     Deletes all two-factor login records for a user or member.
    /// </summary>
    /// <param name="userOrMemberKey">The unique key of the user or member.</param>
    /// <returns><c>true</c> if records were deleted; otherwise, <c>false</c>.</returns>
    Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey);

    /// <summary>
    ///     Deletes two-factor login records for a specific provider for a user or member.
    /// </summary>
    /// <param name="userOrMemberKey">The unique key of the user or member.</param>
    /// <param name="providerName">The name of the provider.</param>
    /// <returns><c>true</c> if records were deleted; otherwise, <c>false</c>.</returns>
    Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey, string providerName);

    /// <summary>
    ///     Gets all two-factor login records for a user or member.
    /// </summary>
    /// <param name="userOrMemberKey">The unique key of the user or member.</param>
    /// <returns>A collection of two-factor login records.</returns>
    Task<IEnumerable<ITwoFactorLogin>> GetByUserOrMemberKeyAsync(Guid userOrMemberKey);
}
