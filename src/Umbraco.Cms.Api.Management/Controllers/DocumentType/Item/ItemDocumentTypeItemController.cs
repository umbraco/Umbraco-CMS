using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

/// <summary>
/// API controller responsible for managing individual document type items within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class ItemDocumentTypeItemController : DocumentTypeItemControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.DocumentType.Item.ItemDocumentTypeItemController"/> class.
    /// </summary>
    /// <param name="contentTypeService">An instance of <see cref="IContentTypeService"/> used to manage content types.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between models.</param>
    public ItemDocumentTypeItemController(IContentTypeService contentTypeService, IUmbracoMapper mapper)
    {
        _contentTypeService = contentTypeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a collection of document type items corresponding to the specified IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of unique identifiers for the document type items to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with the collection of matching document type items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document type items.")]
    [EndpointDescription("Gets a collection of document type items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<DocumentTypeItemResponseModel>()));
        }

        IEnumerable<IContentType> contentTypes = _contentTypeService.GetMany(ids);
        List<DocumentTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IContentType, DocumentTypeItemResponseModel>(contentTypes);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
