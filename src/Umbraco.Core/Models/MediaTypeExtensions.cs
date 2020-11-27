namespace Umbraco.Core.Models
{
    internal static class MediaTypeExtensions
    {
        internal static bool IsSystemMediaType(this IMediaType mediaType) =>
            mediaType.Alias == Constants.Conventions.MediaTypes.File
            || mediaType.Alias == Constants.Conventions.MediaTypes.Folder
            || mediaType.Alias == Constants.Conventions.MediaTypes.Image;
    }
}
