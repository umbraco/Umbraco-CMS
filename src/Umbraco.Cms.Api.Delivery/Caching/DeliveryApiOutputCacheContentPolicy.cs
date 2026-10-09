using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Output cache policy for Delivery API content endpoints.
/// </summary>
internal sealed class DeliveryApiOutputCacheContentPolicy : DeliveryApiOutputCachePolicyBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiOutputCacheContentPolicy"/> class.
    /// </summary>
    /// <param name="defaultDuration">The default cache duration from configuration.</param>
    /// <param name="defaultVaryByHeaders">The default vary-by headers for content requests.</param>
    public DeliveryApiOutputCacheContentPolicy(TimeSpan defaultDuration, StringValues defaultVaryByHeaders)
        : base(defaultDuration, defaultVaryByHeaders)
    {
    }

    /// <inheritdoc />
    protected override string ResolvedItemsKey => DeliveryApiOutputCacheKeys.ResolvedContentItemsKey;

    /// <inheritdoc />
    protected override string ItemTagPrefix => Constants.DeliveryApi.OutputCache.ContentTagPrefix;

    /// <inheritdoc />
    protected override string AllItemsTag => Constants.DeliveryApi.OutputCache.AllContentTag;

    /// <inheritdoc />
    protected override void AddItemTags(OutputCacheContext context, IPublishedContent item, IServiceProvider services)
    {
        // Tag with ancestor keys for branch eviction.
        IDocumentNavigationQueryService navigationService = services.GetRequiredService<IDocumentNavigationQueryService>();
        if (navigationService.TryGetAncestorsKeys(item.Key, out IEnumerable<Guid> ancestorKeys))
        {
            foreach (Guid ancestorKey in ancestorKeys)
            {
                context.Tags.Add(Constants.DeliveryApi.OutputCache.AncestorTagPrefix + ancestorKey);
            }
        }
    }
}
