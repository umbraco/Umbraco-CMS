using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models
{
    [UmbracoVolatile]
    public static class MediaTypeExtensions
    {
        public static bool IsSystemMediaType(this IMediaType mediaType) =>
            mediaType.Alias == Constants.Conventions.MediaTypes.File
            || mediaType.Alias == Constants.Conventions.MediaTypes.Folder
            || mediaType.Alias == Constants.Conventions.MediaTypes.Image;
    }
}
