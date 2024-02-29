using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Filter;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/{Constants.UdiEntityType.DataType}")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public abstract class DataTypeFilterControllerBase : ManagementApiControllerBase
{
}
