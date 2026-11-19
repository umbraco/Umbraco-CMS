namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a selectable option of a value list editor.
/// </summary>
public class ValueListOption
{
    /// <summary>
    ///     Gets or sets the value stored when the option is selected.
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    ///     Gets or sets the label shown for the option, or <c>null</c> to show the value. An empty label is treated as no label.
    /// </summary>
    public string? Label
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? null : value;
    }
}
