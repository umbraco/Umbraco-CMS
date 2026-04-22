using Microsoft.AspNetCore.OutputCaching;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Default implementation of <see cref="IDeliveryApiOutputCacheManager"/> that delegates
///     to the ASP.NET Core <see cref="IOutputCacheStore"/>.
/// </summary>
internal sealed class DeliveryApiOutputCacheManager : IDeliveryApiOutputCacheManager
{
    private readonly IOutputCacheStore _outputCacheStore;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiOutputCacheManager"/> class.
    /// </summary>
    /// <param name="outputCacheStore">The ASP.NET Core output cache store.</param>
    public DeliveryApiOutputCacheManager(IOutputCacheStore outputCacheStore)
        => _outputCacheStore = outputCacheStore;

    /// <inheritdoc />
    public async Task EvictContentAsync(Guid contentKey, CancellationToken cancellationToken = default)
        => await _outputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.ContentTagPrefix + contentKey, cancellationToken);

    /// <inheritdoc />
    public async Task EvictMediaAsync(Guid mediaKey, CancellationToken cancellationToken = default)
        => await _outputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.MediaTagPrefix + mediaKey, cancellationToken);

    /// <inheritdoc />
    public async Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default)
        => await _outputCacheStore.EvictByTagAsync(tag, cancellationToken);

    /// <inheritdoc />
    public async Task EvictAllContentAsync(CancellationToken cancellationToken = default)
        => await _outputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.AllContentTag, cancellationToken);

    /// <inheritdoc />
    public async Task EvictAllMediaAsync(CancellationToken cancellationToken = default)
        => await _outputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.AllMediaTag, cancellationToken);

    /// <inheritdoc />
    public async Task EvictAllAsync(CancellationToken cancellationToken = default)
        => await _outputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.AllTag, cancellationToken);
}
