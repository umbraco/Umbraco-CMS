using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

public class MediaRecycleBinQueryService : IMediaRecycleBinQueryService
{
    private readonly IEntityService _entityService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IRelationService _relationService;

    public MediaRecycleBinQueryService(
        IEntityService entityService,
        ICoreScopeProvider scopeProvider,
        IRelationService relationService)
    {
        _entityService = entityService;
        _scopeProvider = scopeProvider;
        _relationService = relationService;
    }

    public Task<Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedMediaId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        if (_entityService.Get(trashedMediaId, UmbracoObjectTypes.Media) is not IMediaEntitySlim entity)
        {
            return Task.FromResult(Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NotFound));
        }

        if (entity.Trashed is false)
        {
            return Task.FromResult(Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NotTrashed));
        }

        IEnumerable<IRelation> relationsByChild = _relationService.GetByChildId(entity.Id);
        IRelation? parentRecycleRelation = relationsByChild.FirstOrDefault(
            r => r.RelationType.Alias == Core.Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias);

        if (parentRecycleRelation is null)
        {
            return Task.FromResult(Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NoParentRecycleRelation));
        }

        if (parentRecycleRelation.ParentId == Constants.System.Root)
        {
            return Task.FromResult(Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>.Succeed(RecycleBinQueryResultType.ParentIsRoot, null));
        }

        var parent =
            _entityService.Get(parentRecycleRelation.ParentId, UmbracoObjectTypes.Media) as IMediaEntitySlim;

        if (parent is null)
        {
            return Task.FromResult(Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.ParentNotFound));
        }

        if (parent.Trashed)
        {
            return Task.FromResult(Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.ParentNotFound, parent));
        }

        return Task.FromResult(Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>.Succeed(RecycleBinQueryResultType.Success, parent));
    }
}
