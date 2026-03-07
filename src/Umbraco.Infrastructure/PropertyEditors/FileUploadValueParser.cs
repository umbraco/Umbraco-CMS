using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

/// <summary>
/// Handles the parsing of raw values to <see cref="FileUploadValue"/> objects.
/// </summary>
public sealed class FileUploadValueParser
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadValueParser"/> class.
    /// </summary>
    /// <param name="jsonSerializer"></param>
    public FileUploadValueParser(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    /// <summary>
    /// Parses raw value to a <see cref="FileUploadValue"/>.
    /// </summary>
    /// <param name="editorValue">The editor value.</param>
    /// <returns><a cref="FileUploadValue"></a> value</returns>
    /// <exception cref="ArgumentException"></exception>
    public FileUploadValue? Parse(object? editorValue)
    {
        if (editorValue is null)
        {
            return null;
        }

        if (editorValue is string sourceString && sourceString.DetectIsJson() is false)
        {
            return new FileUploadValue()
            {
                Src = sourceString,
            };
        }

        return _jsonSerializer.TryDeserialize(editorValue, out FileUploadValue? modelValue)
            ? modelValue
            : throw new ArgumentException($"Could not parse editor value to a {nameof(FileUploadValue)} object.");
    }
}
