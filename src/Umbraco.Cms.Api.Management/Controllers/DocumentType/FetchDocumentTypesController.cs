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
public class FetchDocumentTypesController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FetchDocumentTypesController"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="umbracoMapper">The presentation model mapper.</param>
    public FetchDocumentTypesController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost("fetch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FetchResponseModel<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Fetch(CancellationToken cancellationToken, FetchRequestModel requestModel)
    {
        Guid[] ids = [.. requestModel.Ids.Select(x => x.Id).Distinct()];

        if (ids.Length == 0)
        {
            return Ok(new FetchResponseModel<DocumentTypeResponseModel>());
        }

        IEnumerable<IContentType> contentTypes = _contentTypeService.GetMany(ids);

        List<IContentType> ordered = OrderByRequestedIds(contentTypes, ids);

        var responseModels = ordered.Select(ct => _umbracoMapper.Map<DocumentTypeResponseModel>(ct)!).ToList();

        return Ok(new FetchResponseModel<DocumentTypeResponseModel>
        {
            Total = responseModels.Count,
            Items = responseModels,
        });
    }
}
