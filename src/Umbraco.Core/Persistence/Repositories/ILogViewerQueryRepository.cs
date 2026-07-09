using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="ILogViewerQuery" /> entities.
/// </summary>
public interface ILogViewerQueryRepository : IReadWriteQueryRepository<int, ILogViewerQuery>
{
    /// <summary>
    ///     Gets a log viewer query by its name.
    /// </summary>
    /// <param name="name">The name of the query.</param>
    /// <returns>The log viewer query if found; otherwise, <c>null</c>.</returns>
    ILogViewerQuery? GetByName(string name);
}
