using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.RelationType.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.RelationType}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.RelationType))]
// NOTE: at the moment relation types aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class RelationTypeTreeControllerBase : EntityTreeControllerBase<EntityTreeItemViewModel>
{
    public RelationTypeTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.RelationType;

    protected EntityTreeItemViewModel[] MapTreeItemViewModels(Guid? parentKey, IRelationType[] relationTypes)
        => relationTypes.Select(relationType => new EntityTreeItemViewModel
        {
            Icon = Constants.Icons.RelationType,
            Name = relationType.Name!,
            Key = relationType.Key,
            Type = Constants.UdiEntityType.RelationType,
            HasChildren = false,
            IsContainer = false,
            ParentKey = parentKey
        }).ToArray();
}
