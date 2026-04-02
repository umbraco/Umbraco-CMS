using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Provides a base implementation for published content caches.
/// </summary>
public abstract class PublishedCacheBase : IPublishedCache
{
    private readonly IVariationContextAccessor? _variationContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedCacheBase"/> class.
    /// </summary>
    /// <param name="variationContextAccessor">The variation context accessor for culture and segment information.</param>
    /// <param name="previewDefault">A value indicating whether preview mode is enabled by default.</param>
    public PublishedCacheBase(IVariationContextAccessor variationContextAccessor, bool previewDefault)
    {
        _variationContextAccessor = variationContextAccessor;
        PreviewDefault = previewDefault;
    }

    /// <summary>
    /// Gets a value indicating whether preview mode is enabled by default.
    /// </summary>
    public bool PreviewDefault { get; }

    /// <inheritdoc />
    public abstract IPublishedContent? GetById(bool preview, int contentId);

    /// <inheritdoc />
    public IPublishedContent? GetById(int contentId)
        => GetById(PreviewDefault, contentId);

    /// <inheritdoc />
    public abstract IPublishedContent? GetById(bool preview, Guid contentId);

    /// <inheritdoc />
    public IPublishedContent? GetById(Guid contentId)
        => GetById(PreviewDefault, contentId);

    /// <summary>
    /// Determines whether content with the specified identifier exists.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns><c>true</c> if content with the specified identifier exists; otherwise, <c>false</c>.</returns>
    public abstract bool HasById(bool preview, int contentId);

    /// <summary>
    /// Determines whether content with the specified identifier exists using the default preview setting.
    /// </summary>
    /// <param name="contentId">The content unique identifier.</param>
    /// <returns><c>true</c> if content with the specified identifier exists; otherwise, <c>false</c>.</returns>
    public bool HasById(int contentId)
        => HasById(PreviewDefault, contentId);

    /// <summary>
    /// Gets content items at the root of the content tree.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="culture">The optional culture to filter by.</param>
    /// <returns>A collection of published content items at the root level.</returns>
    public abstract IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null);
}
