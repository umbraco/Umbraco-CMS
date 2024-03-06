using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Filter;

[ApiExplorerSettings(GroupName = "Data Type")]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/{Constants.UdiEntityType.DataType}")]
// This auth policy might become problematic, as when getting DataTypes on Media types, you don't need access to the document tree.
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public abstract class DataTypeFilterControllerBase : ManagementApiControllerBase
{
}
