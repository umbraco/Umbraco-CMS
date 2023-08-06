using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MemberType}")]
[ApiExplorerSettings(GroupName = "Member Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessMemberTypes)]
public class MemberTypeTreeControllerBase : EntityTreeControllerBase<EntityTreeItemResponseModel>
{
    public MemberTypeTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberType;

    protected override EntityTreeItemResponseModel MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        EntityTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentKey, entity);
        return responseModel;
    }
}
