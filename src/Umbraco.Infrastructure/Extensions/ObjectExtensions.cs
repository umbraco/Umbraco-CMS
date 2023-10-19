using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Extensions;

internal static class ObjectExtensions
{
    internal static RichTextEditorValue? TryParseRichTextEditorValue(this object? value, IJsonSerializer jsonSerializer, ILogger logger)
    {
        // NOTE: value can be both a JSON string and a JSON object here.
        var stringValue = value as string ?? value?.ToString();
        if (stringValue is null)
        {
            return null;
        }

        try
        {
            return jsonSerializer.Deserialize<RichTextEditorValue>(stringValue);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not parse rich text editor value, see exception for details.");
            return null;
        }
    }
}
