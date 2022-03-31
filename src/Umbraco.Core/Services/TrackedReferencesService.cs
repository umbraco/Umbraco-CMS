using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services
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

        /// <summary>
        /// Gets a paged result of items which are in relation with the current item.
        /// Basically, shows the items which depend on the current item.
        /// </summary>
        public PagedResult<RelationItem> GetPagedRelationsForItem(int id, long pageIndex, int pageSize, bool filterMustBeIsDependency)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            var items = _trackedReferencesRepository.GetPagedRelationsForItem(id, pageIndex, pageSize, filterMustBeIsDependency, out var totalItems);

            return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
        }

        /// <summary>
        /// Gets a paged result of items used in any kind of relation from selected integer ids.
        /// </summary>
        public PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            var items = _trackedReferencesRepository.GetPagedItemsWithRelations(ids, pageIndex, pageSize, filterMustBeIsDependency, out var totalItems);

            return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
        }

        /// <summary>
        /// Gets a paged result of the descending items that have any references, given a parent id.
        /// </summary>
        public PagedResult<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency)
        {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);

            var items = _trackedReferencesRepository.GetPagedDescendantsInReferences(
                parentId,
                pageIndex,
                pageSize,
                filterMustBeIsDependency,
                out var totalItems);
            return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
        }
    }
}
