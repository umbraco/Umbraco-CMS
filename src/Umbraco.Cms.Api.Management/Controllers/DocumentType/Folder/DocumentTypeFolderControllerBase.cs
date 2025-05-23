using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DocumentType}/folder")]
[ApiExplorerSettings(GroupName = "Document Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public abstract class DocumentTypeFolderControllerBase : FolderManagementControllerBase<IContentType>
{
    protected DocumentTypeFolderControllerBase(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeContainerService contentTypeContainerService)
        : base(backOfficeSecurityAccessor, contentTypeContainerService)
    {
    }
}
