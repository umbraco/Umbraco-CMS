namespace Umbraco.Cms.Api.Management.ViewModels.VisualEditor;

/// <summary>
/// A single unsaved property value submitted for a visual editor preview render.
/// </summary>
public class VisualEditorPropertyValueModel
{
    /// <summary>Gets or sets the property alias.</summary>
    public required string Alias { get; set; }

    /// <summary>Gets or sets the editor-format value (raw string for simple editors, JSON for complex editors).</summary>
    public object? Value { get; set; }

    /// <summary>Gets or sets the culture this value applies to, or <c>null</c> for invariant.</summary>
    public string? Culture { get; set; }

    /// <summary>Gets or sets the segment this value applies to, or <c>null</c> for none.</summary>
    public string? Segment { get; set; }
}
