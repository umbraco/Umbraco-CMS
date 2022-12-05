using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

internal static class TypeExtensions
{
    public static bool IsRenderingModel(this Type type)
        => typeof(ContentModel).IsAssignableFrom(type) || typeof(IPublishedContent).IsAssignableFrom(type);
}
