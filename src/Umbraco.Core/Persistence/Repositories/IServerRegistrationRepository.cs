using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IServerRegistration" /> entities.
/// </summary>
public interface IServerRegistrationRepository : IReadWriteQueryRepository<int, IServerRegistration>
{
    /// <summary>
    ///     Deactivates servers that have not been active within the specified timeout.
    /// </summary>
    /// <param name="staleTimeout">The timeout after which servers are considered stale.</param>
    void DeactiveStaleServers(TimeSpan staleTimeout);

    /// <summary>
    ///     Clears the server registration cache.
    /// </summary>
    void ClearCache();
}
