using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.DataType}")]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public class DatatypeItemControllerBase : ManagementApiControllerBase
{
}
