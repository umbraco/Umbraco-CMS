using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IMediaType" />
/// </summary>
/// <remarks>
///     The shared content-type-composition logic lives in <see cref="AsyncContentTypeRepositoryBase{TEntity}"/>;
///     media types add no definition tables of their own, so this class only supplies the node object type and
///     the entity-specific parts of the persistence hooks.
/// </remarks>
internal sealed class MediaTypeRepository : AsyncContentTypeRepositoryBase<IMediaType>, IMediaTypeRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeRepository"/> class.
    /// </summary>
    public MediaTypeRepository(
        AppCaches cache,
        ILogger<MediaTypeRepository> logger,
        IContentTypeCommonRepository commonRepository,
        ILanguageRepository languageRepository,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        IIdKeyMap idKeyMap,
        ICacheSyncService cacheSyncService,
        IEFCoreScopeAccessor<UmbracoDbContext> efCoreScopeAccessor)
        : base(
            cache,
            logger,
            commonRepository,
            languageRepository,
            repositoryCacheVersionService,
            idKeyMap,
            cacheSyncService,
            efCoreScopeAccessor)
    {
    }

    /// <inheritdoc />
    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.MediaType;

    /// <inheritdoc />
    protected override bool SupportsPublishing => MediaType.SupportsPublishingConst;

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IMediaType entity)
    {
        entity.AddingEntity();

        await PersistNewBaseContentTypeAsync(entity);

        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override async Task PersistUpdatedItemAsync(IMediaType entity)
    {
        ValidateAlias(entity);

        // Updates Modified date
        entity.UpdatingEntity();

        // Look up parent to get and set the correct Path if ParentId has changed
        if (entity.IsPropertyDirty("ParentId"))
        {
            var parent = await ExecuteEfScopeAsync(db => db.Nodes
                .Where(x => x.NodeId == entity.ParentId)
                .Select(x => new { x.Path, x.Level })
                .FirstAsync());
            entity.Path = string.Concat(parent.Path, ",", entity.Id);
            entity.Level = parent.Level + 1;

            var maxSortOrder = await ExecuteEfScopeAsync(db => db.Nodes
                .Where(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync()) ?? 0;
            entity.SortOrder = maxSortOrder + 1;
        }

        await PersistUpdatedBaseContentTypeAsync(entity);

        entity.ResetDirtyProperties();
    }
}
