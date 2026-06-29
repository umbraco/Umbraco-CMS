using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Item;

/// <summary>
/// Controller responsible for managing item partial views via the Umbraco CMS Management API.
/// </summary>
[ApiVersion("1.0")]
public class ItemPartialViewItemController : PartialViewItemControllerBase
{
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemPartialViewItemController"/> class.
    /// </summary>
    /// <param name="fileItemPresentationFactory">An instance of <see cref="IFileItemPresentationFactory"/> used to create file item presentations.</param>
    public ItemPartialViewItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<PartialViewItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of partial view items.")]
    [EndpointDescription("Gets a collection of partial view items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "path")] HashSet<string> paths)
    {
        if (paths.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<PartialViewItemResponseModel>()));
        }

        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<PartialViewItemResponseModel> responseModels = _fileItemPresentationFactory.CreatePartialViewItemResponseModels(paths);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
