using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MemberGroup}")]
[ApiExplorerSettings(GroupName = "Member Group")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberGroups)]
public class MemberGroupTreeControllerBase : NamedEntityTreeControllerBase<NamedEntityTreeItemResponseModel>
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public MemberGroupTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public MemberGroupTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>())
    {
    }

    public MemberGroupTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap)
        : base(entityService, flagProviders, entitySearchService, idKeyMap)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberGroup;
    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.Unknown;
}
