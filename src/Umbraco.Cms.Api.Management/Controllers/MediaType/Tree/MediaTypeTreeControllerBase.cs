using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MediaType}")]
[ApiExplorerSettings(GroupName = "Media Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessMediaTypes)]
public class MediaTypeTreeControllerBase : FolderTreeControllerBase<MediaTypeTreeItemResponseModel>
{
    private readonly IMediaTypeService _mediaTypeService;

    public MediaTypeTreeControllerBase(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService) =>
        _mediaTypeService = mediaTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MediaType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.MediaTypeContainer;

    protected override MediaTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var mediaTypes = _mediaTypeService
            .GetAll(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            MediaTypeTreeItemResponseModel responseModel = MapTreeItemViewModel(parentKey, entity);
            if (mediaTypes.TryGetValue(entity.Id, out IMediaType? mediaType))
            {
                responseModel.Icon = mediaType.Icon ?? responseModel.Icon;
            }

            return responseModel;
        }).ToArray();
    }
}
