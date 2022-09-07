using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Media.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Media}/tree")]
public class MediaTreeControllerBase : ContentTreeControllerBase<ContentTreeItemViewModel>
{
    public MediaTreeControllerBase(
        IEntityService entityService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, appCaches, backofficeSecurityAccessor)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Media;

    protected override ContentTreeItemViewModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        ContentTreeItemViewModel viewModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity is IMediaEntitySlim mediaEntitySlim)
        {
            viewModel.Icon = mediaEntitySlim.ContentTypeIcon ?? viewModel.Icon;
        }

        return viewModel;
    }

    // TODO: delete (faking start node setup for unlimited editor)
    protected override int[] GetUserStartNodeIds() => new[] { -1 };

    protected override string[] GetUserStartNodePaths() => Array.Empty<string>();

    // TODO: use these implementations instead of the dummy ones above
    // protected override int[] GetUserStartNodeIds()
    //     => BackofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .CalculateMediaStartNodeIds(EntityService, AppCaches)
    //        ?? Array.Empty<int>();
    //
    // protected override string[] GetUserStartNodePaths()
    //     => BackofficeSecurityAccessor
    //            .BackOfficeSecurity?
    //            .CurrentUser?
    //            .GetMediaStartNodePaths(EntityService, AppCaches)
    //        ?? Array.Empty<string>();
}
