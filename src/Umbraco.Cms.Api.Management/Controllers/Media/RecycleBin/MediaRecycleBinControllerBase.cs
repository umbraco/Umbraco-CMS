using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Media}")]
[RequireMediaTreeRootAccess]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
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
