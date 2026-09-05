using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Element;

/// <summary>
/// Provides seed keys for the element cache by traversing the element tree breadth-first
/// and filtering out unpublished elements and containers.
/// </summary>
/// <remarks>
/// The element tree contains both containers (folders) and actual elements.
/// Containers are always traversed for their children but are never added to the seed set.
/// Only published elements are counted toward the seed limit.
/// </remarks>
internal sealed class ElementBreadthFirstKeyProvider : BreadthFirstKeyProvider, IElementSeedKeyProvider
{
    private readonly IElementPublishStatusQueryService _publishStatusService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementBreadthFirstKeyProvider"/> class.
    /// </summary>
    /// <param name="elementNavigationQueryService">The element navigation query service for traversing the element tree.</param>
    /// <param name="cacheSettings">The cache settings containing the element seed count.</param>
    /// <param name="publishStatusService">The publish status service for filtering unpublished elements.</param>
    public ElementBreadthFirstKeyProvider(
        IElementNavigationQueryService elementNavigationQueryService,
        IOptions<CacheSettings> cacheSettings,
        IElementPublishStatusQueryService publishStatusService)
        : base(elementNavigationQueryService, cacheSettings.Value.ElementBreadthFirstSeedCount)
    {
        _publishStatusService = publishStatusService;
    }

    /// <inheritdoc/>
    protected override bool ShouldSeed(Guid key)
        => _publishStatusService.IsPublishedInAnyCulture(key);

    /// <inheritdoc/>
    /// <remarks>
    /// Always traverse children because containers (which are not published and not seeded)
    /// may have published element children.
    /// </remarks>
    protected override bool ShouldTraverseChildren(Guid key) => true;
}
