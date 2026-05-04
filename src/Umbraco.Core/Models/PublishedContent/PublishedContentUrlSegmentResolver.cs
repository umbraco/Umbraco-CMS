using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
/// Resolves the URL segment of an <see cref="IPublishedContent"/> from its <see cref="IPublishedContent.Cultures"/>
/// dictionary, honouring culture variance and the ambient variation context.
/// </summary>
/// <remarks>
/// Internal helper used to satisfy the obsolete <see cref="IPublishedContent.UrlSegment"/> contract during the v18
/// deprecation period. Scheduled for removal in Umbraco 20 alongside the property itself. New code should use
/// <c>IDocumentUrlService.GetUrlSegment()</c>.
/// </remarks>
[Obsolete("Helper for IPublishedContent.UrlSegment during the v18 deprecation period. Use IDocumentUrlService.GetUrlSegment() instead. Scheduled for removal in Umbraco 20.")]
public static class PublishedContentUrlSegmentResolver
{
    /// <summary>
    /// Resolves the URL segment for the specified content, using the supplied culture or falling back to the
    /// variation context's current culture for variant content.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="variationContextAccessor">Used to resolve the ambient culture for variant content when <paramref name="culture"/> is null.</param>
    /// <param name="culture">An explicit culture, or null to use the ambient variation context.</param>
    /// <returns>The URL segment, or null if no segment is available for the resolved culture.</returns>
    public static string? Resolve(
        IPublishedContent content,
        IVariationContextAccessor? variationContextAccessor,
        string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (content.ContentType.VariesByCulture() is false)
        {
            return content.Cultures.TryGetValue(string.Empty, out PublishedCultureInfo? invariantInfos)
                ? invariantInfos.UrlSegment
                : null;
        }

        culture ??= variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        return culture != string.Empty && content.Cultures.TryGetValue(culture, out PublishedCultureInfo? infos)
            ? infos.UrlSegment
            : null;
    }
}
