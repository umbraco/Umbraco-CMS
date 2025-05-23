using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MediaType}/folder")]
[ApiExplorerSettings(GroupName = "Media Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public abstract class MediaTypeFolderControllerBase : FolderManagementControllerBase<IMediaType>
{
    protected MediaTypeFolderControllerBase(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeContainerService mediaTypeContainerService)
        : base(backOfficeSecurityAccessor, mediaTypeContainerService)
    {
    }
}
