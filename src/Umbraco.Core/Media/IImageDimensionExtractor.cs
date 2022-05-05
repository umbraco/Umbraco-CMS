using System.Drawing;
using System.IO;

namespace Umbraco.Cms.Core.Media
{
    public interface IImageDimensionExtractor
    {
        public Size? GetDimensions(Stream? stream);
    }
}
