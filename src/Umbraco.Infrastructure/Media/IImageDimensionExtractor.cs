using System.IO;

namespace Umbraco.Web.Media
{
    public interface IImageDimensionExtractor
    {
        public ImageSize GetDimensions(Stream stream);
    }
}
