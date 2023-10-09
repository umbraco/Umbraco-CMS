using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

[ApiVersion("1.0")]
public class ItemMediaItemController : MediaItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IUserStartNodeEntitiesService _userStartNodeEntitiesService;
    private readonly IUmbracoMapper _mapper;
    private readonly IMediaPresentationModelFactory _mediaPresentationModelFactory;

    public ItemMediaItemController(IEntityService entityService, IDataTypeService dataTypeService, IUserStartNodeEntitiesService userStartNodeEntitiesService, IUmbracoMapper mapper, IMediaPresentationModelFactory mediaPresentationModelFactory)
    {
        _entityService = entityService;
        _dataTypeService = dataTypeService;
        _userStartNodeEntitiesService = userStartNodeEntitiesService;
        _mapper = mapper;
        _mediaPresentationModelFactory = mediaPresentationModelFactory;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids, Guid? dataTypeId = null)
    {
        IEnumerable<IMediaEntitySlim> media = _entityService.GetAll(UmbracoObjectTypes.Media, ids.ToArray()).OfType<IMediaEntitySlim>();
        if (dataTypeId is not null)
        {
            if (_dataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeId.Value))
            {
                // FIXME: right now we're faking user id by just passing "-1"
                // We should use the backoffice security accessor once auth is in place.
                media = _userStartNodeEntitiesService.UserAccessEntities(media, new[] {"-1"}).OfType<IMediaEntitySlim>();
            }
        }

        var responseModels = media.Select(entity => _mediaPresentationModelFactory.CreateItemResponseModel(entity)).ToList();

        return Ok(responseModels);
    }
}
