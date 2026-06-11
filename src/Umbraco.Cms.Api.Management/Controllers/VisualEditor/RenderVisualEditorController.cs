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
public class RenderVisualEditorController : VisualEditorControllerBase
{
    private readonly IVisualEditorRenderService _renderService;

    public RenderVisualEditorController(IVisualEditorRenderService renderService)
        => _renderService = renderService;

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
