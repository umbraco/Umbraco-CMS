// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Defines the file upload property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.UploadField,
    ValueEditorIsReusable = true)]
public class FileUploadPropertyEditor : DataEditor, IMediaUrlGenerator, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadPropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for property values.</param>
    /// <param name="ioHelper">Helper used for IO operations such as file path resolution.</param>
    public FileUploadPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(FileUploadValue);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("object", "null"),
        ["properties"] = new JsonObject
        {
            ["src"] = new JsonObject
            {
                ["type"] = new JsonArray("string", "null"),
                ["description"] = "Source file path",
            },
            ["temporaryFileId"] = new JsonObject
            {
                ["type"] = new JsonArray("string", "null"),
                ["format"] = "uuid",
                ["pattern"] = ValueSchemaPatterns.Uuid,
                ["description"] = "Temporary file ID for new uploads",
            },
        },
        ["description"] = "File upload value with source path or temporary file reference",
    };

    /// <inheritdoc/>
    public bool TryGetMediaPath(string? propertyEditorAlias, object? value, [MaybeNullWhen(false)] out string mediaPath)
    {
        if (propertyEditorAlias == Alias &&
            value?.ToString() is var mediaPathValue &&
            !string.IsNullOrWhiteSpace(mediaPathValue))
        {
            mediaPath = mediaPathValue;
            return true;
        }

        mediaPath = null;
        return false;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new FileUploadConfigurationEditor(_ioHelper);

    /// <summary>
    ///     Creates the corresponding property value editor.
    /// </summary>
    /// <returns>The corresponding property value editor.</returns>
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<FileUploadPropertyValueEditor>(Attribute!);
}
