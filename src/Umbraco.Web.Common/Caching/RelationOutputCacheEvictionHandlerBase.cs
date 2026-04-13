using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Caching;

/// <summary>
///     Base class for output cache eviction handlers that evict cached responses for documents
///     referencing changed entities via Umbraco's automatic relation types.
/// </summary>
public abstract class RelationOutputCacheEvictionHandlerBase
{
    private readonly IRelationService _relationService;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationOutputCacheEvictionHandlerBase"/> class.
    /// </summary>
    /// <param name="outputCacheStore">The output cache store for evicting cached responses.</param>
    /// <param name="relationService">The relation service for querying entity references.</param>
    /// <param name="idKeyMap">The ID/key mapping service for converting between integer IDs and GUIDs.</param>
    protected RelationOutputCacheEvictionHandlerBase(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap)
    {
        OutputCacheStore = outputCacheStore;
        _relationService = relationService;
        _idKeyMap = idKeyMap;
    }

    /// <summary>
    ///     Gets the <see cref="IOutputCacheStore"/> instance used by this handler.
    /// </summary>
    protected IOutputCacheStore OutputCacheStore { get; }

    /// <summary>
    ///     Evicts cached responses for all documents that reference any of the given entity IDs
    ///     via the specified relation type. Deduplicates parent IDs across all entities.
    /// </summary>
    /// <param name="changedEntityIds">The IDs of the entities that changed.</param>
    /// <param name="relationTypeAlias">The relation type alias to query.</param>
    /// <param name="contentTagPrefix">The tag prefix for content items (e.g. <c>umb-content-</c> or <c>umb-dapi-content-</c>).</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    protected async Task EvictRelatedContentAsync(
        IEnumerable<int> changedEntityIds,
        string relationTypeAlias,
        string contentTagPrefix,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var parentIds = new HashSet<int>();

        foreach (var entityId in changedEntityIds)
        {
            IEnumerable<IRelation> relations = _relationService.GetByChildId(entityId, relationTypeAlias);
            foreach (IRelation relation in relations)
            {
                parentIds.Add(relation.ParentId);
            }
        }

        foreach (var parentId in parentIds)
        {
            Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(parentId, UmbracoObjectTypes.Document);
            if (parentKeyAttempt.Success)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Entity referenced by document {ParentKey} via {RelationType} — evicting.",
                        parentKeyAttempt.Result,
                        relationTypeAlias);
                }

                await OutputCacheStore.EvictByTagAsync(
                    contentTagPrefix + parentKeyAttempt.Result,
                    cancellationToken);
            }
        }
    }
}
