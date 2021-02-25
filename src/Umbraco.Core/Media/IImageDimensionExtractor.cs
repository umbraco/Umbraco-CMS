using System.IO;

namespace Umbraco.Cms.Core.Media
{
    public interface IImageDimensionExtractor
    {
        public ImageSize GetDimensions(Stream stream);
    }
}
