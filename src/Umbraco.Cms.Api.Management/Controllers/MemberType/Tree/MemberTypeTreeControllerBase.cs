using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MemberType}")]
[ApiExplorerSettings(GroupName = "Member Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMembersOrMemberTypes)]
public class MemberTypeTreeControllerBase : NamedEntityTreeControllerBase<MemberTypeTreeItemResponseModel>
{
    private readonly IMemberTypeService _memberTypeService;

    public MemberTypeTreeControllerBase(IEntityService entityService, IMemberTypeService memberTypeService)
        : base(entityService) =>
        _memberTypeService = memberTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberType;

    protected override MemberTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var memberTypes = _memberTypeService
            .GetMany(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            MemberTypeTreeItemResponseModel responseModel = MapTreeItemViewModel(parentKey, entity);
            if (memberTypes.TryGetValue(entity.Id, out IMemberType? memberType))
            {
                responseModel.Icon = memberType.Icon ?? responseModel.Icon;
            }

            return responseModel;
        }).ToArray();
    }
}
