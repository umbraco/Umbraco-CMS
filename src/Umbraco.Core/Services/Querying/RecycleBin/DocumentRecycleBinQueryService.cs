using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

public class DocumentRecycleBinQueryService : IDocumentRecycleBinQueryService
{
    private readonly IEntityService _entityService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IRelationService _relationService;

    public DocumentRecycleBinQueryService(
        IEntityService entityService,
        ICoreScopeProvider scopeProvider,
        IRelationService relationService)
    {
        _entityService = entityService;
        _scopeProvider = scopeProvider;
        _relationService = relationService;
    }

    public Task<Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedDocumentId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        if (_entityService.Get(trashedDocumentId, UmbracoObjectTypes.Document) is not IDocumentEntitySlim entity)
        {
            return Task.FromResult(Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NotFound));
        }

        if (entity.Trashed is false)
        {
            return Task.FromResult(Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NotTrashed));
        }

        IEnumerable<IRelation> relationsByChild = _relationService.GetByChildId(entity.Id);
        IRelation? parentRecycleRelation = relationsByChild.FirstOrDefault(
            r => r.RelationType.Alias == Core.Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias);

        if (parentRecycleRelation is null)
        {
            return Task.FromResult(Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NoParentRecycleRelation));
        }

        if (parentRecycleRelation.ParentId == Constants.System.Root)
        {
            return Task.FromResult(Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>.Succeed(RecycleBinQueryResultType.ParentIsRoot, null));
        }

        var parent =
            _entityService.Get(parentRecycleRelation.ParentId, UmbracoObjectTypes.Document) as IDocumentEntitySlim;
        if (parent is null)
        {
            return Task.FromResult(Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.ParentNotFound));
        }

        if (parent.Trashed)
        {
            return Task.FromResult(Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.ParentNotFound, parent));
        }

        return Task.FromResult(Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>.Succeed(RecycleBinQueryResultType.Success, parent));
    }
}
