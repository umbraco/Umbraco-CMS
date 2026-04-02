using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Evicts cached pages that reference the changed content via picker properties.
///     When content or media B is published, any document A that has a picker referencing B
///     will have its cached page evicted.
/// </summary>
/// <remarks>
///     Uses the automatic relation types (<c>umbDocument</c>, <c>umbMedia</c>) that Umbraco
///     maintains based on property editor values (content pickers, media pickers, etc.).
/// </remarks>
internal sealed class RelationOutputCacheEvictionProvider : IWebsiteOutputCacheEvictionProvider
{
    private readonly IRelationService _relationService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILogger<RelationOutputCacheEvictionProvider> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationOutputCacheEvictionProvider"/> class.
    /// </summary>
    /// <param name="relationService">The relation service for querying content references.</param>
    /// <param name="idKeyMap">The ID/key mapping service for converting between integer IDs and GUIDs.</param>
    /// <param name="logger">The logger.</param>
    public RelationOutputCacheEvictionProvider(
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<RelationOutputCacheEvictionProvider> logger)
    {
        _relationService = relationService;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<IEnumerable<string>> GetAdditionalEvictionTagsAsync(
        OutputCacheContentChangedContext context,
        CancellationToken cancellationToken = default)
    {
        var tags = new List<string>();

        // Find all documents that reference this content via content picker properties.
        // Media and member relations are handled by their own dedicated notification handlers.
        {
            IEnumerable<IRelation> relations = _relationService.GetByChildId(context.ContentId, Constants.Conventions.RelationTypes.RelatedDocumentAlias);
            foreach (IRelation relation in relations)
            {
                Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(relation.ParentId, UmbracoObjectTypes.Document);
                if (parentKeyAttempt.Success)
                {
                    tags.Add(Constants.Website.OutputCache.ContentTagPrefix + parentKeyAttempt.Result);
                    _logger.LogDebug(
                        "Content {ChangedKey} is referenced by {ParentKey} via umbDocument relation — evicting.",
                        context.ContentKey,
                        parentKeyAttempt.Result);
                }
            }
        }

        return Task.FromResult<IEnumerable<string>>(tags);
    }
}
