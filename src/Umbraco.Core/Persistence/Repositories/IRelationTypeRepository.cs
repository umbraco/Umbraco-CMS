using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IRelationType" /> entities.
/// </summary>
public interface IRelationTypeRepository : IAsyncReadWriteRepository<int, IRelationType>
{
    /// <summary>
    ///     Gets a relation type by its unique key.
    /// </summary>
    /// <param name="key">The unique key (GUID) of the relation type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching relation type, or <c>null</c> if not found.</returns>
    Task<IRelationType?> GetAsync(Guid key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a relation type by its alias.
    /// </summary>
    /// <param name="alias">The alias of the relation type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching relation type, or <c>null</c> if not found.</returns>
    Task<IRelationType?> GetByAliasAsync(string alias, CancellationToken cancellationToken = default);
}
