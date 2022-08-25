using System.Drawing;

namespace Umbraco.Cms.Core.Media
{
    public sealed class NoopImageDimensionExtractor : IImageDimensionExtractor
    {
        public Size? GetDimensions(Stream? stream) => null;
    }
}
