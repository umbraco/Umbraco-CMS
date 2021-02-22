using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Media
{
    public interface IImageUrlGenerator
    {
        IEnumerable<string> SupportedImageFileTypes { get; }

        string GetImageUrl(ImageUrlGenerationOptions options);
    }
}
