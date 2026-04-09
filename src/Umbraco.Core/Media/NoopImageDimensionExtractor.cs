using System.Drawing;

namespace Umbraco.Cms.Core.Media;

/// <summary>
///     A no-operation implementation of <see cref="IImageDimensionExtractor"/> that does not extract any image dimensions.
/// </summary>
public sealed class NoopImageDimensionExtractor : IImageDimensionExtractor
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedImageFileTypes { get; } = Enumerable.Empty<string>();

    /// <inheritdoc />
    public Size? GetDimensions(Stream stream) => null;
}
