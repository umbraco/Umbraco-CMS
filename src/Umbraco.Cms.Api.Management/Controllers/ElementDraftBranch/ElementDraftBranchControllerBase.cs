using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.ElementDraftBranch;

/// <summary>
/// Serves as the base controller for handling operations related to named draft branches of elements within the
/// Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Element}/{{id:guid}}/branch")]
[ApiExplorerSettings(GroupName = $"{nameof(Constants.UdiEntityType.Element)} Draft Branch")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessElements)]
public abstract class ElementDraftBranchControllerBase : ContentDraftBranchControllerBase
{
    /// <inheritdoc />
    protected override string EntityName => "element";
}
