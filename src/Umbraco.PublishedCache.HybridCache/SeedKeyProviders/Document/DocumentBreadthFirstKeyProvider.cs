using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;

internal sealed class DocumentBreadthFirstKeyProvider : BreadthFirstKeyProvider, IDocumentSeedKeyProvider
{
    private readonly IDocumentPublishStatusQueryService _publishStatusService;

    public DocumentBreadthFirstKeyProvider(
        IDocumentNavigationQueryService documentNavigationQueryService,
        IOptions<CacheSettings> cacheSettings,
        IDocumentPublishStatusQueryService publishStatusService)
        : base(documentNavigationQueryService, cacheSettings.Value.DocumentBreadthFirstSeedCount)
    {
        _publishStatusService = publishStatusService;
    }

    /// <inheritdoc/>
    protected override bool ShouldSeed(Guid key)
        => _publishStatusService.IsPublishedInAnyCulture(key);
}
