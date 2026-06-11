using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.VisualEditor;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Api.Management.Controllers.VisualEditor;

/// <summary>
/// Renders a document's template with the visual editor's unsaved values for live preview.
/// </summary>
[ApiVersion("1.0")]
public class RenderVisualEditorController : VisualEditorControllerBase
{
    private readonly IVisualEditorRenderService _renderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderVisualEditorController"/> class.
    /// </summary>
    /// <param name="renderService">The <see cref="IVisualEditorRenderService"/> used to render document templates with visual editor overrides.</param>
    public RenderVisualEditorController(IVisualEditorRenderService renderService)
        => _renderService = renderService;

    /// <summary>
    /// Renders a document's template with the unsaved property values supplied by the visual editor.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="requestModel">The model containing the document key, culture, segment, and property value overrides to render.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="VisualEditorRenderResponseModel"/> with the rendered HTML on success.
    /// </returns>
    [HttpPost("render")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(VisualEditorRenderResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Renders a document with unsaved visual editor values.")]
    public async Task<IActionResult> Render(
        CancellationToken cancellationToken,
        VisualEditorRenderRequestModel requestModel)
    {
        var overrides = requestModel.Values
            .Select(v => new VisualEditorPropertyOverride(v.Alias, v.Value, v.Culture, v.Segment))
            .ToList();

        var html = await _renderService.RenderAsync(
            requestModel.Unique,
            requestModel.Culture,
            requestModel.Segment,
            overrides);

        return Ok(new VisualEditorRenderResponseModel { Html = html });
    }
}
