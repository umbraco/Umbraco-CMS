namespace Umbraco.Core.Models
{
    public static class MediaTypeExtensions
    {
        public static bool IsSystemMediaType(this IMediaType mediaType) =>
            mediaType.Alias == Constants.Conventions.MediaTypes.File
            || mediaType.Alias == Constants.Conventions.MediaTypes.Folder
            || mediaType.Alias == Constants.Conventions.MediaTypes.Image;
    }
}
