using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentDraftBranch;

/// <summary>
/// Serves as the base controller for handling operations related to named draft branches of documents within the
/// Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Document}/{{id:guid}}/branch")]
[ApiExplorerSettings(GroupName = $"{nameof(Constants.UdiEntityType.Document)} Draft Branch")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public abstract class DocumentDraftBranchControllerBase : ContentDraftBranchControllerBase
{
    /// <inheritdoc />
    protected override string EntityName => "document";
}
