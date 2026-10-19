using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

/// <summary>
/// Serves as the base controller for media type tree management in the Umbraco CMS API,
/// providing shared functionality for derived media type tree controllers.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MediaType}")]
[ApiExplorerSettings(GroupName = "Media Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class MediaTypeTreeControllerBase : FolderTreeControllerBase<MediaTypeTreeItemResponseModel>
{
    private readonly IMediaTypeService _mediaTypeService;

    public MediaTypeTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IMediaTypeService mediaTypeService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap) =>
        _mediaTypeService = mediaTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MediaType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.MediaTypeContainer;

    protected override async Task<MediaTypeTreeItemResponseModel[]> MapTreeItemViewModelsAsync(Guid? parentKey, IEntitySlim[] entities)
    {
        var mediaTypes = _mediaTypeService
            .GetMany(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        IEnumerable<Task<MediaTypeTreeItemResponseModel>> tasks = entities.Select(async entity =>
        {
            MediaTypeTreeItemResponseModel responseModel = await MapTreeItemViewModelAsync(parentKey, entity);
            if (mediaTypes.TryGetValue(entity.Id, out IMediaType? mediaType))
            {
                responseModel.Icon = mediaType.Icon ?? responseModel.Icon;
                responseModel.IsDeletable = mediaType.IsSystemMediaType() is false;
            }

            return responseModel;
        });

        return await Task.WhenAll(tasks);
    }
}
