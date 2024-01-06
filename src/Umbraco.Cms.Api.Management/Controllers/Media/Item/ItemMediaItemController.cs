using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

[ApiVersion("1.0")]
public class ItemMediaItemController : MediaItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IMediaPresentationModelFactory _mediaPresentationModelFactory;

    public ItemMediaItemController(IEntityService entityService, IMediaPresentationModelFactory mediaPresentationModelFactory)
    {
        _entityService = entityService;
        _mediaPresentationModelFactory = mediaPresentationModelFactory;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IMediaEntitySlim> media = _entityService
            .GetAll(UmbracoObjectTypes.Media, ids.ToArray())
            .OfType<IMediaEntitySlim>();

        IEnumerable<MediaItemResponseModel> responseModels = media.Select(_mediaPresentationModelFactory.CreateItemResponseModel);
        return await Task.FromResult(Ok(responseModels));
    }
}
