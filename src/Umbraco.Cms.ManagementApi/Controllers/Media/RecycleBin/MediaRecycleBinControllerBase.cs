using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.RecycleBin;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.Cms.ManagementApi.ViewModels.RecycleBin;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Media}/recycle-bin")]
[RequireMediaTreeRootAccess]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[OpenApiTag(nameof(Constants.UdiEntityType.Media))]
public class MediaRecycleBinControllerBase : RecycleBinControllerBase<RecycleBinItemViewModel>
{
    public MediaRecycleBinControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override int RecycleBinRootId => Constants.System.RecycleBinMedia;

    protected override RecycleBinItemViewModel MapRecycleBinViewModel(Guid? parentKey, IEntitySlim entity)
    {
        RecycleBinItemViewModel viewModel = base.MapRecycleBinViewModel(parentKey, entity);

        if (entity is IMediaEntitySlim mediaEntitySlim)
        {
            viewModel.Icon = mediaEntitySlim.ContentTypeIcon ?? viewModel.Icon;
        }

        return viewModel;
    }
}
