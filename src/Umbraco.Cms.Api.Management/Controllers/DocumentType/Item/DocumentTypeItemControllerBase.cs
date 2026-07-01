using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

/// <summary>
/// Serves as the base controller for managing document type items in the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute( $"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DocumentType}")]
[ApiExplorerSettings(GroupName = "Document Type")]
public class DocumentTypeItemControllerBase : ManagementApiControllerBase
{
}
