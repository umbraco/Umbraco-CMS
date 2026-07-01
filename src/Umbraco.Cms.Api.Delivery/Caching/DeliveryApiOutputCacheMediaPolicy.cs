using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Output cache policy for Delivery API media endpoints.
/// </summary>
internal sealed class DeliveryApiOutputCacheMediaPolicy : DeliveryApiOutputCachePolicyBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiOutputCacheMediaPolicy"/> class.
    /// </summary>
    /// <param name="defaultDuration">The default cache duration from configuration.</param>
    /// <param name="defaultVaryByHeaders">The default vary-by headers for media requests.</param>
    public DeliveryApiOutputCacheMediaPolicy(TimeSpan defaultDuration, StringValues defaultVaryByHeaders)
        : base(defaultDuration, defaultVaryByHeaders)
    {
    }

    /// <inheritdoc />
    protected override string ResolvedItemsKey => DeliveryApiOutputCacheKeys.ResolvedMediaItemsKey;

    /// <inheritdoc />
    protected override string ItemTagPrefix => Constants.DeliveryApi.OutputCache.MediaTagPrefix;

    /// <inheritdoc />
    protected override string AllItemsTag => Constants.DeliveryApi.OutputCache.AllMediaTag;
}
