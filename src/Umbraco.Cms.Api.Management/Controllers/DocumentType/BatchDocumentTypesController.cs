using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Provides an API controller for retrieving the full details for multiple document types by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchDocumentTypesController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchDocumentTypesController"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="umbracoMapper">The presentation model mapper.</param>
    public BatchDocumentTypesController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple document types.")]
    [EndpointDescription("Gets multiple document types identified by the provided Ids.")]
    public async Task<IActionResult> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];

        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<DocumentTypeResponseModel>());
        }

        IEnumerable<IContentType> contentTypes = _contentTypeService.GetMany(requestedIds);

        List<IContentType> ordered = OrderByRequestedIds(contentTypes, requestedIds);

        var responseModels = ordered.Select(ct => _umbracoMapper.Map<DocumentTypeResponseModel>(ct)!).ToList();

        return Ok(new BatchResponseModel<DocumentTypeResponseModel>
        {
            Total = responseModels.Count,
            Items = responseModels,
        });
    }
}
