namespace Umbraco.Cms.Api.Management.ViewModels.VisualEditor;

/// <summary>
/// Request to render a document's template with unsaved visual editor values overlaid.
/// </summary>
public class VisualEditorRenderRequestModel
{
    /// <summary>Gets or sets the document key to render.</summary>
    public Guid Unique { get; set; }

    /// <summary>Gets or sets the culture to render, or <c>null</c> for the default/invariant.</summary>
    public string? Culture { get; set; }

    /// <summary>Gets or sets the segment to render, or <c>null</c> for none.</summary>
    public string? Segment { get; set; }

    /// <summary>Gets or sets the unsaved property values to overlay onto the draft content.</summary>
    public IEnumerable<VisualEditorPropertyValueModel> Values { get; set; } = [];
}
