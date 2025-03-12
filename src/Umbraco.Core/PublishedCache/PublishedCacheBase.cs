using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using StaticServiceProvider = Umbraco.Cms.Core.DependencyInjection.StaticServiceProvider;

namespace Umbraco.Cms.Core.PublishedCache;

public abstract class PublishedCacheBase : IPublishedCache
{
    private readonly IVariationContextAccessor? _variationContextAccessor;


    [Obsolete("Use ctor with all parameters. This will be removed in V15")]
    public PublishedCacheBase(IVariationContextAccessor variationContextAccessor)
        : this(variationContextAccessor, false)
    {
    }

    [Obsolete("Use ctor with all parameters. This will be removed in V15")]
    protected PublishedCacheBase(bool previewDefault)
        : this(StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(), previewDefault)
    {
    }

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
