using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

[VersionedApiBackOfficeRoute( $"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DocumentType}")]
[ApiExplorerSettings(GroupName = "Document Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class DocumentTypeItemControllerBase : ManagementApiControllerBase
{
}
