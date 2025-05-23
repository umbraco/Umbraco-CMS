using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

// NOTE: this class is deliberately made accessible to 3rd party consumers (i.e. Deploy, uSync, ...)
public static class RichTextPropertyEditorHelper
{
    /// <summary>
    /// Attempts to parse a <see cref="RichTextEditorValue"/> instance from a property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="jsonSerializer">The system JSON serializer.</param>
    /// <param name="logger">A logger for error message handling.</param>
    /// <param name="richTextEditorValue">The parsed <see cref="RichTextEditorValue"/> instance, or null if parsing fails.</param>
    /// <returns>True if the parsing succeeds, false otherwise</returns>
    /// <remarks>
    /// The passed value can be:
    /// - a JSON string.
    /// - a JSON object.
    /// - a raw markup string (for backwards compatability).
    /// </remarks>
    public static bool TryParseRichTextEditorValue(object? value, IJsonSerializer jsonSerializer, ILogger logger, [NotNullWhen(true)] out RichTextEditorValue? richTextEditorValue)
    {
        var stringValue = value as string ?? value?.ToString();
        if (stringValue is null)
        {
            richTextEditorValue = null;
            return false;
        }

        if (stringValue.DetectIsJson() is false)
        {
            // assume value is raw markup and construct the model accordingly (no blocks stored)
            richTextEditorValue = new RichTextEditorValue { Markup = stringValue, Blocks = null };
            return true;
        }

        try
        {
            richTextEditorValue = jsonSerializer.Deserialize<RichTextEditorValue>(stringValue);
            return richTextEditorValue != null;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not parse rich text editor value, see exception for details.");
            richTextEditorValue = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a <see cref="RichTextEditorValue"/> instance for property value storage.
    /// </summary>
    /// <param name="richTextEditorValue">The <see cref="RichTextEditorValue"/> instance to serialize.</param>
    /// <param name="jsonSerializer">The system JSON serializer.</param>
    /// <returns>A string value representing the passed <see cref="RichTextEditorValue"/> instance.</returns>
    public static string SerializeRichTextEditorValue(RichTextEditorValue richTextEditorValue, IJsonSerializer jsonSerializer)
        => jsonSerializer.Serialize(richTextEditorValue);
}
