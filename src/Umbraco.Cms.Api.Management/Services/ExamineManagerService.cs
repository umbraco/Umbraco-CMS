using Examine;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Provides services for managing Examine indexes and searchers within Umbraco CMS.
/// </summary>
public class ExamineManagerService : IExamineManagerService
{
    private readonly IExamineManager _examineManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExamineManagerService"/> class.
    /// </summary>
    /// <param name="examineManager">
    /// The <see cref="IExamineManager"/> instance used to manage examine operations.
    /// </param>
    public ExamineManagerService(IExamineManager examineManager) => _examineManager = examineManager;

    /// <summary>
    /// Attempts to find a searcher by the given searcher name.
    /// </summary>
    /// <param name="searcherName">The name of the searcher to find.</param>
    /// <param name="searcher">When this method returns, contains the found searcher if the searcher was found; otherwise, null.</param>
    /// <returns>True if the searcher was found; otherwise, false.</returns>
    public bool TryFindSearcher(string searcherName, out ISearcher searcher)
    {
        // try to get the searcher from the indexes
        if (!_examineManager.TryGetIndex(searcherName, out IIndex index))
        {
            // if we didn't find anything try to find it by an explicitly declared searcher
            return _examineManager.TryGetSearcher(searcherName, out searcher);
        }

        searcher = index.Searcher;
        return true;
    }
}
