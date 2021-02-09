using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core
{
    public static class MediaTypeExtensions
    {
        public static bool IsSystemMediaType(this IMediaType mediaType) =>
            mediaType.Alias == Constants.Conventions.MediaTypes.File
            || mediaType.Alias == Constants.Conventions.MediaTypes.Folder
            || mediaType.Alias == Constants.Conventions.MediaTypes.Image;
    }
}
