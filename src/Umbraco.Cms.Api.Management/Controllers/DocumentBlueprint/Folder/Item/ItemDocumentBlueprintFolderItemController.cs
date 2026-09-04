using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder.Item;

/// <summary>
/// API controller responsible for retrieving document blueprint folder items by their identifiers.
/// </summary>
[ApiVersion("1.0")]
public class ItemDocumentBlueprintFolderItemController : DocumentBlueprintFolderItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDocumentBlueprintFolderItemController"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="umbracoMapper">Mapper for converting domain models to view models.</param>
    public ItemDocumentBlueprintFolderItemController(
        IEntityService entityService,
        IUmbracoMapper umbracoMapper)
    {
        _entityService = entityService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Gets a collection of document blueprint folder items identified by the provided identifiers.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">The set of unique identifiers of the document blueprint folder items to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the collection of document blueprint folder items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FolderItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document blueprint folder items.")]
    [EndpointDescription("Gets a collection of document blueprint folder items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<FolderItemResponseModel>()));
        }

        IEnumerable<IEntitySlim> containers = _entityService
            .GetAll([UmbracoObjectTypes.DocumentBlueprintContainer], ids.ToArray());

        List<FolderItemResponseModel> responseModels = _umbracoMapper.MapEnumerable<IEntitySlim, FolderItemResponseModel>(containers);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
