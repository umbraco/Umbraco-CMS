using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.MemberGroup.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberGroup}/tree")]
public class MemberGroupTreeControllerBase : TreeControllerBase<TreeItemViewModel>
{
    public MemberGroupTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberGroup;

    protected override TreeItemViewModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TreeItemViewModel viewModel = base.MapTreeItemViewModel(parentKey, entity);
        viewModel.Icon = Constants.Icons.MemberGroup;
        return viewModel;
    }
}
