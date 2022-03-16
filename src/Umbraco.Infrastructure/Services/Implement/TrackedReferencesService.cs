using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement
{
    public class TrackedReferencesService : ITrackedReferencesService
    {
        private readonly ITrackedReferencesRepository _trackedReferencesRepository;
        private readonly IScopeProvider _scopeProvider;
        private readonly IEntityService _entityService;

        public TrackedReferencesService(ITrackedReferencesRepository trackedReferencesRepository, IScopeProvider scopeProvider, IEntityService entityService)
        {
            _trackedReferencesRepository = trackedReferencesRepository;
            _scopeProvider = scopeProvider;
            _entityService = entityService;
        }

        public PagedResult<RelationItem> GetPagedRelationsForItems(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            var items =  _trackedReferencesRepository.GetPagedRelationsForItems(ids, pageIndex, pageSize,  filterMustBeIsDependency, out var totalItems);

            return new PagedResult<RelationItem>(totalItems, pageIndex+1, pageSize) { Items = items };
        }

        public PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            var items =  _trackedReferencesRepository.GetPagedItemsWithRelations(ids, pageIndex, pageSize,  filterMustBeIsDependency, out var totalItems);

            return new PagedResult<RelationItem>(totalItems, pageIndex+1, pageSize) { Items = items };
        }

        public PagedResult<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);

            var items = _trackedReferencesRepository.GetPagedDescendantsInReferences(
                parentId,
                pageIndex,
                pageSize,
                filterMustBeIsDependency,
                out var totalItems);
            return new PagedResult<RelationItem>(totalItems, pageIndex+1, pageSize) { Items = items };
        }
    }
}
