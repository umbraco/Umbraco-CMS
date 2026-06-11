namespace Umbraco.Cms.Api.Management.ViewModels.VisualEditor;

/// <summary>
/// The rendered HTML for a visual editor preview render request.
/// </summary>
public class VisualEditorRenderResponseModel
{
    /// <summary>Gets or sets the rendered page HTML.</summary>
    public required string Html { get; set; }
}
