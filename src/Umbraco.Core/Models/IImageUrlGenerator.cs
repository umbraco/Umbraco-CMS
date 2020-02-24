namespace Umbraco.Core.Models
{
    public interface IImageUrlGenerator
    {
        string GetImageUrl(ImageUrlGenerationOptions options);
    }
}
