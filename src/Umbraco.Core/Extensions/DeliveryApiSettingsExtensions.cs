using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class DeliveryApiSettingsExtensions
{
    public static bool IsAllowedContentType(this DeliveryApiSettings settings, IPublishedContent content)
        => settings.IsAllowedContentType(content.ContentType.Alias);

    public static bool IsDisallowedContentType(this DeliveryApiSettings settings, IPublishedContent content)
        => settings.IsDisallowedContentType(content.ContentType.Alias);

    public static bool IsAllowedContentType(this DeliveryApiSettings settings, string contentTypeAlias)
        => settings.IsDisallowedContentType(contentTypeAlias) is false;

    public static bool IsDisallowedContentType(this DeliveryApiSettings settings, string contentTypeAlias)
        => settings.DisallowedContentTypeAliases.InvariantContains(contentTypeAlias);
}
