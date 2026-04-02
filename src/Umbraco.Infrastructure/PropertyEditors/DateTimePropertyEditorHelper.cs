using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Helper class for the DateTime property editors.
/// </summary>
public static class DateTimePropertyEditorHelper
{
    internal static bool TryParseToIntermediateValue(object? value, IJsonSerializer jsonSerializer, ILogger logger, out DateTimeValueConverterBase.DateTimeDto? intermediateValue)
    {
        if (value is null)
        {
            intermediateValue = null;
            return true;
        }

        try
        {
            intermediateValue = value switch
            {
                // This DateTime check is for compatibility with the "deprecated" `Umbraco.DateTime`.
                // Once that is removed, this can be removed too.
                DateTime dateTime => new DateTimeValueConverterBase.DateTimeDto
                {
                    Date = new DateTimeOffset(dateTime, TimeSpan.Zero),
                },
                string sourceStr => jsonSerializer.Deserialize<DateTimeValueConverterBase.DateTimeDto>(sourceStr),
                _ => throw new InvalidOperationException($"Unsupported type {value.GetType()}"),
            };
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not parse date time editor value, see exception for details.");
            intermediateValue = null;
            return false;
        }
    }
}
