using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.MemberType)]
[ApiExplorerSettings(GroupName = "Member Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMembersOrMemberTypes)]
public abstract class MemberTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult OperationStatusResult(ContentTypeOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeOperationStatusResult(status, "member");

    protected IActionResult StructureOperationStatusResult(ContentTypeStructureOperationStatus status)
        => DocumentTypeControllerBase.ContentTypeStructureOperationStatusResult(status, "member");
}
