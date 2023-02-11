using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.MemberGroup.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberGroup}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.MemberGroup))]
public class MemberGroupTreeControllerBase : EntityTreeControllerBase<EntityTreeItemViewModel>
{
    public MemberGroupTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberGroup;

    protected override EntityTreeItemViewModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        EntityTreeItemViewModel viewModel = base.MapTreeItemViewModel(parentKey, entity);
        viewModel.Icon = Constants.Icons.MemberGroup;
        return viewModel;
    }
}
