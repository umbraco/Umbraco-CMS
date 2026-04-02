using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for selecting colors using an eye dropper.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ColorPickerEyeDropper,
    ValueEditorIsReusable = true)]
public class EyeDropperColorPickerPropertyEditor : DataEditor, IValueSchemaProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EyeDropperColorPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    public EyeDropperColorPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(string);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("string", "null"),
        ["pattern"] = "^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$",
        ["description"] = "Hex color value (e.g., #FF0000 or #FF0000FF with alpha)",
    };
}
