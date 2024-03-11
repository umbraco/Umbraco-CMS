using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public class DocumentItemControllerBase : ManagementApiControllerBase
{
}
