using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extensions methods for <see cref="IContentTypeBase" />.
/// </summary>
public static class ContentTypeBaseExtensions
{
    public static PublishedItemType GetItemType(this IContentTypeBase contentType)
    {
        Type type = contentType.GetType();
        PublishedItemType itemType = PublishedItemType.Unknown;
        if (contentType.IsElement)
        {
            itemType = PublishedItemType.Element;
        }
        else if (typeof(IContentType).IsAssignableFrom(type))
        {
            itemType = PublishedItemType.Content;
        }
        else if (typeof(IMediaType).IsAssignableFrom(type))
        {
            itemType = PublishedItemType.Media;
        }
        else if (typeof(IMemberType).IsAssignableFrom(type))
        {
            itemType = PublishedItemType.Member;
        }

        return itemType;
    }

    /// <summary>
    ///     Used to check if any property type was changed between variant/invariant
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public static bool WasPropertyTypeVariationChanged(this IContentTypeBase contentType) =>
        contentType.WasPropertyTypeVariationChanged(out IReadOnlyCollection<string> _);

    /// <summary>
    ///     Used to check if any property type was changed between variant/invariant
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="aliases"></param>
    /// <returns></returns>
    internal static bool WasPropertyTypeVariationChanged(
        this IContentTypeBase contentType,
        out IReadOnlyCollection<string> aliases)
    {
        var a = new List<string>();

        // property variation change?
        var hasAnyPropertyVariationChanged = contentType.PropertyTypes.Any(propertyType =>
        {
            // skip new properties
            // TODO: This used to be WasPropertyDirty("HasIdentity") but i don't think that actually worked for detecting new entities this does seem to work properly
            var isNewProperty = propertyType.WasPropertyDirty("Id");
            if (isNewProperty)
            {
                return false;
            }

            // variation change?
            var dirty = propertyType.WasPropertyDirty("Variations");
            if (dirty)
            {
                a.Add(propertyType.Alias);
            }

            return dirty;
        });

        aliases = a;
        return hasAnyPropertyVariationChanged;
    }
}
