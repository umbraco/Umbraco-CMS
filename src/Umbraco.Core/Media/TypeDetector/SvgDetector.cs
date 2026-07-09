using System.Xml.Linq;

namespace Umbraco.Cms.Core.Media.TypeDetector;

/// <summary>
///     Detects whether a file stream contains an SVG (Scalable Vector Graphics) image.
/// </summary>
public class SvgDetector
{
    /// <summary>
    ///     Determines whether the specified file stream contains an SVG image.
    /// </summary>
    /// <param name="fileStream">The file stream to examine.</param>
    /// <returns><c>true</c> if the stream contains an SVG image; otherwise, <c>false</c>.</returns>
    public static bool IsOfType(Stream fileStream)
    {
        var document = new XDocument();

        try
        {
            document = XDocument.Load(fileStream);
        }
        catch (Exception)
        {
            return false;
        }

        return document.Root?.Name.LocalName == "svg";
    }
}
