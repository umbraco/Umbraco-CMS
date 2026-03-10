using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a value editor for Markdown content that sanitizes the input.
/// </summary>
internal sealed class MarkDownPropertyValueEditor : DataValueEditor
{
    private readonly IMarkdownSanitizer _markdownSanitizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkDownPropertyValueEditor"/> class.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="ioHelper">The IO helper.</param>
    /// <param name="attribute">The data editor attribute.</param>
    /// <param name="markdownSanitizer">The Markdown sanitizer.</param>
    public MarkDownPropertyValueEditor(
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataEditorAttribute attribute,
        IMarkdownSanitizer markdownSanitizer)
        : base(shortStringHelper, jsonSerializer, ioHelper, attribute) => _markdownSanitizer = markdownSanitizer;

    /// <inheritdoc />
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        if (string.IsNullOrWhiteSpace(editorValue.Value?.ToString()))
        {
            return null;
        }

        var sanitized = _markdownSanitizer.Sanitize(editorValue.Value.ToString()!);

        return sanitized.NullOrWhiteSpaceAsNull();
    }
}
