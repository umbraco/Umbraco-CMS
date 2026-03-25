using System.Drawing;

namespace Umbraco.Cms.Core.Media;

/// <summary>
/// Extracts image dimensions from an SVG stream.
/// </summary>
public interface ISvgDimensionExtractor
{
    /// <summary>
    /// Gets the dimensions.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>
    /// The dimensions of the image if the stream was parsable; otherwise, <c>null</c>.
    /// </returns>
    public Size? GetDimensions(Stream stream);
}
