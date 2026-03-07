using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

    /// <summary>
    /// Serves as the base controller for operations related to the media recycle bin in the Umbraco CMS.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Media}")]
[RequireMediaTreeRootAccess]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessMedia)]
public class MediaRecycleBinControllerBase : RecycleBinControllerBase<MediaRecycleBinItemResponseModel>
{
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaRecycleBinControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the media recycle bin.</param>
    /// <param name="mediaPresentationFactory">Factory responsible for creating media presentation models.</param>
    public MediaRecycleBinControllerBase(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService)
        => _mediaPresentationFactory = mediaPresentationFactory;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override Guid RecycleBinRootKey => Constants.System.RecycleBinMediaKey;

    protected override MediaRecycleBinItemResponseModel MapRecycleBinViewModel(Guid? parentKey, IEntitySlim entity)
    {
        MediaRecycleBinItemResponseModel responseModel = base.MapRecycleBinViewModel(parentKey, entity);

        if (entity is IMediaEntitySlim mediaEntitySlim)
        {
            responseModel.Variants = _mediaPresentationFactory.CreateVariantsItemResponseModels(mediaEntitySlim);
            responseModel.MediaType = _mediaPresentationFactory.CreateMediaTypeReferenceResponseModel(mediaEntitySlim);
        }

        return responseModel;
    }
}
