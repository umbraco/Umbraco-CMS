

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Model used in Razor Views for rendering
    /// </summary>
    public class MediaWithCrops
    {
        public IPublishedContent MediaItem { get; set; }

        public ImageCropperValue LocalCrops { get; set; }
    }
}
