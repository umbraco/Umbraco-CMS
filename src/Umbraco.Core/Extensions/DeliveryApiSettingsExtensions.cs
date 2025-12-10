using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class DeliveryApiSettingsExtensions
{
    public static bool IsAllowedContentType(this DeliveryApiSettings settings, IPublishedContent content)
        => settings.IsAllowedContentType(content.ContentType.Alias);

    public static bool IsDisallowedContentType(this DeliveryApiSettings settings, IPublishedContent content)
        => settings.IsDisallowedContentType(content.ContentType.Alias);

    /// <summary>
    ///     Determines whether a content type alias is allowed to be exposed through the Delivery API.
    /// </summary>
    /// <param name="settings">The Delivery API settings.</param>
    /// <param name="contentTypeAlias">The content type alias to check.</param>
    /// <returns>
    ///     <c>true</c> if the content type is allowed; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     If the allow list is configured (non-empty), only content types in the allow list are permitted.
    ///     The allow list takes precedence - if a content type is in both allow and disallow lists, it is allowed.
    ///     If the allow list is empty, all content types are allowed except those in the disallow list.
    /// </remarks>
    public static bool IsAllowedContentType(this DeliveryApiSettings settings, string contentTypeAlias)
    {
        // If allow list is configured, it takes precedence
        if (settings.AllowedContentTypeAliases.Count > 0)
        {
            return settings.AllowedContentTypeAliases.InvariantContains(contentTypeAlias);
        }

        // Otherwise the content type is allowed if it's not in the disallow list
        return settings.DisallowedContentTypeAliases.InvariantContains(contentTypeAlias) is false;
    }

    public static bool IsDisallowedContentType(this DeliveryApiSettings settings, string contentTypeAlias)
        => settings.IsAllowedContentType(contentTypeAlias) is false;
}
