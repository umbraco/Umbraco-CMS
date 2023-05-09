using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MemberGroup}")]
[ApiExplorerSettings(GroupName = "Member Group")]
public class MemberGroupTreeControllerBase : EntityTreeControllerBase<EntityTreeItemResponseModel>
{
    public MemberGroupTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberGroup;

    protected override EntityTreeItemResponseModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        EntityTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentKey, entity);
        responseModel.Icon = Constants.Icons.MemberGroup;
        return responseModel;
    }
}
