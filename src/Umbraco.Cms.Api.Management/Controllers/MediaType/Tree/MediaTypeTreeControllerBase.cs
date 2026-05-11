using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service used for managing and retrieving entities in the system.</param>
    /// <param name="mediaTypeService">Service used for managing and retrieving media types.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public MediaTypeTreeControllerBase(IEntityService entityService, IMediaTypeService mediaTypeService)
        : this(
            entityService,
            StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
            mediaTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeTreeControllerBase"/> class with the specified services.
    /// </summary>
    /// <param name="entityService">The service used for entity operations.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="mediaTypeService">The service used for managing media types.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public MediaTypeTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders, IMediaTypeService mediaTypeService)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>(),
            mediaTypeService)
    {
    }

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

    protected override MediaTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var mediaTypes = _mediaTypeService
            .GetMany(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            MediaTypeTreeItemResponseModel responseModel = MapTreeItemViewModel(parentKey, entity);
            if (mediaTypes.TryGetValue(entity.Id, out IMediaType? mediaType))
            {
                responseModel.Icon = mediaType.Icon ?? responseModel.Icon;
                responseModel.IsDeletable = mediaType.IsSystemMediaType() is false;
            }

            return responseModel;
        }).ToArray();
    }
}
