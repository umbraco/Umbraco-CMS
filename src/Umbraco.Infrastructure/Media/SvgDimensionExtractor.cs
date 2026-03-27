using System.Drawing;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Infrastructure.Media;

/// <inheritdoc />
public class SvgDimensionExtractor : ISvgDimensionExtractor
{
    private readonly ILogger<SvgDimensionExtractor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SvgDimensionExtractor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SvgDimensionExtractor(ILogger<SvgDimensionExtractor> logger)
        => _logger = logger;

    /// <inheritdoc />
    public Size? GetDimensions(Stream stream)
    {
        if (stream.CanRead is false)
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract dimensions from SVG stream.");
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

    private static Size? ReadDimensions(Stream stream)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };
        using var reader = XmlReader.Create(stream, settings);
        var document = XDocument.Load(reader);

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

        // Fall back to viewbox.
        size ??= ParseViewBox(root);

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

        if (double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width) is false)
        {
            return null;
        }

        if (double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height) is false)
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

    private static Size? ParseWidthHeightAttributes(string widthAttributeValue, string heightAttributeValue)
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
    /// </summary>
    private static bool TryExtractNumericFromValue(string attributeValue, out int value)
    {
        if (int.TryParse(attributeValue, out int onlyNumbersValue) && onlyNumbersValue > 0)
        {
            value = onlyNumbersValue;
            return true;
        }

        value = 0;

        var input = attributeValue.Trim();

        if (input.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            input = input[..^2].Trim();
        }

        if (int.TryParse(input, out var numericValue) && numericValue > 0)
        {
            value = numericValue;
            return true;
        }

        return false;
    }
}
