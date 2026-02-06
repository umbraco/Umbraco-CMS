using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Folder.Item;

[ApiVersion("1.0")]
public class ItemFolderItemController : FolderItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ItemFolderItemController(
        IEntityService entityService,
        IUmbracoMapper umbracoMapper)
    {
        _entityService = entityService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FolderItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<FolderItemResponseModel>()));
        }

        IEnumerable<IEntitySlim> elements = _entityService
            .GetAll([UmbracoObjectTypes.ElementContainer], ids.ToArray());

        List<FolderItemResponseModel> responseModels = _umbracoMapper.MapEnumerable<IEntitySlim, FolderItemResponseModel>(elements);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
