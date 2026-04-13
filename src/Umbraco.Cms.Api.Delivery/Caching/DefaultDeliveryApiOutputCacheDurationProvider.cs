using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Default implementation of <see cref="IDeliveryApiOutputCacheDurationProvider"/> that always
///     returns <c>null</c>, deferring to the configured default duration.
/// </summary>
internal sealed class DefaultDeliveryApiOutputCacheDurationProvider : IDeliveryApiOutputCacheDurationProvider
{
    /// <inheritdoc />
    public TimeSpan? GetDuration(IPublishedContent content) => null;
}
