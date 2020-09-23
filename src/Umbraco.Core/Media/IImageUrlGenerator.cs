using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    public interface IImageUrlGenerator
    {
        IEnumerable<string> SupportedImageFileTypes { get; }

        string GetImageUrl(ImageUrlGenerationOptions options);
    }
}
