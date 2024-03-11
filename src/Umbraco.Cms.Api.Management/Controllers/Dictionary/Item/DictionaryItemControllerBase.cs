using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDictionary)]
public class DictionaryItemControllerBase : ManagementApiControllerBase
{
}
