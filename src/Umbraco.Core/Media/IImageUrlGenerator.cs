using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    public interface IImageUrlGenerator
    {
        string GetImageUrl(ImageUrlGenerationOptions options);
    }
}
