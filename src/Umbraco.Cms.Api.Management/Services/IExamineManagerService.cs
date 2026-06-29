using Examine;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Defines the contract for managing Examine indexers and searchers within Umbraco CMS.
/// </summary>
public interface IExamineManagerService
{
    /// <summary>
    /// Attempts to find a searcher by its name.
    /// </summary>
    /// <param name="searcherName">The name of the searcher to find.</param>
    /// <param name="searcher">When this method returns, contains the found searcher if the searcher was found; otherwise, null.</param>
    /// <returns>True if the searcher was found; otherwise, false.</returns>
    bool TryFindSearcher(string searcherName, out ISearcher searcher);
}
