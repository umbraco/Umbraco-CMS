using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace Umbraco.Cms.Core.Media;

/// <inheritdoc />
public class SvgDimensionExtractor : ISvgDimensionExtractor
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedImageFileTypes { get; } = ["svg"];

    /// <inheritdoc />
    public Size? GetDimensions(Stream stream)
    {

        if (!stream.CanRead)
        {
            return null;
        }

        long? originalPosition = null;

        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            return ReadDimensions(stream);
        }
        catch
        {
            return null;
        }
        finally
        {
            if (originalPosition.HasValue)
            {
                stream.Position = originalPosition.Value;
            }
        }

    }

    private Size? ReadDimensions(Stream stream)
    {
        var document = XDocument.Load(stream);
        XElement? root = document.Root;

        if (root is null)
        {
            return null;
        }

        var widthAttributeValue = root.Attribute("width")?.Value;
        var heightAttributeValue = root.Attribute("height")?.Value;

        Size? size = null;

        if (widthAttributeValue is not null && heightAttributeValue is not null)
        {
            size = ParseWidthHeightAttributes(widthAttributeValue, heightAttributeValue);
        }

        // Fall back to viewbox
        if (size is null)
        {
            size = ParseViewBox(root);
        }

        return size;

    }

    private static Size? ParseViewBox(XElement root)
    {
        var viewBox = root.Attribute("viewBox")?.Value;

        if (string.IsNullOrWhiteSpace(viewBox))
        {
            return null;
        }

        var parts = viewBox.Split([' ', ',', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4)
        {
            return null;
        }

        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width))
        {
            return null;
        }

        if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return null;
        }

        if (width < 0 || height < 0)
        {
            return null;
        }

        return new Size(
            (int)Math.Round(width),
            (int)Math.Round(height));
    }

    private Size? ParseWidthHeightAttributes(string widthAttributeValue, string heightAttributeValue)
    {
        if (TryExtractNumericFromValue(widthAttributeValue, out var widthValue)
            && TryExtractNumericFromValue(heightAttributeValue, out var heightValue))
        {
            return new Size(widthValue, heightValue);
        }

        return null;
    }

    /// <summary>
    /// Extract a "pixel" value from the width / height attributes.
    /// This method makes some assumptions for valid but uncommon units like %,em and cm.
    /// </summary>
    /// <param name="attributeValue"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool TryExtractNumericFromValue(string attributeValue, out int value)
    {
        if (int.TryParse(attributeValue, out int onlyNumbersValue))
        {
            value = onlyNumbersValue;
            return true;
        }
        value = 0;

        var input = attributeValue.Trim();

        double multiplier = 1;

        if (input.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1;
            input = input[..^2].Trim();
        }
        else if (input.EndsWith("%", StringComparison.Ordinal))
        {
            multiplier = 4; // 100% = 400px
            input = input[..^1].Trim();
        }
        else if (input.EndsWith("em", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 10; // 1em = 10px
            input = input[..^2].Trim();
        }
        else if (input.EndsWith("cm", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 10; // 1cm = 10px
            input = input[..^2].Trim();
        }

        if (!int.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue))
        {
            return false;
        }

        if (numericValue < 0)
        {
            return false;
        }

        value = (int)Math.Round(numericValue * multiplier);

        return true;
    }
}
