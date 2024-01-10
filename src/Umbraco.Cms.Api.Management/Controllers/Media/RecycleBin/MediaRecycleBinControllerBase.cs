using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Media}")]
[RequireMediaTreeRootAccess]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessMedia)]
public class MediaRecycleBinControllerBase : RecycleBinControllerBase<MediaRecycleBinItemResponseModel>
{
    private readonly IMediaPresentationModelFactory _mediaPresentationModelFactory;

    public MediaRecycleBinControllerBase(IEntityService entityService, IMediaPresentationModelFactory mediaPresentationModelFactory)
        : base(entityService)
        => _mediaPresentationModelFactory = mediaPresentationModelFactory;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override int RecycleBinRootId => Constants.System.RecycleBinMedia;

    protected override MediaRecycleBinItemResponseModel MapRecycleBinViewModel(Guid? parentKey, IEntitySlim entity)
    {
        MediaRecycleBinItemResponseModel responseModel = base.MapRecycleBinViewModel(parentKey, entity);

        if (entity is IMediaEntitySlim mediaEntitySlim)
        {
            responseModel.Variants = _mediaPresentationModelFactory.CreateVariantsItemResponseModels(mediaEntitySlim);
            responseModel.MediaType = _mediaPresentationModelFactory.CreateMediaTypeReferenceResponseModel(mediaEntitySlim);
        }

        return responseModel;
    }
}
