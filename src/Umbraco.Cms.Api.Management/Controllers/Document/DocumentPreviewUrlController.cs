using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class DocumentPreviewUrlController : DocumentControllerBase
{
    private readonly IContentService _contentService;
    private readonly IDocumentUrlFactory _documentUrlFactory;

    public DocumentPreviewUrlController(
        IContentService contentService,
        IDocumentUrlFactory documentUrlFactory)
    {
        _contentService = contentService;
        _documentUrlFactory = documentUrlFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/preview-url")]
    [ProducesResponseType(typeof(DocumentUrlInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets the preview URL for a document.")]
    [EndpointDescription("Gets the preview URL for the document identified by the provided Id.")]
    public async Task<IActionResult> GetPreviewUrl(Guid id, string providerAlias, string? culture, string? segment)
    {
        IContent? content = _contentService.GetById(id);
        if (content is null)
        {
            return NotFound(new ProblemDetailsBuilder()
                .WithTitle("Document not found")
                .WithDetail("The requested document did not exist.")
                .Build());
        }

        DocumentUrlInfo? previewUrlInfo = await _documentUrlFactory.GetPreviewUrlAsync(content, providerAlias, culture, segment);
        if (previewUrlInfo is null)
        {
            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("No preview URL for document")
                .WithDetail("Failed to produce a preview URL for the requested document.")
                .Build());
        }

        return Ok(previewUrlInfo);
    }
}
