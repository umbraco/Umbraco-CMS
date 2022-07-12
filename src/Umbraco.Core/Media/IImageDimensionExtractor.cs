using System.Drawing;

namespace Umbraco.Cms.Core.Media;

public interface IImageDimensionExtractor
{
    public Size? GetDimensions(Stream? stream);
}
