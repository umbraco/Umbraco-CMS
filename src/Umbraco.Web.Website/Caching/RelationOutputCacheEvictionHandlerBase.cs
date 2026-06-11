using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Base class for website output cache eviction handlers that evict cached pages referencing changed entities
///     via Umbraco's automatic relation types. Delegates to the shared
///     <see cref="Common.Caching.RelationOutputCacheEvictionHandlerBase"/> with the website content tag prefix.
/// </summary>
internal abstract class RelationOutputCacheEvictionHandlerBase
    : Common.Caching.RelationOutputCacheEvictionHandlerBase
{
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
        : base(outputCacheStore, relationService, idKeyMap)
    {
    }

    /// <summary>
    ///     Evicts cached pages for all documents that reference any of the given entity IDs
    ///     via the specified relation type. Uses the website content tag prefix.
    /// </summary>
    protected Task EvictRelatedPagesAsync(
        IEnumerable<int> changedEntityIds,
        string relationTypeAlias,
        ILogger logger,
        CancellationToken cancellationToken)
        => EvictRelatedContentAsync(
            changedEntityIds,
            relationTypeAlias,
            Constants.Website.OutputCache.ContentTagPrefix,
            logger,
            cancellationToken);
}
