using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A custom value editor to ensure that macro syntax is parsed when being persisted and formatted correctly for
///     display in the editor
/// </summary>
internal class MarkDownPropertyValueEditor : DataValueEditor
{
    private readonly IMarkdownSanitizer _markdownSanitizer;

    public MarkDownPropertyValueEditor(
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataEditorAttribute attribute,
        IMarkdownSanitizer markdownSanitizer)
        : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute) => _markdownSanitizer = markdownSanitizer;

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
