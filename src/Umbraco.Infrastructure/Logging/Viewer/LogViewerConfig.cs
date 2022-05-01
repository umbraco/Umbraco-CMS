using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Logging.Viewer
{
    public class LogViewerConfig : ILogViewerConfig
    {
        private readonly ILogViewerQueryRepository _logViewerQueryRepository;
        private readonly IScopeProvider _scopeProvider;

        public LogViewerConfig(ILogViewerQueryRepository logViewerQueryRepository, IScopeProvider scopeProvider)
        {
            _logViewerQueryRepository = logViewerQueryRepository;
            _scopeProvider = scopeProvider;
        }

        public IReadOnlyList<SavedLogSearch>? GetSavedSearches()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var logViewerQueries =  _logViewerQueryRepository.GetMany();
            var result = logViewerQueries?.Select(x => new SavedLogSearch() { Name = x.Name, Query = x.Query }).ToArray();
            return result;
        }

        public IReadOnlyList<SavedLogSearch>? AddSavedSearch(string? name, string? query)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            _logViewerQueryRepository.Save(new LogViewerQuery(name, query));

            return GetSavedSearches();
        }

        public IReadOnlyList<SavedLogSearch>? DeleteSavedSearch(string? name, string? query)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var item = name is null ? null : _logViewerQueryRepository.GetByName(name);
            if (item is not null)
            {
                _logViewerQueryRepository.Delete(item);
            }

            //Return the updated object - so we can instantly reset the entire array from the API response
            return GetSavedSearches();
        }
    }
}
