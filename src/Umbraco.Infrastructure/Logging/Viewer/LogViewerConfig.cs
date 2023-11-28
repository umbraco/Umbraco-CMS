using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.Common.DependencyInjection;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using StaticServiceProvider = Umbraco.Cms.Core.DependencyInjection.StaticServiceProvider;

namespace Umbraco.Cms.Core.Logging.Viewer;

public class LogViewerConfig : ILogViewerConfig
{
    private readonly ILogViewerQueryRepository _logViewerQueryRepository;
    private readonly IScopeProvider _scopeProvider;

    [Obsolete("Use non-obsolete ctor. This will be removed in Umbraco 14.")]
    public LogViewerConfig(ILogViewerQueryRepository logViewerQueryRepository, Umbraco.Cms.Core.Scoping.IScopeProvider scopeProvider)
        : this(logViewerQueryRepository, StaticServiceProvider.Instance.GetRequiredService<IScopeProvider>())
    {

    }

    //Temp ctor used by MSDI (Greedy)
    [Obsolete("Use non-obsolete ctor. This will be removed in Umbraco 14.")]
    public LogViewerConfig(ILogViewerQueryRepository logViewerQueryRepository, Umbraco.Cms.Core.Scoping.IScopeProvider coreScopeProvider, IScopeProvider scopeProvider)
        : this(logViewerQueryRepository, scopeProvider)
    {

    }

    public LogViewerConfig(ILogViewerQueryRepository logViewerQueryRepository, IScopeProvider scopeProvider)
    {
        _logViewerQueryRepository = logViewerQueryRepository;
        _scopeProvider = scopeProvider;
    }

    public IReadOnlyList<SavedLogSearch> GetSavedSearches()
    {
        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
        IEnumerable<ILogViewerQuery> logViewerQueries = _logViewerQueryRepository.GetMany();
        SavedLogSearch[] result = logViewerQueries.Select(x => new SavedLogSearch() { Name = x.Name, Query = x.Query }).ToArray();
        return result;
    }

    public IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query)
    {
        using IScope scope = _scopeProvider.CreateScope();
        _logViewerQueryRepository.Save(new LogViewerQuery(name, query));

        scope.Complete();
        return GetSavedSearches();
    }

    [Obsolete("Use the overload that only takes a 'name' parameter instead. This will be removed in Umbraco 14.")]
    public IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name, string query) => DeleteSavedSearch(name);

    public IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name)
    {
        using IScope scope = _scopeProvider.CreateScope();
        ILogViewerQuery? item = _logViewerQueryRepository.GetByName(name);
        if (item is not null)
        {
            _logViewerQueryRepository.Delete(item);
        }

        // Return the updated object - so we can instantly reset the entire array from the API response
        IReadOnlyList<SavedLogSearch> result =  GetSavedSearches()!;
        scope.Complete();
        return result;
        scope.Complete();
        return result;
    }
}
