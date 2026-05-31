using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller responsible for managing and retrieving URLs for documents within the Umbraco CMS.
/// Provides endpoints for operations related to document URLs.
/// </summary>
[ApiVersion("1.0")]
public class DocumentUrlController : DocumentControllerBase
{
    private readonly IContentService _contentService;
    private readonly IDocumentUrlFactory _documentUrlFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.DocumentUrlController"/> class.
    /// </summary>
    /// <param name="contentService">An instance of <see cref="IContentService"/> used to manage content operations.</param>
    /// <param name="documentUrlFactory">An instance of <see cref="IDocumentUrlFactory"/> used to generate document URLs.</param>
    public DocumentUrlController(
        IContentService contentService,
        IDocumentUrlFactory documentUrlFactory)
    {
        _contentService = contentService;
        _documentUrlFactory = documentUrlFactory;
    }

    /// <summary>
    /// Retrieves the URLs for the documents identified by the specified set of IDs.
    /// </summary>
    /// <param name="ids">A set of document IDs for which to retrieve URLs.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a collection of URL information for each requested document.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("urls")]
    [ProducesResponseType(typeof(IEnumerable<DocumentUrlInfoResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets URLs for a document.")]
    [EndpointDescription("Gets the URLs for the document identified by the provided Id.")]
    public async Task<IActionResult> GetUrls([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IContent> items = _contentService.GetByIds(ids);

        return Ok(await _documentUrlFactory.CreateUrlSetsAsync(items));
    }
}
