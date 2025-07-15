using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public abstract class PublishedCacheBase : IPublishedCache
{
    private readonly IVariationContextAccessor? _variationContextAccessor;

    public PublishedCacheBase(IVariationContextAccessor variationContextAccessor, bool previewDefault)
    {
        _variationContextAccessor = variationContextAccessor;
        PreviewDefault = previewDefault;
    }

    public bool PreviewDefault { get; }

    public abstract IPublishedContent? GetById(bool preview, int contentId);

    public IPublishedContent? GetById(int contentId)
        => GetById(PreviewDefault, contentId);

    public abstract IPublishedContent? GetById(bool preview, Guid contentId);

    public IPublishedContent? GetById(Guid contentId)
        => GetById(PreviewDefault, contentId);

    public abstract IPublishedContent? GetById(bool preview, Udi contentId);

    public IPublishedContent? GetById(Udi contentId)
        => GetById(PreviewDefault, contentId);

    public abstract bool HasById(bool preview, int contentId);

    public bool HasById(int contentId)
        => HasById(PreviewDefault, contentId);

    public abstract IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null);

    public IEnumerable<IPublishedContent> GetAtRoot(string? culture = null) => GetAtRoot(PreviewDefault, culture);

    public abstract bool HasContent(bool preview);

    public bool HasContent() => HasContent(PreviewDefault);
}
