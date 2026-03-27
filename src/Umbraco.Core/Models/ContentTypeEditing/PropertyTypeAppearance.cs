namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents the appearance settings for a property type in the backoffice.
/// </summary>
public class PropertyTypeAppearance
{
    /// <summary>
    ///     Gets or sets a value indicating whether the label should be displayed above the property editor.
    /// </summary>
    public bool LabelOnTop { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this property type is editable in the visual editor.
    /// </summary>
    public bool EditableInVisualEditor { get; set; }
}
