using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.RelationType}")]
[ApiExplorerSettings(GroupName = "Relation Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessRelationTypes)]
// NOTE: at the moment relation types aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class RelationTypeTreeControllerBase : NamedEntityTreeControllerBase<NamedEntityTreeItemResponseModel>
{
    public RelationTypeTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.RelationType;

    protected IEnumerable<NamedEntityTreeItemResponseModel> MapTreeItemViewModels(Guid? parentKey, IEnumerable<IRelationType> relationTypes)
        => relationTypes.Select(relationType => new NamedEntityTreeItemResponseModel
        {
            Name = relationType.Name!,
            Id = relationType.Key,
            Type = Constants.UdiEntityType.RelationType,
            HasChildren = false,
            Parent = parentKey.HasValue
                ? new ReferenceByIdModel
                {
                    Id = parentKey.Value
                }
                : null
        });
}
