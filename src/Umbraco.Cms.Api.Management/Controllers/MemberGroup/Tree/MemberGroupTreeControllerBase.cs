using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Tree;

/// <summary>
/// Serves as the base controller for operations related to the member group tree in the management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MemberGroup}")]
[ApiExplorerSettings(GroupName = "Member Group")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberGroups)]
public class MemberGroupTreeControllerBase : NamedEntityTreeControllerBase<NamedEntityTreeItemResponseModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberGroupTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">The <see cref="IEntityService"/> instance used to perform member group operations.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public MemberGroupTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberGroupTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities within the system.</param>
    /// <param name="flagProviders">A collection of providers that supply additional flags or metadata for entities.</param>
    public MemberGroupTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberGroup;
}
