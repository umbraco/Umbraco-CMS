﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.RelationType}")]
[ApiExplorerSettings(GroupName = "Relation Type")]
// NOTE: at the moment relation types aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class RelationTypeTreeControllerBase : EntityTreeControllerBase<EntityTreeItemResponseModel>
{
    public RelationTypeTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.RelationType;

    protected EntityTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IRelationType[] relationTypes)
        => relationTypes.Select(relationType => new EntityTreeItemResponseModel
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
