using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence;

/// <summary>
///     Defines the base implementation of a querying repository.
/// </summary>
public interface IQueryRepository<TEntity> : IRepository
{
    /// <summary>
    ///     Gets entities.
    /// </summary>
    IEnumerable<TEntity> Get(IQuery<TEntity> query);

    /// <summary>
    ///     Counts entities.
    /// </summary>
    int Count(IQuery<TEntity> query);
}
