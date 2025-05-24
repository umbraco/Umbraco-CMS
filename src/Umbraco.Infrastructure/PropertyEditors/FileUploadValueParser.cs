using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

// TODO: add interfacea and inject it via DI

/// <summary>
/// File upload value parser
/// </summary>
public sealed class FileUploadValueParser
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Creates instane of FileUploadValueParser
    /// </summary>
    /// <param name="jsonSerializer"></param>
    public FileUploadValueParser(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// Parses raw value to <a cref="FileUploadValue"></a>
    /// </summary>
    /// <param name="editorValue"></param>
    /// <returns><a cref="FileUploadValue"></a> value</returns>
    /// <exception cref="ArgumentException"></exception>
    public FileUploadValue? Parse(object? editorValue)
    {
        {
            if (editorValue is null)
            {
                return null;
            }

            if (editorValue is string sourceString && sourceString.DetectIsJson() is false)
            {
                return new FileUploadValue()
                {
                    Src = sourceString
                };
            }

            return _jsonSerializer.TryDeserialize(editorValue, out FileUploadValue? modelValue)
                ? modelValue
                : throw new ArgumentException($"Could not parse editor value to a {nameof(FileUploadValue)} object.");
        }
    }
}
