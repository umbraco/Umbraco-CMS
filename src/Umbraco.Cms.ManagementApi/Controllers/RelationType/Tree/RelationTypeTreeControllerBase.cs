using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.RelationType.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.RelationType}/tree")]
// NOTE: at the moment relation types aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class RelationTypeTreeControllerBase : TreeControllerBase<TreeItemViewModel>
{
    public RelationTypeTreeControllerBase(IEntityService entityService, IRelationService relationService)
        : base(entityService) =>
        RelationService = relationService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.RelationType;

    protected IRelationService RelationService { get; }

    protected TreeItemViewModel[] MapTreeItemViewModels(Guid? parentKey, IRelationType[] relationTypes)
        => relationTypes.Select(relationType => new TreeItemViewModel
        {
            Icon = "icon-trafic",
            Name = relationType.Name!,
            Key = relationType.Key,
            Type = Constants.UdiEntityType.RelationType,
            HasChildren = false,
            IsContainer = false,
            ParentKey = parentKey
        }).ToArray();
}
