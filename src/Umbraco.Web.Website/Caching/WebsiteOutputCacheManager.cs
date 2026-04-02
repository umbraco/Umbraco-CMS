using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Default implementation of <see cref="IWebsiteOutputCacheManager"/> that delegates to the
///     ASP.NET Core <see cref="IOutputCacheStore"/> using Umbraco's tag naming conventions.
///     All methods are no-ops when the output cache store is not registered.
/// </summary>
internal sealed class WebsiteOutputCacheManager : IWebsiteOutputCacheManager
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebsiteOutputCacheManager"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the output cache store.</param>
    public WebsiteOutputCacheManager(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public async Task EvictContentAsync(Guid contentKey, CancellationToken cancellationToken = default)
    {
        IOutputCacheStore? store = _serviceProvider.GetService<IOutputCacheStore>();
        if (store is not null)
        {
            await store.EvictByTagAsync(Constants.Website.OutputCache.ContentTagPrefix + contentKey, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        IOutputCacheStore? store = _serviceProvider.GetService<IOutputCacheStore>();
        if (store is not null)
        {
            await store.EvictByTagAsync(tag, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task EvictAllAsync(CancellationToken cancellationToken = default)
    {
        IOutputCacheStore? store = _serviceProvider.GetService<IOutputCacheStore>();
        if (store is not null)
        {
            await store.EvictByTagAsync(Constants.Website.OutputCache.AllContentTag, cancellationToken);
        }
    }
}
