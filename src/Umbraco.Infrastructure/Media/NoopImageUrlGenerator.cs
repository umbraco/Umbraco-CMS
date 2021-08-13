using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Media
{
    public class NoopImageUrlGenerator : IImageUrlGenerator
    {
        /// <inheritdoc />
        public IEnumerable<string> SupportedImageFileTypes => Enumerable.Empty<string>();

        /// <inheritdoc />
        public string GetImageUrl(ImageUrlGenerationOptions options) => options?.ImageUrl;
    }
}
