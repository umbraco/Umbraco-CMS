using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

[VersionedApiBackOfficeRoute( $"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DocumentType}")]
[ApiExplorerSettings(GroupName = "Document Type")]
public class DocumentTypeItemControllerBase : ManagementApiControllerBase
{
}
