using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Media;

internal sealed class MediaBreadthFirstKeyProvider :  BreadthFirstKeyProvider, IMediaSeedKeyProvider
{
    public MediaBreadthFirstKeyProvider(
        IMediaNavigationQueryService navigationQueryService, IOptions<CacheSettings> cacheSettings)
        : base(navigationQueryService, cacheSettings.Value.MediaBreadthFirstSeedCount)
    {
    }
}
