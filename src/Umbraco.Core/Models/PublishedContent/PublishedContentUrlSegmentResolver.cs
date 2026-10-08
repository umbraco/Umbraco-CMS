using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
/// Resolves the URL segment of an <see cref="IPublishedContent"/> via <see cref="IDocumentUrlService.GetUrlSegment"/>,
/// honouring the ambient variation context and preview state.
/// </summary>
/// <remarks>
/// Helper used to satisfy the obsolete <see cref="IPublishedContent.UrlSegment"/> contract during the v18
/// deprecation period. Scheduled for removal in Umbraco 20 alongside the property itself. New code should call
/// <see cref="IDocumentUrlService.GetUrlSegment"/> directly.
/// </remarks>
[Obsolete("Helper for IPublishedContent.UrlSegment during the v18 deprecation period. Use IDocumentUrlService.GetUrlSegment() instead. Scheduled for removal in Umbraco 20.")]
public static class PublishedContentUrlSegmentResolver
{
    /// <summary>
    /// Resolves the URL segment for the specified content, using the supplied culture or falling back to the
    /// variation context's current culture, and detecting preview state from the ambient <see cref="IUmbracoContext"/>.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor">Used to resolve the ambient culture when <paramref name="culture"/> is null.</param>
    /// <param name="culture">An explicit culture, or null to use the ambient variation context.</param>
    /// <returns>The URL segment, or null if no segment is available for the resolved culture.</returns>
    public static string? Resolve(
        IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        var resolvedCulture = culture ?? variationContextAccessor?.VariationContext?.Culture ?? string.Empty;

        IUmbracoContextAccessor umbracoContextAccessor = StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>();
        var isDraft = umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) && umbracoContext.InPreviewMode;

        IDocumentUrlService documentUrlService = StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlService>();
        return documentUrlService.GetUrlSegment(content.Key, resolvedCulture, isDraft);
    }
}
