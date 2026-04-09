using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides API endpoints for generating and retrieving preview URLs for documents.
/// </summary>
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

    /// <summary>Retrieves the preview URL for a document by its unique identifier.</summary>
    /// <param name="id">The unique identifier (GUID) of the document.</param>
    /// <param name="providerAlias">The alias of the provider used to generate the preview URL.</param>
    /// <param name="culture">An optional culture code for the preview URL.</param>
    /// <param name="segment">An optional segment for the preview URL.</param>
    /// <returns>An <see cref="IActionResult"/> containing the preview URL information if found; otherwise, a <see cref="ProblemDetails"/> response indicating the error.</returns>
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
