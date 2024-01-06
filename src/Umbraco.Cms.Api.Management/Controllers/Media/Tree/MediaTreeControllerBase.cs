using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessForMediaTree)]
public class MediaTreeControllerBase : UserStartNodeTreeControllerBase<MediaTreeItemResponseModel>
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IMediaPresentationModelFactory _mediaPresentationModelFactory;

    public MediaTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationModelFactory mediaPresentationModelFactory)
        : base(entityService, userStartNodeEntitiesService, dataTypeService)
    {
        _appCaches = appCaches;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _mediaPresentationModelFactory = mediaPresentationModelFactory;
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.SortOrder));

    protected override MediaTreeItemResponseModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        MediaTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity is IMediaEntitySlim mediaEntitySlim)
        {
            responseModel.MediaType = _mediaPresentationModelFactory.CreateMediaTypeReferenceResponseModel(mediaEntitySlim);
            responseModel.IsTrashed = entity.Trashed;
            responseModel.Id = entity.Key;
        }

        return responseModel;
    }

    // TODO: delete these (faking start node setup for unlimited editor)
    protected override int[] GetUserStartNodeIds() => new[] { -1 };

    protected override string[] GetUserStartNodePaths() => Array.Empty<string>();

    // TODO: use these implementations instead of the dummy ones above once we have backoffice auth in place
    // protected override int[] GetUserStartNodeIds()
    //     => _backofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .CalculateMediaStartNodeIds(EntityService, _appCaches)
    //        ?? Array.Empty<int>();
    //
    // protected override string[] GetUserStartNodePaths()
    //     => _backofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .GetMediaStartNodePaths(EntityService, _appCaches)
    //        ?? Array.Empty<string>();
}
