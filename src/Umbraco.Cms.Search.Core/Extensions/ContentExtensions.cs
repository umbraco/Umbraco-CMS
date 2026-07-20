using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Search.Core.Extensions;

internal static class ContentExtensions
{
    public static IEnumerable<int> AncestorIds(this IContentBase content)
        => content.Path.Split(CoreConstants.CharArrays.Comma)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture))
            .Where(i => i > 0 && i != content.Id);

    public static string?[] PublishedCultures(this IContentBase content)
        => content is IContent c && c.VariesByCulture()
            ? c.PublishedCultures.ToArray()
            : new string?[] { null };

    public static string?[] AvailableCultures(this IContentBase content)
        => content is IContent && content.VariesByCulture()
            ? content.AvailableCultures.ToArray()
            : new string?[] { null };

    public static bool IsPublished(this IContentBase content)
        => content is IContent { Published: true };

    public static bool VariesByCulture(this IContentBase content)
        => content is IContent c && c.ContentType.VariesByCulture();

    public static UmbracoObjectTypes ObjectType(this IContentBase content)
        => content switch
        {
            IContent => UmbracoObjectTypes.Document,
            IMedia => UmbracoObjectTypes.Media,
            IMember => UmbracoObjectTypes.Member,
            _ => throw new ArgumentOutOfRangeException(nameof(content))
        };
}
