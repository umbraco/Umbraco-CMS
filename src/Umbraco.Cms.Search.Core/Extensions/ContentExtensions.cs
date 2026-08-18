using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Search.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IContentBase"/>, used by the content indexing pipeline.
/// </summary>
internal static class ContentExtensions
{
    /// <summary>
    /// Gets the numeric IDs of the content's ancestors, excluding the content itself and the root.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The ancestor IDs.</returns>
    public static IEnumerable<int> AncestorIds(this IContentBase content)
        => content.Path.Split(CoreConstants.CharArrays.Comma)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture))
            .Where(i => i > 0 && i != content.Id);

    /// <summary>
    /// Gets the published culture codes for the content, or a single null entry for invariant content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The published culture codes.</returns>
    public static string?[] PublishedCultures(this IContentBase content)
        => content is IContent c && c.VariesByCulture()
            ? c.PublishedCultures.ToArray()
            : new string?[] { null };

    /// <summary>
    /// Gets the available culture codes for the content, or a single null entry for invariant content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The available culture codes.</returns>
    public static string?[] AvailableCultures(this IContentBase content)
        => content is IContent && content.VariesByCulture()
            ? content.AvailableCultures.ToArray()
            : new string?[] { null };

    /// <summary>
    /// Gets whether the content is a published <see cref="IContent"/>.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>True if the content is published.</returns>
    public static bool IsPublished(this IContentBase content)
        => content is IContent { Published: true };

    /// <summary>
    /// Gets whether the content varies by culture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>True if the content's content type varies by culture.</returns>
    public static bool VariesByCulture(this IContentBase content)
        => content is IContent c && c.ContentType.VariesByCulture();

    /// <summary>
    /// Gets the Umbraco object type of the content.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The corresponding <see cref="UmbracoObjectTypes"/> value.</returns>
    public static UmbracoObjectTypes ObjectType(this IContentBase content)
        => content switch
        {
            IContent => UmbracoObjectTypes.Document,
            IMedia => UmbracoObjectTypes.Media,
            IMember => UmbracoObjectTypes.Member,
            _ => throw new ArgumentOutOfRangeException(nameof(content))
        };
}
