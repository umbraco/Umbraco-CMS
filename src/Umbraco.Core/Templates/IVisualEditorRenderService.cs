using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
/// Renders a document's assigned template to an HTML string using the visual editor's unsaved values,
/// with property-access tracking enabled so the output carries <c>data-umb-*</c> annotations.
/// </summary>
public interface IVisualEditorRenderService
{
    /// <summary>
    /// Renders the document identified by <paramref name="documentKey"/> with the supplied unsaved
    /// <paramref name="overrides"/> overlaid. Returns the rendered HTML, or an empty string if the
    /// document or its template cannot be resolved.
    /// </summary>
    /// <param name="documentKey">The key of the document to render.</param>
    /// <param name="culture">The culture to render, or <c>null</c> for the default/invariant.</param>
    /// <param name="segment">The segment to render, or <c>null</c> for none.</param>
    /// <param name="overrides">The unsaved editor values to overlay onto the draft content.</param>
    /// <returns>The rendered page HTML, or an empty string if the document or template is unavailable.</returns>
    Task<string> RenderAsync(
        Guid documentKey,
        string? culture,
        string? segment,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides);
}
