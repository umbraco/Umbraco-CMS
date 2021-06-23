using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Model used in Razor Views for rendering
    /// </summary>
    public class MediaWithCrops : PublishedContentWrapped
    {
        public IPublishedContent MediaItem => Unwrap();

        public ImageCropperValue LocalCrops { get; }

        public MediaWithCrops(IPublishedContent content, ImageCropperValue localCrops)
            : base(content)
        {
            LocalCrops = localCrops;
        }
    }
}
