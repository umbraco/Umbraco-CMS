using System.Drawing;

namespace Umbraco.Cms.Core.Media;

public sealed class NoopImageDimensionExtractor : IImageDimensionExtractor
{
    public IEnumerable<string> SupportedImageFileTypes { get; } = Enumerable.Empty<string>();

    public Size? GetDimensions(Stream stream) => null;
}
