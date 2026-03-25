using System.Drawing;

namespace Umbraco.Cms.Core.Media;

/// <summary>
/// Extracts image dimensions from an SVG stream.
/// </summary>
public interface ISvgDimensionExtractor
{
    /// <summary>
    /// Gets the supported image file types/extensions.
    /// </summary>
    /// <value>
    /// The supported image file types/extensions.
    /// </value>
    IEnumerable<string> SupportedImageFileTypes { get; }

    /// <summary>
    /// Gets the dimensions.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>
    /// The dimensions of the image if the stream was parsable; otherwise, <c>null</c>.
    /// </returns>
    public Size? GetDimensions(Stream stream);
}
