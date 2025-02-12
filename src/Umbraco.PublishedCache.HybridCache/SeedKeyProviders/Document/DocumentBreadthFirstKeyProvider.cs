using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;

internal sealed class DocumentBreadthFirstKeyProvider : BreadthFirstKeyProvider, IDocumentSeedKeyProvider
{
    public DocumentBreadthFirstKeyProvider(
        IDocumentNavigationQueryService documentNavigationQueryService,
        IOptions<CacheSettings> cacheSettings)
        : base(documentNavigationQueryService, cacheSettings.Value.DocumentBreadthFirstSeedCount)
    {
    }
}
