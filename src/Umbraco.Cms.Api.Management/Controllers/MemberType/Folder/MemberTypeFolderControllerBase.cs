using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Folder;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberType}/folder")]
[ApiExplorerSettings(GroupName = "Member Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public abstract class MemberTypeFolderControllerBase : FolderManagementControllerBase<IMemberType>
{
    protected MemberTypeFolderControllerBase(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeContainerService memberTypeContainerService)
        : base(backOfficeSecurityAccessor, memberTypeContainerService)
    {
    }
}
